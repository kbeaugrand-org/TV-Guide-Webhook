using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Intents.GoogleDialogFlow
{
    public class GoogleDialogFlowResponse
    {
        [JsonIgnore]
        public GoogleDialogFlowRequest Request { get; }

        public Session Session => this.Request.Session;

        public Prompt Prompt { get; set; } = new Prompt();

        private GoogleDialogFlowResponse(GoogleDialogFlowRequest request)
        {
            this.Request = request;
        }

        public static GoogleDialogFlowResponse FromRequest(GoogleDialogFlowRequest request)
        {
            return new GoogleDialogFlowResponse(request);
        }
    }
}
