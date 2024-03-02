using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OwlApi.Models
{

    public class Availability
    {
        public int Id { get; set; }
        public TimeSpan MinimumNotice { get; set; }
        public int GranularityMinutes { get; set; } = 15;
        public TimeSpan WorkTimeFrom { get; set; }
        public TimeSpan WorkTimeTo { get; set; }
        public TimeSpan MaxArrivalInacurracy { get; set; }


        [InverseProperty("Availability")]
        public ICollection<TimeWindow> TimeWindows { get; set; }

        public static TimeSpan ValidateTimeSpan(TimeSpan ts)
        {
            if (ts < TimeSpan.FromHours(0)) ts = TimeSpan.FromHours(0);
            else if (ts >= TimeSpan.FromHours(24)) ts = TimeSpan.FromHours(24) - TimeSpan.FromSeconds(1);
            return ts;
        }

        public bool Validate()
        {
            if (MinimumNotice < TimeSpan.Zero) return false;

            foreach (TimeWindow tw in TimeWindows)
            {
                tw.Start = ValidateTimeSpan(tw.Start);
                tw.End = ValidateTimeSpan(tw.End);
                if (tw.End < tw.Start) return false;
            }

            return true;
        }

        public static void Update(OwlApiContext _context, Availability oldA, Availability newA)
        {
            oldA.MinimumNotice = newA.MinimumNotice;
            oldA.GranularityMinutes = newA.GranularityMinutes;
            oldA.WorkTimeFrom = newA.WorkTimeFrom;
            oldA.WorkTimeTo = newA.WorkTimeTo;
            oldA.MaxArrivalInacurracy = newA.MaxArrivalInacurracy;
            newA.TimeWindows.OrderBy(tw => tw.Start);

            var oldTWs = oldA.TimeWindows;
            var newTWs = newA.TimeWindows;
            int min = Math.Min(oldTWs.Count, newTWs.Count);

            for (int i = 0; i < min; i++)
            {
                TimeWindow oldTW = oldTWs.ElementAt(i);
                TimeWindow tw = newTWs.ElementAt(i);
                oldTW.Start = tw.Start;
                oldTW.End = tw.End;
                oldTW.BookableSlots = tw.BookableSlots;
                oldTW.BookablePallets = tw.BookablePallets;
                oldTW.BookableWeekdays = tw.BookableWeekdays;
            }
            if (oldTWs.Count < newTWs.Count)
            {
                for (int i = min; i < newTWs.Count; i++)
                {
                    TimeWindow tw = newTWs.ElementAt(i);
                    TimeWindow oldTW = new TimeWindow()
                    {
                        Start = tw.Start,
                        End = tw.End,
                        AvailabilityId = oldA.Id,
                        BookableSlots = tw.BookableSlots,
                        BookablePallets = tw.BookablePallets,
                        BookableWeekdays = tw.BookableWeekdays
                    };
                    _context.TimeWindows.Add(oldTW);
                }
            }
            else
            {
                int deleteCount = oldTWs.Count - min;
                for (int i = 0; i < deleteCount; i++)
                {
                    TimeWindow oldTW = oldTWs.ElementAt(oldTWs.Count - 1);
                    oldTWs.Remove(oldTW);
                    _context.TimeWindows.Remove(oldTW);
                }
            }
        }
    }
}
