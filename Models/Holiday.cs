using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class Holiday
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

    }
}
