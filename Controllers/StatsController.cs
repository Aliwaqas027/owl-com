using ChoETL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    [Authorize]
    public class StatsController : BaseController
    {

        public StatsController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        public enum BookedWindowsStatsDatePrecision
        {
            DAILY,
            WEEKLY,
            MONTHLY,
            YEARLY,
        }

        public class BookedWindowsStats
        {
            public BookedWindowsStatsDatePrecision precision { get; set; }
            public List<BookedWindowsStatsData> data { get; set; }
            public long totalNumberOfBookings { get; set; }
        }

        public class BookedWindowsStatsData
        {
            public DateTime forDate { get; set; }
            public long totalNumberOfBookings { get; set; }
            public List<BookedWindowsStatsPerWarehouse> bookingsPerWarehouse { get; set; }
        }

        public class BookedWindowsStatsPerWarehouse
        {
            public long warehouseId { get; set; }
            public string warehouseName { get; set; }
            public long totalNumberOfBookings { get; set; }
            public List<BookedWindowsStatsPerDoor> bookingsPerDoor { get; set; }
        }

        public class BookedWindowsStatsPerDoor
        {
            public long doorId { get; set; }
            public string doorName { get; set; }
            public long totalNumberOfBookings { get; set; }
            public List<BookedWindowsStatsPerTimeWindow> bookingsPerTimeWindow { get; set; }
        }

        public class BookedWindowsStatsPerTimeWindow
        {
            public string timeWindow { get; set; }
            public long totalNumberOfBookings { get; set; }
        }

        public class GetBookedWindowsStatsRequest
        {
            public BookedWindowsStatsDatePrecision precision { get; set; }
            public DateTime dateFrom { get; set; }
            public DateTime dateTo { get; set; }
        }

        public async Task<BookedWindowsStats> GetBookedWindowsStats([FromBody] GetBookedWindowsStatsRequest request)
        {
            User actor = GetCurrentActor();
            WarehouseAdminOnly();

            var bookings = await GetBookingsAmount(actor.Company.Id, request);

            var data = new List<BookedWindowsStatsData>();

            long totalNumberOfBookings = 0;

            foreach (var booking in bookings)
            {
                totalNumberOfBookings += booking.totalNumberOfBookings;

                var timeWindowStat = new BookedWindowsStatsPerTimeWindow()
                {
                    timeWindow = booking.timeWindow,
                    totalNumberOfBookings = booking.totalNumberOfBookings,
                };

                var doorStat = new BookedWindowsStatsPerDoor()
                {
                    doorId = booking.doorId,
                    doorName = booking.doorName,
                    totalNumberOfBookings = booking.totalNumberOfBookings,
                    bookingsPerTimeWindow = new List<BookedWindowsStatsPerTimeWindow>() { timeWindowStat }
                };

                var warehouseStat = new BookedWindowsStatsPerWarehouse()
                {
                    bookingsPerDoor = new List<BookedWindowsStatsPerDoor>() { doorStat },
                    warehouseId = booking.warehouseId,
                    warehouseName = booking.warehouseName,
                    totalNumberOfBookings = booking.totalNumberOfBookings
                };

                var existingData = data.Find(d => d.forDate == booking.groupedDay).FirstOrDefault<BookedWindowsStatsData>();
                if (existingData != null)
                {
                    existingData.totalNumberOfBookings += booking.totalNumberOfBookings;

                    var existingWarehouse = existingData.bookingsPerWarehouse.Find(w => w.warehouseId == booking.warehouseId).FirstOrDefault<BookedWindowsStatsPerWarehouse>();
                    if (existingWarehouse != null)
                    {
                        existingWarehouse.totalNumberOfBookings += booking.totalNumberOfBookings;

                        var existingDoor = existingWarehouse.bookingsPerDoor.Find(d => d.doorId == booking.doorId).FirstOrDefault<BookedWindowsStatsPerDoor>();
                        if (existingDoor != null)
                        {
                            existingDoor.totalNumberOfBookings += booking.totalNumberOfBookings;
                            existingDoor.bookingsPerTimeWindow.Add(timeWindowStat);
                        }
                        else
                        {
                            existingWarehouse.bookingsPerDoor.Add(doorStat);
                        }
                    }
                    else
                    {
                        existingData.bookingsPerWarehouse.Add(warehouseStat);
                    }
                }
                else
                {
                    data.Add(new BookedWindowsStatsData()
                    {
                        bookingsPerWarehouse = new List<BookedWindowsStatsPerWarehouse>() { warehouseStat },
                        forDate = booking.groupedDay,
                        totalNumberOfBookings = warehouseStat.totalNumberOfBookings
                    });
                }
            }

            return new BookedWindowsStats()
            {
                data = data,
                precision = request.precision,
                totalNumberOfBookings = totalNumberOfBookings
            };
        }

        public class GetBookingsAmountRawResult
        {
            public DateTime groupedDay { get; set; }
            public string warehouseName { get; set; }
            public long warehouseId { get; set; }
            public string doorName { get; set; }
            public long doorId { get; set; }
            public string timeWindow { get; set; }
            public long totalNumberOfBookings { get; set; }
        }

        private async Task<List<GetBookingsAmountRawResult>> GetBookingsAmount(int companyId, GetBookedWindowsStatsRequest request)
        {
            var bookings = new List<GetBookingsAmountRawResult>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @$"
            WITH ""groupedData"" as (
                SELECT 
                    date_trunc('{BookedWindowsStatsDatePrecisionToDateParamSQL(request.precision)}', ""Date"") AS ""groupedDay"",
                    doors.""Id"" AS ""doorId"",
                    warehouses.""Id"" AS ""warehouseId"",
                    reservations.""Start"" || '-' || reservations.""End"" AS ""timeWindow"",
                    COUNT(*) as ""totalNumberOfBookings""
                FROM reservations
                LEFT JOIN doors ON reservations.""DoorId"" = doors.""Id"" 
                LEFT JOIN warehouses ON doors.""WarehouseId"" = warehouses.""Id"" 
                WHERE ""Date"" BETWEEN '{FormatDateForSQL(request.dateFrom)}' AND '{FormatDateForSQL(request.dateTo)}' AND warehouses.""CompanyId"" = {companyId}
                GROUP BY ""groupedDay"", ""timeWindow"", ""doorId"", ""warehouseId""
            )

            SELECT 
                ""groupedData"".*, 
                warehouses.""Name"" as ""warehouseName"", 
                doors.""Name"" as ""doorName"" FROM ""groupedData""
            LEFT JOIN doors ON ""groupedData"".""doorId"" = doors.""Id""
            LEFT JOIN warehouses ON ""groupedData"".""warehouseId"" = warehouses.""Id""
            ORDER BY ""groupedData"".""groupedDay"" DESC;";

            // cmd.Parameters.Add(new SqlParameter("@appID", appointmentID));
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    bookings.Add(new GetBookingsAmountRawResult()
                    {
                        groupedDay = (DateTime)reader["groupedDay"],
                        totalNumberOfBookings = (long)reader["totalNumberOfBookings"],
                        warehouseId = (int)reader["warehouseId"],
                        warehouseName = (string)reader["warehouseName"],
                        doorId = (int)reader["doorId"],
                        doorName = (string)reader["doorName"],
                        timeWindow = (string)reader["timeWindow"]
                    });
                }
            }

            return bookings;
        }

        public string BookedWindowsStatsDatePrecisionToDateParamSQL(BookedWindowsStatsDatePrecision precision)
        {
            if (precision == BookedWindowsStatsDatePrecision.DAILY)
            {
                return "day";
            }
            else if (precision == BookedWindowsStatsDatePrecision.WEEKLY)
            {
                return "week";
            }
            else if (precision == BookedWindowsStatsDatePrecision.MONTHLY)
            {
                return "month";
            }
            else if (precision == BookedWindowsStatsDatePrecision.YEARLY)
            {
                return "year";
            }

            return "day";
        }

        public string FormatDateForSQL(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }
    }

}