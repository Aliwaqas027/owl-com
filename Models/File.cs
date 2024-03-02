using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.Json.Serialization;

namespace OwlApi.Models
{
    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? UserId { get; set; }
        public int? CompanyId { get; set; }
        public int? ReservationId { get; set; }
        public int? RecurringReservationId { get; set; }
        public int? WarehouseId { get; set; }

        [JsonIgnore]
        public string Path { get; set; }

        public string PdfToken { get; set; }

        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; }

        [ForeignKey("RecurringReservationId")]
        public RecurringReservation RecurringReservation { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; }
        public int? CompanyAttachmentId { get; set; }

        [ForeignKey("CompanyAttachmentId")]
        public Company CompanyAttachment { get; set; }

        public int? WarehouseAttachmentId { get; set; }

        [ForeignKey("WarehouseAttachmentId")]
        public Warehouse WarehouseAttachment { get; set; }

        public int? DoorAttachmentId { get; set; }
        [ForeignKey("DoorAttachmentId")]
        public Door DoorAttachment { get; set; }

        public int? LanguageId { get; set; }

        [ForeignKey("LanguageId")]
        public AppLanguage Language { get; set; }


        public FileStream GetStream()
        {
            return System.IO.File.OpenRead(Path);
        }

        public string GetFileUrl()
        {
            return $"api/file/download/{Id}/{Name}";
        }

        public void Delete()
        {
            System.IO.File.Delete(Path);
        }
    }

    public class FileExcerptDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }

        public static FileExcerptDto FromFile(File file)
        {
            return new FileExcerptDto()
            {
                id = file.Id,
                name = file.Name,
                url = file.GetFileUrl(),
            };
        }
    }
}
