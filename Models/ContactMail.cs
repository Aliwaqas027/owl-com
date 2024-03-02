using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class ContactMail
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? CompanyId { get; set; }
        public string Email { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
    }
}
