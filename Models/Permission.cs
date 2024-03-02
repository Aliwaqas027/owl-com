using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public enum PermissionStatus
    {
        Pending,
        Accepted,
        Declined
    }

    public enum PermissionType
    {
        ALL_DOORS,
        ONLY_SPECIFIC_DOORS,
        ONLY_TWO_PHASE
    }

    public class Permission
    {
        public int Id { get; set; }
        public int CarrierId { get; set; }
        public int WarehouseId { get; set; }

        public PermissionType Type { get; set; } = 0;

        [Column(TypeName = "smallint")]
        public PermissionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("CarrierId")]
        public User Carrier { get; set; }
        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; }

        [InverseProperty("Permission")]
        public ICollection<PermissionForDoor> PermissionsForDoor { get; set; }
    }
}
