using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Helpers
{
    public static class DynamicHelper
    {
        public static int CovertToInt32(dynamic item, int defaultValue)
        {
            if (item == null)
            {
                return defaultValue;
            }

            return Convert.ToInt32(item);
        }
    }
}
