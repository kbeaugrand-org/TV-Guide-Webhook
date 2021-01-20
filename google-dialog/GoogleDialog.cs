using google_dialog.Intents;
using google_dialog.Intents.GoogleDialogFlow;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Sample
{
    public static class GoogleDialog
    {
        [FunctionName("GoogleDialog")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            using (var reader = new StreamReader(req.Body))
            {
                var data = await reader.ReadToEndAsync();
                var request = JsonConvert.DeserializeObject<GoogleDialogFlowRequest>(data);

                IntentBase intent = null;

                switch (request.Intent.Name)
                {
                    case "actions.intent.MAIN":
                        intent = new WelcomeIntent();
                        break;
                    case "LookUpIntent":
                        intent = new LookupIntent();
                        break;
                    default:
                        return new BadRequestObjectResult($"Intent {request.Intent.Name} is not handled!");
                }

                return await intent.Handle(request, log);
            }
        }        
    }
}
