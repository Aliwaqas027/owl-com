using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class DoorFieldsFilter
    {
        public int Id { get; set; }
        public int DoorId { get; set; }
        public int ReservationFieldId { get; set; }

        [ForeignKey("DoorId")]
        public Door Door { get; set; }

        [ForeignKey("ReservationFieldId")]
        public ReservationField ReservationField { get; set; }

        public string Value { get; set; }

        public string[] Values { get; set; }
    }
}
