using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog
{
    public static class Constants
    {
        public static string StorageAccountConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        public static string ChannelsTableName = "channels";
        public static string ProgramsTableName = "programs";
    }
}
