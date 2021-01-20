using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Intents.GoogleDialogFlow
{
    public class ListItemResponseContent
    {
        public string Key { get; set; }
    }

    public class ListResponseContent: IResponseContent
    {
        public string Title { get; set; }

        public string SubTitle { get; set; }

        public List<ListItemResponseContent> Items { get; set; }
    }
}
