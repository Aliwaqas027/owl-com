using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class AppLanguage
    {
        public int Id { get; set; }

        public string name { get; set; }

        public string localeId { get; set; }

        public string subdomain { get; set; }

        public int? CountryId { get; set; }

        [ForeignKey("CountryId")]
        public Country Country { get; set; }
    }
}
