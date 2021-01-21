using google_dialog.Intents.GoogleDialogFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace google_dialog.Intents
{
    public class ChannelDetailsIntent : IntentBase
    {
        public override Task<IActionResult> Handle(GoogleDialogFlowRequest request, ILogger log)
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

            throw new NotImplementedException();
        }
    }
}
