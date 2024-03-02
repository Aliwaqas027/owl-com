using Newtonsoft.Json;
using OwlApi.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public enum ReservationType
    {
        Fixed,
        Calculated,
        Free
    }
    public class DoorProperties
    {
        public ReservationType Type;
        public bool PalletsMode = false;
        public TimeSpan TimePerPallet;
        public TimeSpan BaseTime;
    }
    public class Door
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int WarehouseId { get; set; }
        public int AvailabilityId { get; set; }
        [Column(TypeName = "jsonb")]
        [JsonConverter(typeof(JsonBConverter<DoorProperties>))]
        public string Properties { get; set; }
        public string Description { get; set; }
        public int DailyPalletsLimit { get; set; } = 0;

        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; }
        [ForeignKey("AvailabilityId")]
        public Availability Availability { get; set; }
        [InverseProperty("Door")]
        public ICollection<Reservation> Reservations { get; set; }

        [InverseProperty("Door")]
        public ICollection<ReservationField> ReservationFields { get; set; }

        [InverseProperty("Door")]
        public ICollection<DoorFieldsFilter> DoorFieldsFilters { get; set; }

        public DoorProperties GetProperties()
        {
            return JsonConvert.DeserializeObject<DoorProperties>(Properties);
        }
        public void SetProperties(DoorProperties properties)
        {
            Properties = JsonConvert.SerializeObject(properties);
        }
    }

    public class DoorExcerptDto
    {
        public int id { get; set; }
        public string name { get; set; }

        public static DoorExcerptDto FromDoor(Door door)
        {
            return new DoorExcerptDto()
            {
                id = door.Id,
                name = door.Name,
            };
        }
    }
}
