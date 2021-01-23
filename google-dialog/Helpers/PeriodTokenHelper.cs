using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Helpers
{
    public static class PeriodTokenHelper
    {
        public static DateTimeOffset GetDateTimeFromToken(string token)
        {
            switch (token)
            {
                case Constants.PeriodToken_Tonight:
                    return new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 19, 40, 0, DateTimeOffset.UtcNow.Offset);

                case Constants.PeriodToken_Tonight2ndPart:
                    return new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 20, 40, 0, DateTimeOffset.UtcNow.Offset);

                case Constants.PeriodToken_Now:
                default:
                    return DateTimeOffset.UtcNow;


            }
        }
    }
}
