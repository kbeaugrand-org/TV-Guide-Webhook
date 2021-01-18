using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog
{
    public class TVProgram : TableEntity 
    {
        public string Channel { get; set; }

        public DateTimeOffset Start { get; set; }

        public DateTimeOffset Stop { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int? Date { get; set; }

        public string? Category { get; set; }

        public string? IconSrc { get; set; }

        public string? Rating { get; set; }

        public string? RatingIconSrc { get; set; }

        public string? StarRating { get; set; }
    }
}
