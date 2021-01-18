using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Extensions
{
    public static class DateExtensions
    {
        public static DateTimeOffset ParseDate(this string date)
        {
            return DateTimeOffset.ParseExact(date, "yyyyMMddHHmmss zzzz", new System.Globalization.CultureInfo("fr-FR"));
        }
    }
}

