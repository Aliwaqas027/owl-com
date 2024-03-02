using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class TimeWindowFieldsFilter
    {
        public int Id { get; set; }
        public int TimeWindowId { get; set; }
        public int ReservationFieldId { get; set; }

        [ForeignKey("TimeWindowId")]
        public TimeWindow TimeWindow { get; set; }

        [ForeignKey("ReservationFieldId")]
        public ReservationField ReservationField { get; set; }

        public string Value { get; set; }

        public string[] Values { get; set; }
    }
}
