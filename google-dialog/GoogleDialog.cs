using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using google_dialog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sample
{
    public class Handler
    {
        public string Name { get; set; }
    }


    public class IntentParameter
    {
        public string Original { get; set; }

        public string Resolved { get; set; }

    }

    public class Intent
    {
        public string Name { get; set; }

        public Dictionary<string, IntentParameter> Params { get; set; }

        public string Query { get; set; }
    }

    public class Scene
    {
        public string Name { get; set; }

        public string SlotFillingStatus { get; set; }
    }

    public class Session
    {
        public string Id { get; set; }

        public Dictionary<string, string> Params { get; set; }
    }

    public class GoogleDialogFlowRequest
    {
        public Handler Handler { get; set; }

        public Intent Intent { get; set; }

        public Scene Scene { get; set; }

        public Session Session { get; set; }

    }

    public static class GoogleDialog
    {
        private static readonly JsonParser jsonParser =
            new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        [FunctionName("GoogleDialog")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            using (var reader = new StreamReader(req.Body))
            {
                var data = await reader.ReadToEndAsync();
                var request = JsonConvert.DeserializeObject<GoogleDialogFlowRequest>(data);

                switch (request.Intent.Name)
                {
                    case "actions.intent.MAIN":
                        return GetWelcomeResult(request, log);
                    case "LookUpIntent":
                        return await GetProgrammationResults(request, log);
                }

                return new BadRequestResult();
            }
        }

        private static async Task<IActionResult> GetProgrammationResults(GoogleDialogFlowRequest request, ILogger log)
        {
            string period = null;
            string channel = null;

            if (request.Intent.Params.ContainsKey("channel"))
            {
                channel = request.Intent.Params["channel"].Resolved;
            }

            if (request.Intent.Params.ContainsKey("period"))
            {
                period = request.Intent.Params["period"].Resolved;
            }

            if (String.IsNullOrWhiteSpace(period))
            {
                period = "tonight";
            }

            var correspondingPrograms = await SearchPrograms(log, period);
            correspondingPrograms = correspondingPrograms.FilterPeriod(period);

            if (!String.IsNullOrWhiteSpace(channel))
            {
                correspondingPrograms = correspondingPrograms.FilterChannel(channel);
            }

            if (!correspondingPrograms.Any())
            {
                return new OkObjectResult(new
                {
                    session = new
                    {
                        id = request.Session.Id,
                        @params = new
                        {
                            period = period
                        }
                    },
                    prompt = new
                    {
                        @override = false,
                        firstSimple = new
                        {
                            speech = "Désolé, je n'ai pas trouvé de programme qui pourrait vous correspondre."
                        }
                    }
                });
            }

            var programGroups = correspondingPrograms
                    .GroupBy(c => c.Channel)
                    .OrderByDescending(c => c.Select(x => x.StarRating)?.Max());

            var channels = await GetChannels(log);

            var items = programGroups
                            .Take(5)
                            .Select(p =>
                                {
                                    var programm = p.OrderBy(x => x.Start).First();
                                    var programChannel = channels.SingleOrDefault(c => c.RowKey == p.Key);

                                    var programStartDate = programm.Start.ToLocalTime();
                                    var programStartDateString = $"{programStartDate.Hour} heure";

                                    if (programStartDate.Minute != 0)
                                    {
                                        programStartDateString += $" {programStartDate.Minute}";
                                    }

                                    return new
                                    {
                                        Key = programm.RowKey,
                                        Title = $"Sur {programChannel.DisplayName}, à {programStartDateString}, {programm.Title}",
                                        programm.Description,
                                        programm.Category,
                                        programm.IconSrc,
                                        programm.RatingIconSrc                                 
                                    };
                                }).ToArray();

            var result = new
            {
                session = new
                {
                    id = request.Session.Id,
                    @params = new
                    {
                        period = period
                    },
                    typeOverrides = new[] {
                        new
                        {
                            name = "program",
                            typeOverrideMode = "TYPE_REPLACE",
                            synonym = new {
                                entries = items.Select(c => new {
                                    name = c.Key,
                                    synonyms = new []
                                    {
                                        c.Title
                                    },
                                    display =  new
                                    {
                                        title = c.Title,
                                        description = c.Description,
                                    }
                                }).ToArray()
                            }
                        }
                    }
                },
                prompt = new
                {
                    firstSimple = new
                    {
                        speech = "Voici les programmes qui pourraient vous interresser ce soir:"
                    },
                    content = new
                    {
                        list = new
                        {
                            items = items.Select(c => new {
                                key = c.Key
                            }),
                            title = "Programmes de ce soir"
                        }
                    }
                }
            };

            return new OkObjectResult(result);
        }

        private static IActionResult GetWelcomeResult(GoogleDialogFlowRequest request, ILogger log)
        {
            return new OkObjectResult(new
            {
                session = new
                {
                    id = request.Session.Id
                },
                prompt = new
                {
                    @override = false,
                    firstSimple = new
                    {
                        speech = "Bonjour, que puis-je faire pour vous ?"
                    }
                }
            });
        }

        private static async Task<IEnumerable<TVProgram>> SearchPrograms(ILogger log, string period)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(Constants.StorageAccountConnectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();

            var cloudTable = tableClient.GetTableReference(Constants.ProgramsTableName);

            var continuationToken = new TableContinuationToken();

            var date = DateTimeOffset.UtcNow;

            switch (period)
            {
                case "tonight":
                    date = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 22, 0, 0, DateTimeOffset.UtcNow.Offset);
                    break;

            }

            var tableQuery = new TableQuery<TVProgram>()
                    .Where(TableQuery.CombineFilters(
                        TableQuery.GenerateFilterConditionForDate(nameof(TVProgram.Start), QueryComparisons.GreaterThanOrEqual, date),
                        TableOperators.Or,
                        TableQuery.GenerateFilterConditionForDate(nameof(TVProgram.Stop), QueryComparisons.GreaterThanOrEqual, date)));

            var querySegment = await cloudTable.ExecuteQuerySegmentedAsync<TVProgram>(tableQuery, continuationToken);

            return querySegment.ToList();
        }

        private static async Task<IEnumerable<TVChannel>> GetChannels(ILogger log)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(Constants.StorageAccountConnectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();

            var cloudTable = tableClient.GetTableReference(Constants.ChannelsTableName);

            var continuationToken = new TableContinuationToken();

            var querySegment = await cloudTable.ExecuteQuerySegmentedAsync<TVChannel>(new TableQuery<TVChannel>(), continuationToken);

            return querySegment.ToList();
        }


        private static IEnumerable<TVProgram> FilterChannel(this IEnumerable<TVProgram> items, params string[] channels)
        {
            return items.Where(p => channels.Contains(p.Channel.Split(".").First()));
        }

        private static IEnumerable<TVProgram> FilterPeriod(this IEnumerable<TVProgram> items, string periodString)
        {
            var period = GetPeriod(periodString);

            return items
                    .Where(p => p.Start > period.Item1 &&
                                p.Stop < period.Item2);
        }

        private static Tuple<DateTimeOffset, DateTimeOffset> GetPeriod(string periodString)
        {
            if (periodString != "tonight")
            {
                return new Tuple<DateTimeOffset, DateTimeOffset>(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));
            }

            var startDate = new DateTimeOffset(DateTimeOffset.UtcNow.Year,
                                                DateTimeOffset.UtcNow.Month,
                                                DateTimeOffset.UtcNow.Day,
                                                20,
                                                30,
                                                0,
                                                TimeSpan.FromHours(1));

            return new Tuple<DateTimeOffset, DateTimeOffset>(
                startDate,
                startDate.AddHours(3));
        }
    }
}
