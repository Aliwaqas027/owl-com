using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OwlApi.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OwlApi.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public TimeSpan? Start { get; set; }
        public TimeSpan? End { get; set; }

        public int? FixedTimeWindowId { get; set; }

        [ForeignKey("FixedTimeWindowId")]
        public TimeWindow FixedTimeWindow { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonConverter(typeof(JsonBConverter<List<ReservationField>>))]
        [Column(TypeName = "jsonb")]
        public string Data { get; set; }
        public int? CarrierId { get; set; }


        [ForeignKey("CarrierId")]
        public User Carrier { get; set; }
        public int? DoorId { get; set; }

        [ForeignKey("DoorId")]
        public Door Door { get; set; }
        public int? WarehouseId { get; set; }

        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; }

        [InverseProperty("Reservation")]
        public ICollection<File> Files { get; set; }

        [InverseProperty("Reservation")]
        public ICollection<ReservationStatusUpdate> ReservationStatusUpdates { get; set; }

        [NotMapped]
        public bool isRecurring { get; set; } = false;

        [NotMapped]
        public ReservationStatusUpdate LastStatusUpdate { get; set; } = null;

        [ForeignKey("LanguageId")]
        public AppLanguage Language { get; set; }
        public int? LanguageId { get; set; }

        public string additionalContactEmail { get; set; }

        public int? YAMASArrivalId { get; set; }

        public List<ReservationField> GetData()
        {
            return JsonConvert.DeserializeObject<List<ReservationField>>(Data);
        }

        public void SetData(List<ReservationField> data)
        {
            Data = JsonConvert.SerializeObject(data);
        }

        public double GetPalletsCount()
        {
            var palletsInput = GetData().Find(f => f.SpecialMeaning == ReservationFieldSpecialMeaningField.NUMBER_OF_PALLETS);
            int palletsCount = 0;
            if (palletsInput != null)
            {
                int.TryParse(palletsInput.Value, out palletsCount);
            }

            var halfPalletsInput = GetData().Find(f => f.SpecialMeaning == ReservationFieldSpecialMeaningField.NUMBER_OF_HALF_PALLETS);
            int halfPalletsCount = 0;
            if (halfPalletsInput != null)
            {
                int.TryParse(halfPalletsInput.Value, out halfPalletsCount);
            }

            return palletsCount + halfPalletsCount * 0.5;
        }

        public async Task GenerateCode(OwlApiContext context)
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

                var existingReservation = await context.Reservations.Where(r => r.Code == code).FirstOrDefaultAsync();
                if (existingReservation == null)
                {
                    Code = code;
                    return;
                }

                attempt++;
            }
        }
    }

    public enum ReservationDtoType
    {
        STANDARD,
        TWO_PHASE,
        RECURRING
    }

    public class ReservationDto
    {
        public int id { get; set; }
        public string code { get; set; }
        public string additionalContactEmail { get; set; }
        public ReservationDtoType type { get; set; }
        public StandardReservationData standardReservationData { get; set; }
        public RecurringReservationData recurringReservationData { get; set; }
        public List<ReservationField> data { get; set; }
        public UserExcerptDto carrier { get; set; }
        public DoorExcerptDto door { get; set; }
        public WarehouseExcerptDto warehouse { get; set; }
        public List<FileExcerptDto> files { get; set; }
        public DateTime createdAt { get; set; }
        public int? YAMASArrivalId { get; set; }

        public static ReservationDto FromReservation(Reservation reservation)
        {
            ReservationDtoType type = ReservationDtoType.STANDARD;

            StandardReservationData standardReservationData = null;

            if (reservation.Door == null)
            {
                type = ReservationDtoType.TWO_PHASE;
            }
            else
            {
                standardReservationData = new StandardReservationData()
                {
                    date = reservation.Date,
                    start = (TimeSpan)reservation.Start,
                    end = (TimeSpan)reservation.End,
                    isFixed = reservation.FixedTimeWindow != null
                };
            }

            Warehouse warehouse = reservation.Warehouse;
            if (warehouse == null && reservation.Door != null)
            {
                warehouse = reservation.Door.Warehouse;
            }

            return new ReservationDto()
            {
                id = reservation.Id,
                code = reservation.Code,
                additionalContactEmail = reservation.additionalContactEmail,
                type = type,
                standardReservationData = standardReservationData,
                recurringReservationData = null,
                data = reservation.GetData(),
                carrier = UserExcerptDto.FromUser(reservation.Carrier),
                door = reservation.Door != null ? DoorExcerptDto.FromDoor(reservation.Door) : null,
                warehouse = WarehouseExcerptDto.FromWarehouse(warehouse),
                files = reservation.Files.Select(f => FileExcerptDto.FromFile(f)).ToList(),
                createdAt = reservation.CreatedAt,
                YAMASArrivalId = reservation.YAMASArrivalId
            };
        }

        public static ReservationDto FromRecurringReservation(RecurringReservation reservation)
        {
            Warehouse warehouse = reservation.Warehouse;
            if (warehouse == null && reservation.Door != null)
            {
                warehouse = reservation.Door.Warehouse;
            }

            return new ReservationDto()
            {
                id = reservation.Id,
                code = reservation.Code,
                type = ReservationDtoType.RECURRING,
                standardReservationData = null,
                recurringReservationData = new RecurringReservationData()
                {
                    recurrenceRule = reservation.RecurrenceRule,
                    fromDate = reservation.FromDate,
                    toDate = reservation.ToDate
                },
                data = reservation.GetData(),
                carrier = UserExcerptDto.FromUser(reservation.Carrier),
                door = reservation.Door != null ? DoorExcerptDto.FromDoor(reservation.Door) : null,
                warehouse = WarehouseExcerptDto.FromWarehouse(warehouse),
                files = reservation.Files.Select(f => FileExcerptDto.FromFile(f)).ToList(),
                createdAt = reservation.CreatedAt
            };
        }

        // generate regular reservations from recurring reservations
        public static List<ReservationDto> GenerateNormalReservationsFromRecurringReservation(RecurringReservation reservation, DateTime dateFrom, DateTime dateTo)
        {
            Warehouse warehouse = reservation.Warehouse;
            if (warehouse == null && reservation.Door != null)
            {
                warehouse = reservation.Door.Warehouse;
            }

            List<ReservationDto> reservations = new List<ReservationDto>();

            DateTime iterationDateFrom = reservation.FromDate == null || reservation.FromDate < dateFrom ? dateFrom : (DateTime)reservation.FromDate;
            DateTime iterationDateTo = reservation.ToDate == null || reservation.ToDate > dateTo ? dateTo : (DateTime)reservation.ToDate;

            // iterate over all dates
            foreach (DateTime day in EachDay(iterationDateFrom, iterationDateTo))
            {
                // check if recurring reservation occurs on this day
                if (!reservation.OccursOnDay(day))
                {
                    continue;
                }

                var reservationForThisDay = new ReservationDto()
                {
                    id = reservation.Id,
                    type = ReservationDtoType.RECURRING,
                    standardReservationData = new StandardReservationData()
                    {
                        date = day,
                        start = reservation.Start,
                        end = reservation.End,
                        isFixed = false
                    },
                    recurringReservationData = new RecurringReservationData()
                    {
                        recurrenceRule = reservation.RecurrenceRule,
                        fromDate = reservation.FromDate,
                        toDate = reservation.ToDate
                    },
                    data = reservation.GetData(),
                    carrier = UserExcerptDto.FromUser(reservation.Carrier),
                    door = reservation.Door != null ? DoorExcerptDto.FromDoor(reservation.Door) : null,
                    warehouse = WarehouseExcerptDto.FromWarehouse(warehouse),
                    files = reservation.Files.Select(f => FileExcerptDto.FromFile(f)).ToList(),
                    createdAt = reservation.CreatedAt
                };
                reservations.Add(reservationForThisDay);
            }

            return reservations;
        }

        private static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
    }

    public class RecurringReservationData
    {
        public string recurrenceRule { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
    }

    public class StandardReservationData
    {
        public DateTime date { get; set; }
        public TimeSpan start { get; set; }
        public TimeSpan end { get; set; }
        public bool isFixed { get; set; }
    }
}
