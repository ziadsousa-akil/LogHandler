using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrangeTask
{
    public class TimeRange
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public TimeRange(TimeSpan Start, TimeSpan End)
        {
            this.Start = Start;
            this.End = End;
        }
        public bool isWithinRange(TimeSpan timeSpan)
        {
       
            if (Start <= End) return timeSpan >= Start && timeSpan <= End;
            else
            {
                DateTime StartDate, EndDate, ComparisonDate;
                StartDate = DateTime.Now;
                EndDate = DateTime.Now.AddDays(1);
                StartDate = new DateTime(StartDate.Year,StartDate.Month,StartDate.Day,Start.Hours,Start.Minutes,Start.Seconds);
                EndDate = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, End.Hours, End.Minutes, End.Seconds);

                if (timeSpan > Start) ComparisonDate = DateTime.Now;
                else ComparisonDate = DateTime.Now.AddDays(1);

                ComparisonDate = new DateTime(ComparisonDate.Year,ComparisonDate.Month,ComparisonDate.Day,timeSpan.Hours,timeSpan.Minutes,timeSpan.Seconds);

                return (ComparisonDate >= StartDate && ComparisonDate <= EndDate);
            }
        }
    }
}
