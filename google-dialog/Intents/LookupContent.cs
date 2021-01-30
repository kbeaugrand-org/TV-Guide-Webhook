using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog
{
    public class LookupContent
    {
        public string Key { get; set; }

        public string Title => $"Sur {this.ChanelName}, à {this.StartHour}, {this.ProgramTitle}";

        public string ProgramTitle { get; set; }

        public string ChanelName { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string IconSrc { get; set; }

        public string StartHour { get; set; }

        public string RatingIconSrc { get; set; }

        public string StarRating { get; set; }
    }
}
