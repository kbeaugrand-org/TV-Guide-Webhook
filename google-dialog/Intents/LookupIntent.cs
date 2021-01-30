using google_dialog.Helpers;
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
    public class LookupIntent : IntentBase
    {
        private TVProgramRepository programRepository = new TVProgramRepository();

        private TVChannelRepository channelRepository = new TVChannelRepository();

        public override async Task<IActionResult> Handle(GoogleDialogFlowRequest request, ILogger log)
        {
            var response = GoogleDialogFlowResponse.FromRequest(request);

            PopulateSessionParameters(response);

            var correspondingPrograms = await this.programRepository
                                                        .SearchPrograms(response.Session.Params["dateTime"],
                                                                        response.Session.Params["channel"]) as IEnumerable<TVProgram>;

            if (!correspondingPrograms.Any())
            {
                response.Prompt.FirstSimple = new Simple
                {
                    Speech = "Désolé, je n'ai pas trouvé de programme qui pourrait vous correspondre."
                };

                return new OkObjectResult(response);
            }

            var content = await CreateLookupContent(correspondingPrograms, log);

            PrepareRichContentAnswer(response, content, log);

            if (!response.Request.Intent.Params.ContainsKey("channel"))
            {
                await PrepareSpeechContentAnswer(response, content, log);
            }
            else
            {
                await PrepareSpeechContentAnswerForSingleChannel(response, content, log);
            }

            response.Scene.Next = new Scene
            {
                Name = "actions.scene.END_CONVERSATION"
            };

            return new OkObjectResult(response);
        }

        private void PopulateSessionParameters(GoogleDialogFlowResponse response)
        {
            DateTimeOffset dateTime = DateTimeOffset.UtcNow;

            if (response.Request.Intent.Params.ContainsKey("channel"))
            {
                response.Session.Params["channel"] = response.Request.Intent.Params["channel"].Resolved;
            }
            else
            {
                response.Session.Params["channel"] = null;
            }

            if (response.Request.Intent.Params.ContainsKey("dateTime"))
            {
                dateTime = new DateTimeOffset(
                     DynamicHelper.CovertToInt32(response.Request.Intent.Params["dateTime"].Resolved.year, DateTimeOffset.Now.Year),
                     DynamicHelper.CovertToInt32(response.Request.Intent.Params["dateTime"].Resolved.months, DateTimeOffset.Now.Month),
                     DynamicHelper.CovertToInt32(response.Request.Intent.Params["dateTime"].Resolved.day, DateTimeOffset.Now.Day),
                     DynamicHelper.CovertToInt32(response.Request.Intent.Params["dateTime"].Resolved.hours, DateTimeOffset.Now.Hour),
                     DynamicHelper.CovertToInt32(response.Request.Intent.Params["dateTime"].Resolved.minutes, DateTimeOffset.Now.Minute),
                     0,
                     DateTimeOffset.Now.Offset
                 );

                response.Session.Params["askedPeriod"] = $"à partir de {response.Request.Intent.Params["dateTime"].Original}";
            }
            else if (response.Request.Intent.Params.ContainsKey("dateTimeToken"))
            {
                response.Session.Params["askedPeriod"] = response.Request.Intent.Params["dateTimeToken"].Original;
                dateTime = PeriodTokenHelper.GetDateTimeFromToken(response.Request.Intent.Params["dateTimeToken"].Resolved);
            }
            else
            {
                response.Session.Params["askedPeriod"] = "maintenant";
            }

            response.Session.Params["dateTime"] = dateTime;
        }

        private async Task<IEnumerable<LookupContent>> CreateLookupContent(IEnumerable<TVProgram> correspondingPrograms, ILogger log)
        {
            var channels = await channelRepository
                    .SearchChannels();

            // Group programs by Channels
            var programGroups = correspondingPrograms
                                        .GroupBy(c => c.Channel);

            Dictionary<TVChannel, TVProgram> content = new Dictionary<TVChannel, TVProgram>();

            return programGroups
                .Select(p =>
                {
                    // In Each channel select the most important program in the period.
                    var program = p.OrderByDescending(x => x.Stop - x.Start)
                                      .ThenByDescending(x => x.StarRating)
                                      .First();

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
                        RatingIconSrc = program.RatingIconSrc,
                        StarRating = program.StarRating
                    };
                })
                .OrderByDescending(x => x.StarRating)
                .Take(4)
                .ToArray();
        }

        private void PrepareRichContentAnswer(GoogleDialogFlowResponse response, IEnumerable<LookupContent> items, ILogger log)
        {
            if (!response.Request.Device.Capabilities.Contains("RICH_RESPONSE"))
            {
                return;
            }

            response.Prompt.AddContent("list", new ListResponseContent
            {
                Title = $"Au programme {response.Session.Params["askedPeriod"]} :",
                Items = items.Select(c => new ListItemResponseContent { Key = c.Key }).ToList()
            });

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

        private async Task PrepareSpeechContentAnswerForSingleChannel(GoogleDialogFlowResponse response, IEnumerable<LookupContent> items, ILogger log)
        {
            var channel = (await this.channelRepository.SearchChannels())
                            .Single(channel => channel.RowKey == response.Request.Intent.Params["channel"].Resolved);

            var program = items.First();

            response.Prompt.FirstSimple = new Simple
            {
                Speech = $"{response.Session.Params["askedPeriod"]}, sur {channel.DisplayName} à {program.StartHour} il y a \"{program.ProgramTitle}.\""
            };

            response.Prompt.AddContent("list", new ListResponseContent
            {
                Title = $"{response.Session.Params["askedPeriod"]}, sur {channel.DisplayName}",
                Items = items.Select(c => new ListItemResponseContent { Key = c.Key }).ToList()
            });
        }

        private Task PrepareSpeechContentAnswer(GoogleDialogFlowResponse response, IEnumerable<LookupContent> items, ILogger log)
        {
            return Task.Factory.StartNew(() =>
            {
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

                response.Prompt.FirstSimple = new Simple
                {
                    Speech = $"Voici les programmes qui pourraient vous intéresser {response.Session.Params["askedPeriod"]} :"
                };
            });
        }
    }
}
