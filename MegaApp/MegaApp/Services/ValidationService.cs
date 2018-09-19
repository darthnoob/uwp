using System;
using System.Text.RegularExpressions;
using mega;

namespace MegaApp.Services
{
    public static class ValidationService
    {
        /// <summary>
        /// Checks if a string is a valid email address.
        /// </summary>
        /// <param name="str">String to check.</param>
        /// <returns>TRUE if the string is a valid email address, FALSE in other case.</returns>
        public static bool IsValidEmail(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;

            try
            {
                // Return true if str is in valid e-mail format.
                return Regex.IsMatch(str,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase);
            }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// Calculate the strength of a password
        /// </summary>
        /// <param name="value">Password to calculate</param>
        /// <returns>Strength value of the input password</returns>
        public static MPasswordStrength CalculatePasswordStrength(string value)
        {
            return (MPasswordStrength) Enum.ToObject(
                typeof(MPasswordStrength),
                SdkService.MegaSdk.getPasswordStrength(value));
        }

        /// <summary>
        /// Check if a string contains only numeric digits
        /// </summary>
        /// <param name="str">String to check</param>
        /// <returns>TRUE if the string contains only numeric digits, FALSE in other case.</returns>
        public static bool IsDigitsOnly(string str)
        {
            try
            {
                foreach (char c in str)
                {
                    if (c < '0' || c > '9')
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, 
                    "Error checking if the string contains only numeric digits", e);
                return false;
            }            
        }
    }
}
