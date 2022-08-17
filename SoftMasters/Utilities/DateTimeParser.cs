using System.Globalization;
using System.Text.RegularExpressions;

namespace SoftMasters.test.Utilities
{
    public class DateTimeParser
    {
        private static readonly string dtformat = @"dd.MM.yyyy HH:mm:ss";
        private static readonly string dayReg = "^.{2}";
        private static readonly string monthReg = "(?<=^.{3}).{2}";
        private static readonly string yearReg = "(?<=^.{6}).{4}";
        private static readonly string hourReg = "(?<=^.{11}).{2}";
        private static readonly string minReg = "(?<=^.{14}).{2}";
        private static readonly string secReg = "(?<=^.{17}).{2}";
        CultureInfo cultureUS = CultureInfo.GetCultureInfo("en-US");


        public static DateTime Create(string stringDateTime)
        {

            var day = Int32.Parse(Regex.Match(stringDateTime, dayReg).ToString());
            var month = Int32.Parse(Regex.Match(stringDateTime, monthReg).ToString());
            var year = Int32.Parse(Regex.Match(stringDateTime, yearReg).ToString());
            var hour = Int32.Parse(Regex.Match(stringDateTime, hourReg).ToString());
            var minuts = Int32.Parse(Regex.Match(stringDateTime, minReg).ToString());
            var sec = Int32.Parse(Regex.Match(stringDateTime, secReg).ToString());
            var timekind = DateTimeKind.Unspecified;
            return new DateTime(year, month, day, hour, minuts, sec, timekind);
        }

        public static DateTime ParseFormat(string stringDateTime)
        {
            // 30.06.2019 14:50:00
            
            return DateTime.ParseExact(stringDateTime, dtformat, CultureInfo.InvariantCulture);
        }
        public static DateTime ParseCultureUS(string stringDateTime)
        {
            // 30.06.2019 14:50:00
            return new DateTime();
           // return DateTime.ParseExact(stringDateTime, cultureUS);
        }
    }
}
