using Microsoft.Extensions.Logging;
using google_dialog.Intents.GoogleDialogFlow;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace google_dialog.Intents
{
    public abstract class IntentBase
    {
        public abstract Task<IActionResult> Handle(GoogleDialogFlowRequest request, ILogger log);
    }
}
