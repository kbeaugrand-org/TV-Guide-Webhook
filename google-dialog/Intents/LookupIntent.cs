using google_dialog.Intents.GoogleDialogFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace google_dialog.Intents
{
    public class LookupIntent : IntentBase
    {
        public override async Task<IActionResult> Handle(GoogleDialogFlowRequest request, ILogger log)
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
            correspondingPrograms = FilterPeriod(correspondingPrograms, period);

            if (!String.IsNullOrWhiteSpace(channel))
            {
                correspondingPrograms = FilterChannel(correspondingPrograms, channel);
            }

            if (!correspondingPrograms.Any())
            {
                return GetEmptyResults(request);
            }

            var content = await CreateLookupContent(correspondingPrograms, log);

            var response = GoogleDialogFlowResponse.FromRequest(request);
            PrepareRichContentAnswer(response, content, log);
            PrepareSpeechContentAnswer(response, content, log);

            return new OkObjectResult(response);
        }

        public class LookupContent
        {
            public string Key { get; set; }

            public string Title => $"Sur {this.ChanelName}, à {this.StartHour}, {this.ProgramTitle}";

            public string ProgramTitle { get; set; }

            public string ChanelName { get; set; }

            public string Description { get; set; }

            public string Category { get; set; }

            public string IconSrc { get; set; }

            public string StartHour { get; set; }

            public string RatingIconSrc { get; set; }
        }

        private static async Task<IEnumerable<LookupContent>> CreateLookupContent(IEnumerable<TVProgram> correspondingPrograms, ILogger log)
        {
            var programGroups = correspondingPrograms
                .GroupBy(c => c.Channel)
                .OrderByDescending(c => c.Select(x => x.StarRating)?.Max());

            var channels = await GetChannels(log);

            return programGroups
                .Take(5)
                .Select(p =>
                {
                    var program = p.OrderBy(x => x.Start).First();
                    var programChanel = channels.SingleOrDefault(c => c.RowKey == p.Key);

                    var programStartDate = program.Start.ToLocalTime();
                    var programStartDateString = $"{programStartDate.Hour} heure";

                    if (programStartDate.Minute != 0)
                    {
                        programStartDateString += $" {programStartDate.Minute}";
                    }

                    return new LookupContent
                    {
                        Key = program.RowKey,
                        ProgramTitle = program.Title,
                        StartHour = programStartDateString,
                        ChanelName = programChanel.DisplayName,
                        Description = program.Description,
                        Category = program.Category,
                        IconSrc = program.IconSrc,
                        RatingIconSrc = program.RatingIconSrc
                    };
                }).ToArray();
        }

        private void PrepareRichContentAnswer(GoogleDialogFlowResponse response, IEnumerable<LookupContent> items, ILogger log)
        {
            response.Session.AddTypeOverride(new TypeOverride
            {
                Name = "prompt_program",
                TypeOverrideMode = "TYPE_REPLACE",
                Synonym = new TypeOverrideSynonym
                {
                    Entries = items.Select(c => new TypeOverrideSynonymEntry
                    {
                        Name = c.Key,
                        Synonyms = new string[]
                                    {
                                        c.ChanelName,
                                        c.ProgramTitle
                                    },
                        Display = new SynonymEntryDisplay
                        {
                            Title = c.Title,
                            Description = c.Description,
                            Image = new SynonymEntryDisplayImage
                            {
                                Url = c.IconSrc,
                                Alt = c.Title
                            }
                        }
                    }).ToList()
                }
            });
        }

        private void PrepareSpeechContentAnswer(GoogleDialogFlowResponse response, IEnumerable<LookupContent> items, ILogger log)
        {
            response.Prompt.FirstSimple = new Simple
            {
                Speech = $"Voici les programmes qui pourraient vous intéresser {(response.Request.Intent.Params.ContainsKey("period") ? response.Request.Intent.Params["period"].Original : "")}:"
            };

            if (!response.Request.Device.Capabilities.Contains("RICH_RESPONSE"))
            {
                StringBuilder responseBuilder = new StringBuilder();

                items.ToList()
                     .ForEach(c => responseBuilder.AppendLine($"{c.Category}, {c.Title}."));           

                response.Prompt.LastSimple = new Simple
                {
                    Speech = responseBuilder.ToString()
                };
            }
   
            response.Prompt.AddContent("list", new ListResponseContent
            {
                Title = $"Liste des programmes",
                Items = items.Select(c => new ListItemResponseContent {  Key = c.Key }).ToList()
            });
        }

        private IActionResult GetEmptyResults(GoogleDialogFlowRequest request)
        {
            return new OkObjectResult(new
            {
                session = new
                {
                    id = request.Session.Id,
                    @params = new
                    {
                        period = request.Intent.Params["period"].Resolved
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

        private static IEnumerable<TVProgram> FilterChannel(IEnumerable<TVProgram> items, params string[] channels)
        {
            return items.Where(p => channels.Contains(p.Channel.Split(".").First()));
        }

        private static IEnumerable<TVProgram> FilterPeriod(IEnumerable<TVProgram> items, string periodString)
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
