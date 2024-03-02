using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OwlApi.Models
{
    public class TimeWindow
    {
        public int Id { get; set; }
        public int AvailabilityId { get; set; }
        public int BookableSlots { get; set; } = 1;
        public double BookablePallets { get; set; } = 0;

        [InverseProperty("TimeWindow")]
        public ICollection<TimeWindowFieldsFilter> TimeWindowFieldsFilter { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        [Column(TypeName = "jsonb")]
        public List<int> BookableWeekdays { get; set; }


        [ForeignKey("AvailabilityId")]
        public Availability Availability { get; set; }

        public bool MatchesFields(TimeSpan? Start, TimeSpan? End, List<ReservationField> fields)
        {
            if (this.Start != Start || this.End != End)
            {
                return false;
            }

            foreach (var filter in TimeWindowFieldsFilter)
            {
                var matchingField = fields.Where(f => f.Id == filter.ReservationFieldId).FirstOrDefault();
                if (matchingField == null)
                {
                    return false;
                }

                if (!filter.Values.Contains(matchingField.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
