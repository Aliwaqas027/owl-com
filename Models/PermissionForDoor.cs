using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class PermissionForDoor
    {
        public int Id { get; set; }
        public int DoorId { get; set; }
        public int PermissionId { get; set; }

        [ForeignKey("DoorId")]
        public Door Door { get; set; }

        [ForeignKey("PermissionId")]
        public Permission Permission { get; set; }
    }
}
