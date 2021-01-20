using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Intents.GoogleDialogFlow
{

    public class Simple
    {
        public string Speech { get; set; }

        public string Text { get; set; }
    }

    public class Prompt
    {
        private Dictionary<string, IResponseContent> content;

        public IReadOnlyDictionary<string, IResponseContent> Content => content;

        public bool Override { get; set; } = false;

        public Simple FirstSimple { get; set; }

        public Simple LastSimple { get; set; }

        public void AddContent(string key, IResponseContent item)
        {
            this.content = this.content ?? new Dictionary<string, IResponseContent>();

            this.content.Add(key, item);
        }
    }
}
