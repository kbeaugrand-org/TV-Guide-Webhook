using Microsoft.WindowsAzure.Storage.Table;

namespace google_dialog
{
    public class TVChannel : TableEntity
    {
        public string DisplayName { get; set; }

        public string IconSrc { get; set; }
    }
}
