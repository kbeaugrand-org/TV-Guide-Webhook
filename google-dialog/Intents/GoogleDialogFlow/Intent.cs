using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Intents.GoogleDialogFlow
{
    public class IntentParameter
    {
        public string Original { get; set; }

        public dynamic Resolved { get; set; }

    }

    public class Intent
    {
        public string Name { get; set; }

        public Dictionary<string, IntentParameter> Params { get; set; }

        public string Query { get; set; }
    }
}
