using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class ReservationFieldName
    {
        public int Id { get; set; }

        public string name { get; set; }

        [ForeignKey("fieldId")]
        public ReservationField reservationField { get; set; }

        [ForeignKey("languageId")]
        public AppLanguage language { get; set; }

        public int languageId { get; set; }

        public int fieldId { get; set; }

        public void unattach()
        {
            Id = 0;
        }
    }
}
