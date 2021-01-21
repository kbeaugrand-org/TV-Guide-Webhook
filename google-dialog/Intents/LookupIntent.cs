using google_dialog.Intents.GoogleDialogFlow;
using google_dialog.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace google_dialog.Intents
{
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

    public class LookupIntent : IntentBase
    {
        private TVProgramRepository programRepository = new TVProgramRepository();

        private TVChannelRepository channelRepository = new TVChannelRepository();

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

            var correspondingPrograms = await this.programRepository
                                                        .SearchPrograms(period, channel);

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
 
        private async Task<IEnumerable<LookupContent>> CreateLookupContent(IEnumerable<TVProgram> correspondingPrograms, ILogger log)
        {
            var programGroups = correspondingPrograms
                .GroupBy(c => c.Channel)
                .OrderByDescending(c => c.Select(x => x.StarRating)?.Max());

            var channels = await channelRepository.SearchChannels();

            return programGroups
                .Take(4)
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
                Title = $"Au programme de {response.Request.Intent.Params["period"].Original}:",
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
    }
}
