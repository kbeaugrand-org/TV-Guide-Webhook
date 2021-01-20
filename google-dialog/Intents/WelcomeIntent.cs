using google_dialog.Intents.GoogleDialogFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace google_dialog.Intents
{
    public class WelcomeIntent : IntentBase
    {
        public override Task<IActionResult> Handle(GoogleDialogFlowRequest request, ILogger log)
        {

            var response = GoogleDialogFlowResponse.FromRequest(request);

            response.Prompt.FirstSimple = new Simple
            {
                Speech = "Bonjour"
            };

            response.Prompt.Override = false;

            return Task.FromResult<IActionResult>(new OkObjectResult(response));
        }
    }
}
