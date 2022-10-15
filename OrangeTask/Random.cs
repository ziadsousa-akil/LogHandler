using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Generator = System.Random  ;
namespace LogHandler
{
    public class Random
    { 
        public static string IP
        {
            get
            {
                Generator random = new Generator();
                return $"{random.Next(1, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}";
            }
        }
        public static int Int(int LowerThreshold, int HigherThreshold)
        {
            Generator random = new Generator();
            return random.Next(LowerThreshold, HigherThreshold);
        }
        public static string Text(int Length, string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            Generator random = new Generator();
            return   new string(Enumerable.Repeat(Chars, Length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string Text(int Length, Regex Criteria, string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            string temp = Text(Length, Chars);
            while (!Criteria.IsMatch(temp)) temp = Text(Length, Chars);
            return temp;
        }
        public static DateTime Date(DateTime LowerThreshold, DateTime HigherThreshold)
        {
            Generator random = new Generator();
            return LowerThreshold.AddSeconds(random.Next(0, (int)(HigherThreshold - LowerThreshold).TotalSeconds));
        }
        public static DateTime Date(DateTime LowerThreshold, DateTime HigherThreshold, TimeRange timeRange)
        {
            Generator random = new Generator();
            var date = LowerThreshold.AddDays(random.Next(0, (int)(HigherThreshold - LowerThreshold).TotalDays));
            var Min = (timeRange.Start > timeRange.End) ? timeRange.End : timeRange.Start;
            var Max = (timeRange.Start > timeRange.End) ? timeRange.Start : timeRange.End;
 
            var dateTime = new DateTime(date.Year, date.Month, date.Day, timeRange.Start.Hours, timeRange.Start.Minutes, timeRange.Start.Seconds);
            int Seconds = random.Next(1, (int)(Max - Min).TotalSeconds - 60);
            dateTime = dateTime.AddSeconds(Seconds); 
            if (timeRange.isWithinRange(dateTime.TimeOfDay) == false) ;
            return dateTime;
        }
    }
}
