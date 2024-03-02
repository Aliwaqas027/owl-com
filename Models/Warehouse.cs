using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OwlApi.Models
{
    public class Warehouse
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AvailabilityId { get; set; }
        public string Address { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public bool canCarrierEditReservation { get; set; } = false;
        public bool canCarrierDeleteReservation { get; set; } = false;
        public bool canCarrierCreateAnonymousReservation { get; set; } = false;

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [ForeignKey("CreatedById")]
        public User CreatedBy { get; set; }

        [InverseProperty("Warehouse")]
        public ICollection<Door> Doors { get; set; }

        [InverseProperty("Warehouse")]

        public ICollection<Permission> Permissions { get; set; }
        [ForeignKey("AvailabilityId")]
        public Availability Availability { get; set; }

        [InverseProperty("Warehouse")]
        public ICollection<ReservationField> ReservationFields { get; set; }

        [InverseProperty("ImportantFieldWarehouse")]
        public ICollection<ReservationField> ImportantFields { get; set; }

        [InverseProperty("Warehouse")]
        public File Image { get; set; }
    }

    public class WarehouseExcerptDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public CompanyDto company { get; set; }
        public UserExcerptDto createdBy { get; set; }
        public bool canCarrierEditReservation { get; set; }
        public bool canCarrierDeleteReservation { get; set; }
        public bool canCarrierCreateAnonymousReservation { get; set; }

        public static WarehouseExcerptDto FromWarehouse(Warehouse warehouse)
        {
            return new WarehouseExcerptDto()
            {
                id = warehouse.Id,
                name = warehouse.Name,
                company = CompanyDto.FromCompany(warehouse.Company),
                createdBy = UserExcerptDto.FromUser(warehouse.CreatedBy),
                canCarrierDeleteReservation = warehouse.canCarrierDeleteReservation,
                canCarrierEditReservation = warehouse.canCarrierEditReservation,
                canCarrierCreateAnonymousReservation = warehouse.canCarrierCreateAnonymousReservation,
            };
        }
    }

    public class WarehouseCompanyListItem
    {
        public WarehouseCompany company { get; set; }
        public List<WarehouseListItem> warehouses { get; set; }
    }

    public class WarehouseCompany : CompanyDto
    {
        public bool amIParticipant { get; set; }

        public static WarehouseCompany FromCompany(Company company, bool amIParticipant)
        {
            return new WarehouseCompany()
            {
                Id = company.Id,
                Address = company.Address,
                Phone = company.Phone,
                ContactPerson = company.ContactPerson,
                AuthServerName = company.RealmName,
                DisableTwoPhaseReservations = company.DisableTwoPhaseReservations,
                Name = company.Name,
                amIParticipant = amIParticipant,
                profilePictureUrl = company.Image?.GetFileUrl()
            };
        }
    }

    public class WarehouseListItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public File image { get; set; }
        public Permission permission { get; set; }
        public string description { get; set; }
        public string address { get; set; }
        public string contactEmail { get; set; }
        public string contactPhone { get; set; }

        public List<WarehouseDoorListItem> doors { get; set; }
        public bool canCarrierCreateAnonymousReservation { get; set; }

        public static WarehouseListItem FromWarehouse(Warehouse w, bool amIParticipant, List<Permission> permissions, bool isCarrier)
        {
            Permission permission = null;
            if (!amIParticipant)
            {
                if (!isCarrier && w.canCarrierCreateAnonymousReservation)
                {
                    permission = new Permission()
                    {
                        Status = PermissionStatus.Accepted,
                        Type = PermissionType.ALL_DOORS,
                        PermissionsForDoor = new List<PermissionForDoor>()
                    };
                }
                else
                {
                    permission = permissions.Find(p => p.WarehouseId == w.Id);
                }
            }

            List<WarehouseDoorListItem> doors = new List<WarehouseDoorListItem>();
            if (amIParticipant)
            {
                doors = w.Doors.Select(d => WarehouseDoorListItem.FromDoor(d)).ToList();
            }
            else if (permission != null && permission.Status == PermissionStatus.Accepted)
            {
                var allowedDoorIds = permission.PermissionsForDoor.Select(d => d.DoorId).ToList();
                if (permission.Type != PermissionType.ONLY_TWO_PHASE)
                {
                    doors = w.Doors.Where(d => permission.Type == PermissionType.ALL_DOORS || allowedDoorIds.Contains(d.Id)).Select(d => WarehouseDoorListItem.FromDoor(d)).ToList();
                }
            }

            return new WarehouseListItem()
            {
                id = w.Id,
                name = w.Name,
                permission = permission,
                description = w.Description,
                address = w.Address,
                contactEmail = w.ContactEmail,
                contactPhone = w.ContactPhone,
                doors = doors,
                image = w.Image,
                canCarrierCreateAnonymousReservation = w.canCarrierCreateAnonymousReservation
            };
        }
    }

    public class WarehouseDoorListItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public static WarehouseDoorListItem FromDoor(Door d)
        {
            return new WarehouseDoorListItem()
            {
                id = d.Id,
                name = d.Name,
                description = d.Description,
            };
        }
    }
}
