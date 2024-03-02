using ChoETL;
using Microsoft.EntityFrameworkCore;
using OwlApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace OwlApi.Helpers
{
    public class ReservationHelper
    {
        private readonly OwlApiContext _context;

        public ReservationHelper(OwlApiContext context)
        {
            _context = context;
        }

        public async Task CanReserve(User actor, int WarehouseId, Reservation reservation)
        {
            var warehouse = await _context.Warehouses.Where(w => w.Id == WarehouseId).FirstOrDefaultAsync();
            if (warehouse == null)
            {
                throw new ApplicationException("Warehouse not found");
            }

            if (actor == null && !warehouse.canCarrierCreateAnonymousReservation)
            {
                throw new ApplicationException("Permission denied");
            }

            if (actor != null && !warehouse.canCarrierCreateAnonymousReservation)
            {
                if (actor.IsCarrier())
                {
                    Permission permission = await _context.Permissions
                     .Where(p => p.WarehouseId == WarehouseId)
                     .Where(p => p.CarrierId == actor.Id)
                     .Include(p => p.PermissionsForDoor)
                     .FirstOrDefaultAsync();

                    if (permission == null || permission.Status != PermissionStatus.Accepted)
                    {
                        Console.WriteLine("No permission");
                        throw new ApplicationException("No permission");
                    }

                    if (reservation.DoorId != null && permission.Type != PermissionType.ALL_DOORS)
                    {
                        bool hasNoPermission = false;
                        if (permission.Type == PermissionType.ONLY_TWO_PHASE)
                        {
                            hasNoPermission = true;
                        }

                        if (permission.Type == PermissionType.ONLY_SPECIFIC_DOORS && permission.PermissionsForDoor.Where(d => d.DoorId == reservation.DoorId).FirstOrDefault() == null)
                        {
                            hasNoPermission = true;
                        }

                        if (hasNoPermission)
                        {
                            Console.WriteLine("No permission");
                            throw new ApplicationException("No permission");
                        }
                    }
                }
                else if (actor.IsWarehouse())
                {
                    if (warehouse.CompanyId != actor.Company.Id)
                    {
                        Console.WriteLine("No permission");
                        throw new ApplicationException("No permission");
                    }
                }
            }

            if (reservation.DoorId == null)
            {
                return;
            }

            var existingEmailReservation = await _context.Reservations
                .Where(r => r.Date == reservation.Date && r.DoorId == reservation.DoorId && r.Id != reservation.Id)
                .Where(r => r.additionalContactEmail != null && r.additionalContactEmail.Length > 0 && r.additionalContactEmail == reservation.additionalContactEmail)
                .FirstOrDefaultAsync();

            if (existingEmailReservation != null)
            {
                throw new ApplicationException("Email exists for this day");
            }

            var driverCodeField = ReservationField.FindFieldByMeaning(reservation.GetData(), ReservationFieldSpecialMeaningField.YAMAS_DRIVER_CODE);
            if (driverCodeField == null)
            {
                return;
            }

            var tmpReservations = await _context.Reservations
                .Where(r => r.Date == reservation.Date && r.DoorId == reservation.DoorId && r.Id != reservation.Id)
                .ToListAsync();

            var existingDriverCodeReservation = tmpReservations.Where(r =>
                {
                    var rDriverCodeField = ReservationField.FindFieldByMeaning(r.GetData(), ReservationFieldSpecialMeaningField.YAMAS_DRIVER_CODE);
                    if (rDriverCodeField == null)
                    {
                        return false;
                    }

                    if (rDriverCodeField.Value == null || rDriverCodeField.Value.Length == 0 || rDriverCodeField.Value == null || rDriverCodeField.Value.Length == 0)
                    {
                        return false;
                    }

                    return rDriverCodeField.Value == driverCodeField.Value;
                })
                .FirstOrDefault();

            if (existingDriverCodeReservation != null)
            {
                throw new ApplicationException("Driver code exists for this day");
            }

        }

        public async Task CheckReservationValidity(Reservation reservation, User actor, int? reservationToEditId = null)
        {

            if (reservation.Date < DateTime.UtcNow.Date)
            {
                Console.WriteLine("Date is in past");
                throw new ApplicationException("Date is in past");
            }

            if (reservation.Start > reservation.End)
            {
                Console.WriteLine("Start after end");
                throw new ApplicationException("Start after end");
            }

            reservation.CreatedAt = DateTime.UtcNow;
            reservation.Date = reservation.Date.Date;
            await reservation.GenerateCode(_context);

            Door door = await _context.Doors
                  .Where(d => d.Id == reservation.DoorId)
                  .Include(d => d.Warehouse)
                  .Include(d => d.Availability)
                  .Include(d => d.Availability.TimeWindows
                    .Where(tw => tw.Start <= reservation.Start)
                    .Where(tw => tw.End >= reservation.End)
                  )
                  .ThenInclude(tw => tw.TimeWindowFieldsFilter)
                  .Include(d => d.Reservations
                    .Where(r => r.Date == reservation.Date)
                    .Where(r => r.End > reservation.Start)
                    .Where(r => r.Start < reservation.End)
                  )
                  .FirstOrDefaultAsync();

            if (door == null)
            {
                Console.WriteLine("Door is missing");
                throw new ApplicationException("Door is missing");
            };

            var holiday = await _context.Holidays.Where(h => h.Date == reservation.Date && h.CompanyId == door.Warehouse.CompanyId).FirstOrDefaultAsync();
            if (holiday != null)
            {
                throw new ApplicationException("Holiday");
            }

            if (door.Availability.TimeWindows.Count == 0)
            {
                Console.WriteLine("Door has no time windows");
                throw new ApplicationException("Door has no time windows");
            }

            if (door.GetProperties().Type == ReservationType.Fixed)
            {
                var existingReservations = door.Reservations.Select(r => r).ToList();
                for (int i = door.Availability.TimeWindows.Count - 1; i >= 0; i--)
                {
                    var tw = door.Availability.TimeWindows.ElementAt(i);
                    var matchingReservation = existingReservations.Where(r => tw.MatchesFields(r.Start, r.End, r.GetData())).FirstOrDefault();
                    if (matchingReservation != null)
                    {
                        if (tw.BookableSlots == 0)
                        {
                            continue;
                        }

                        tw.BookableSlots--;
                        if (tw.BookableSlots == 0)
                        {
                            door.Availability.TimeWindows.Remove(tw);
                            existingReservations.Remove(matchingReservation);
                        }
                    }
                }
            }
            else
            {
                if (door.Reservations.Count > 0)
                {
                    var isConflicting = true;
                    if (reservationToEditId != null)
                    {
                        if (door.Reservations.Count == 1 && door.Reservations.First().Id == reservationToEditId)
                        {
                            isConflicting = false;
                        }
                    }

                    if (isConflicting)
                    {
                        Console.WriteLine("Door has existing reservations");
                        throw new ApplicationException("Door has existing reservations");
                    }
                }
            }

            if (reservation.Date + reservation.Start - door.Availability.MinimumNotice < DateTime.Now)
            {
                Console.WriteLine("Minimum notice too early!");
                throw new ApplicationException("Minimum notice too early!");
            }

            DoorProperties properties = door.GetProperties();
            var palletsCount = reservation.GetPalletsCount();

            reservation.FixedTimeWindow = null;

            if (properties.Type == ReservationType.Fixed)
            {
                var data = reservation.GetData();
                TimeWindow fixedTimeWindow = null;
                foreach (TimeWindow tw in door.Availability.TimeWindows)
                {
                    if (tw.MatchesFields(reservation.Start, reservation.End, data))
                    {
                        fixedTimeWindow = tw;
                        break;
                    }
                }

                if (fixedTimeWindow == null)
                {
                    Console.WriteLine("Fixed reservation time window not found");
                    throw new ApplicationException("Fixed reservation time window not found");
                }

                if (!fixedTimeWindow.BookableWeekdays.Contains((int)reservation.Date.DayOfWeek))
                {
                    throw new ApplicationException("Holiday");
                }

                reservation.FixedTimeWindowId = fixedTimeWindow.Id;
            }

            if (properties.Type == ReservationType.Calculated)
            {
                TimeSpan duration = properties.BaseTime + palletsCount * properties.TimePerPallet;
                var diff = duration + reservation.Start - reservation.End;
                if (diff >= TimeSpan.FromSeconds(1))
                {
                    Console.WriteLine("Calculated reservation time window not matching");
                    throw new ApplicationException("Calculated reservation time window not matching " + (duration + reservation.Start).ToString());
                }
            }

            await CanReserve(actor, door.Warehouse.Id, reservation);
        }
    }
}
