using Newtonsoft.Json;
using OwlApi.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OwlApi.Models
{
    public class RecurringReservation
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int? CarrierId { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonConverter(typeof(JsonBConverter<List<ReservationField>>))]
        [Column(TypeName = "jsonb")]
        public string Data { get; set; }

        [ForeignKey("CarrierId")]
        public User Carrier { get; set; }
        public int? DoorId { get; set; }

        [ForeignKey("DoorId")]
        public Door Door { get; set; }
        public int? WarehouseId { get; set; }

        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; }

        [InverseProperty("RecurringReservation")]
        public ICollection<File> Files { get; set; }

        public string additionalContactEmail { get; set; }

        public string RecurrenceRule { get; set; }

        [ForeignKey("LanguageId")]
        public AppLanguage Language { get; set; }
        public int? LanguageId { get; set; }

        public List<ReservationField> GetData()
        {
            return JsonConvert.DeserializeObject<List<ReservationField>>(Data);
        }

        public void SetData(List<ReservationField> data)
        {
            Data = JsonConvert.SerializeObject(data);
        }

        public long GetPalletsCount()
        {
            var palletsInput = GetData().Find(f => f.SpecialMeaning == ReservationFieldSpecialMeaningField.NUMBER_OF_PALLETS);
            long palletsCount = 0;
            if (palletsInput != null)
            {
                palletsCount = int.Parse(palletsInput.Value);
            }

            return palletsCount;
        }

        public Reservation ConvertToReservation(DateTime date)
        {
            return new Reservation()
            {
                Id = Id,
                Carrier = Carrier,
                CarrierId = CarrierId,
                Data = Data,
                Start = Start,
                End = End,
                Date = date,
                CreatedAt = CreatedAt,
                WarehouseId = WarehouseId,
                isRecurring = true,
                ReservationStatusUpdates = new List<ReservationStatusUpdate>()
            };
        }

        public bool OccursOnDay(DateTime day)
        {
            if (RecurrenceRule == null || RecurrenceRule.Length == 0)
            {
                return true;
            }

            return RecurrenceRule.Contains(((int)day.DayOfWeek).ToString());
        }

        public void GenerateCode(OwlApiContext context)
        {
            if (Code != null)
            {
                return;
            }

            int attempt = 0;
            while (attempt < 1000)
            {
                var random = new Random();
                int length = 9;
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var code = new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());

                var existingReservation = context.RecurringReservations.Where(r => r.Code == code).FirstOrDefault();
                if (existingReservation == null)
                {
                    Code = code;
                    return;
                }

                attempt++;
            }
        }
    }
}
