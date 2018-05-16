using System;
using System.Globalization;

namespace MegaApp.Extensions
{
    static class DateAndTimeExtension
    {
        private static string DateFormat
        {
            get
            {
                var culture = CultureInfo.CurrentCulture;
                switch (culture.TwoLetterISOLanguageName)
                {
                    case null:
                        return culture.DateTimeFormat.ShortDatePattern;

                    case "zh":
                        return "yyyy/MM/dd";

                    default:
                        return "dd MMM yyyy";
                }
            }
        }

        public static string DateToString(this DateTime value)
        {
            return value == null ? string.Empty :
                value.Date.ToString(DateFormat, CultureInfo.CurrentCulture);
        }
    }
}
