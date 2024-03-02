using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwlApi.Exceptions;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace OwlApi.Controllers
{
    [Authorize]
    public class DoorController : BaseController
    {
        public DoorController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        [AllowAnonymous]
        public async Task<Door> Get(int id)
        {
            User actor = GetCurrentActor();
            Door door = await _context.Doors
              .Where(d => d.Id == id && (actor != null || d.Warehouse.canCarrierCreateAnonymousReservation))
              .Include(d => d.Warehouse)
              .ThenInclude(w => w.Permissions)
              .ThenInclude(p => p.PermissionsForDoor)
              .Include(d => d.Availability.TimeWindows)
              .FirstOrDefaultAsync();

            if (door == null)
            {
                throw new ApplicationException("Not found");
            }

            if (actor != null)
            {
                door.Warehouse.Permissions = door.Warehouse.Permissions.Where(p => p.CarrierId == actor.Id).ToList();
            }

            door.Availability.TimeWindows = door.Availability.TimeWindows.Where(tw => tw.Id > 0).ToList();

            if (door != null && door.Availability != null && door.Availability.TimeWindows != null)
            {
                door.Availability.TimeWindows = door.Availability.TimeWindows.OrderBy(tw => tw.Start).ToList();
            }

            return door;
        }

        public async Task<ActionResult> Delete(int id)
        {
            User actor = GetCurrentActor();
            Door door = await _context.Doors
              .Where(d => d.Id == id)
              .Include(d => d.Warehouse)
              .Include(d => d.Availability)
              .Include(d => d.ReservationFields)
              .FirstOrDefaultAsync();
            if (door.Warehouse.CompanyId != actor.Company.Id) throw new AuthenticationException();

            var reservations = await _context.Reservations.Where(r => r.DoorId == door.Id).Include(r => r.Files).Include(r => r.ReservationStatusUpdates).ToListAsync();
            var recReservations = await _context.RecurringReservations.Where(r => r.DoorId == door.Id).Include(r => r.Files).ToListAsync();

            _context.RemoveRange(reservations);
            _context.RemoveRange(recReservations);

            _context.Doors.Remove(door);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task<Door> Copy(int id)
        {
            User actor = GetCurrentActor();
            Door door = await _context.Doors
              .Where(d => d.Id == id)
              .Include(d => d.Warehouse)
              .Include(d => d.Availability)
                .ThenInclude(a => a.TimeWindows)
              .FirstOrDefaultAsync();
            if (door.Warehouse.CompanyId != actor.Company.Id) throw new AuthenticationException();

            Availability availability = new Availability()
            {
                MinimumNotice = door.Availability.MinimumNotice,
                GranularityMinutes = door.Availability.GranularityMinutes,
                MaxArrivalInacurracy = door.Availability.MaxArrivalInacurracy,
                WorkTimeFrom = door.Availability.WorkTimeFrom,
                WorkTimeTo = door.Availability.WorkTimeTo,
                TimeWindows = new List<TimeWindow>()
            };
            foreach (TimeWindow tw in door.Availability.TimeWindows)
            {
                availability.TimeWindows.Add(new TimeWindow()
                {
                    Start = tw.Start,
                    End = tw.End,
                    BookablePallets = tw.BookablePallets,
                    BookableSlots = tw.BookableSlots,
                    BookableWeekdays = tw.BookableWeekdays
                });
            }

            Door newDoor = new Door()
            {
                WarehouseId = door.WarehouseId,
                Name = "Copy of " + door.Name,
                Description = door.Description,
                Availability = availability
            };
            newDoor.Properties = door.Properties;

            var newDoorEntity = _context.Doors.Add(newDoor);

            var doorReservationFields = _context.ReservationFields.AsNoTracking().Where(f => f.DoorId == door.Id).Include(f => f.reservationFieldNames).AsNoTracking().ToList();

            foreach (var field in doorReservationFields)
            {
                var translations = field.reservationFieldNames;
                field.unattachAndDeriveFrom(false);
                field.Door = newDoorEntity.Entity;
                _context.Add(field);

                foreach (var fieldName in translations)
                {
                    fieldName.unattach();
                    fieldName.reservationField = field;
                    _context.Add(fieldName);
                }
            }

            await _context.SaveChangesAsync();

            return newDoor;
        }

        public class CopyToDoorSettings
        {
            public bool name { get; set; }
            public bool description { get; set; }
            public bool palletsPerDay { get; set; }
            public bool workDay { get; set; }
            public bool minimumArrivalInaccuracy { get; set; }
            public bool timeWindows { get; set; }
            public bool reservationDurationSettings { get; set; }
            public bool reservationFields { get; set; }
            public bool emailAttachments { get; set; }
        }

        public class CopyToAnotherDoorRequest
        {
            public List<int> ToDoorIds { get; set; }
            public CopyToDoorSettings Settings { get; set; }
        }

        public async Task<IActionResult> CopyToAnotherDoor(int id, [FromBody] CopyToAnotherDoorRequest request)
        {
            WarehouseOnly();
            var actor = GetCurrentActor();
            var doorFrom = await _context.Doors.Where(d => d.Id == id && d.Warehouse.CompanyId == actor.Company.Id).Include(d => d.Availability).ThenInclude(a => a.TimeWindows).FirstOrDefaultAsync();
            if (doorFrom == null)
            {
                throw new ModelNotFoundException();
            }

            for (int i = 0; i < request.ToDoorIds.Count; i++)
            {
                var doorTo = await _context.Doors
                    .Where(d => d.Id == request.ToDoorIds[i] && d.Warehouse.CompanyId == actor.Company.Id)
                    .Include(d => d.Availability)
                    .ThenInclude(a => a.TimeWindows)
                    .ThenInclude(t => t.TimeWindowFieldsFilter).FirstOrDefaultAsync();

                if (doorTo == null)
                {
                    continue;
                }

                if (request.Settings.name)
                {
                    doorTo.Name = doorFrom.Name;
                }

                if (request.Settings.description)
                {
                    doorTo.Description = doorFrom.Description;
                }

                if (request.Settings.palletsPerDay)
                {
                    doorTo.DailyPalletsLimit = doorFrom.DailyPalletsLimit;
                }

                if (request.Settings.workDay)
                {
                    doorTo.Availability.WorkTimeFrom = doorFrom.Availability.WorkTimeFrom;
                    doorTo.Availability.WorkTimeTo = doorFrom.Availability.WorkTimeTo;
                }

                if (request.Settings.minimumArrivalInaccuracy)
                {
                    doorTo.Availability.MaxArrivalInacurracy = doorFrom.Availability.MaxArrivalInacurracy;
                }

                if (request.Settings.timeWindows)
                {
                    var a = new Availability();

                    a.GranularityMinutes = doorFrom.Availability.GranularityMinutes;
                    a.MaxArrivalInacurracy = doorFrom.Availability.MaxArrivalInacurracy;
                    a.MinimumNotice = doorFrom.Availability.MinimumNotice;
                    a.WorkTimeFrom = doorFrom.Availability.WorkTimeFrom;
                    a.WorkTimeTo = doorFrom.Availability.WorkTimeTo;
                    a.TimeWindows = new List<TimeWindow>();

                    doorTo.Availability = a;

                    foreach (TimeWindow tw in doorFrom.Availability.TimeWindows)
                    {
                        doorTo.Availability.TimeWindows.Add(new TimeWindow()
                        {
                            Start = tw.Start,
                            End = tw.End,
                            BookablePallets = tw.BookablePallets,
                            BookableSlots = tw.BookableSlots,
                            BookableWeekdays = tw.BookableWeekdays
                        });
                    }
                }

                if (request.Settings.reservationDurationSettings)
                {
                    doorTo.SetProperties(doorFrom.GetProperties());
                }

                if (request.Settings.reservationFields)
                {
                    var currentFields = _context.ReservationFields.Where(r => r.DoorId == doorTo.Id).ToList();
                    foreach (var field in currentFields)
                    {
                        field.DoorId = null;
                    }

                    _context.UpdateRange(currentFields);

                    var doorReservationFields = _context.ReservationFields.AsNoTracking().Where(f => f.DoorId == doorFrom.Id).Include(f => f.reservationFieldNames).AsNoTracking().ToList();

                    foreach (var field in doorReservationFields)
                    {
                        var translations = field.reservationFieldNames;
                        field.unattachAndDeriveFrom(false);
                        field.Door = doorTo;
                        _context.Add(field);

                        foreach (var fieldName in translations)
                        {
                            fieldName.unattach();
                            fieldName.reservationField = field;
                            _context.Add(fieldName);
                        }
                    }
                }

                if (request.Settings.emailAttachments)
                {
                    var currentFiles = _context.Files.Where(r => r.DoorAttachmentId == doorTo.Id);
                    foreach (var file in currentFiles)
                    {
                        file.DoorAttachmentId = null;
                    }

                    _context.UpdateRange(currentFiles);


                    var files = _context.Files.AsNoTracking().Where(f => f.DoorAttachmentId == doorFrom.Id).ToList();
                    foreach (var file in files)
                    {
                        file.DoorAttachment = doorTo;
                        file.Id = 0;
                        _context.Add(file);
                    }
                }

                _context.Update(doorTo);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        public class ReservationsOnDoorRequest
        {
            public DateTime from;
            public DateTime to;
        }

        [AllowAnonymous]
        public async Task<List<ReservationDto>> ReservationsOnDoor(int id, [FromBody] ReservationsOnDoorRequest request)
        {
            var actor = GetCurrentActor();

            DateTime from = DateTime.SpecifyKind(request.from, DateTimeKind.Utc).Date;
            DateTime to = DateTime.SpecifyKind(request.to, DateTimeKind.Utc).Date;

            Door door = await _context.Doors
                .Where(d => d.Id == id && (actor != null || d.Warehouse.canCarrierCreateAnonymousReservation))
                .FirstOrDefaultAsync();
            if (door == null) throw new ModelNotFoundException();

            List<Reservation> reservations = await _context.Reservations
                .Where(r => r.DoorId == id)
                .Where(r => from <= r.Date && r.Date <= to)
                .OrderBy(r => r.Start)
                .Include(r => r.Carrier)
                .Include(r => r.Files)
                .Include(r => r.ReservationStatusUpdates)
                .Include(r => r.Door)
                    .ThenInclude(r => r.Warehouse)
                    .ThenInclude(r => r.Company)
                .Include(r => r.Warehouse)
                    .ThenInclude(r => r.Company)
                .Include(r => r.Carrier)
                .ToListAsync();

            List<RecurringReservation> recurringReservations = await _context.RecurringReservations
                .Where(r => r.DoorId == id)
                .Where(IsRecurringReservationInDateRange(from, to))
                .OrderBy(r => r.Start)
                .Include(r => r.Carrier)
                .Include(r => r.Files)
                .Include(r => r.Door)
                    .ThenInclude(r => r.Warehouse)
                    .ThenInclude(r => r.Company)
                .Include(r => r.Warehouse)
                    .ThenInclude(r => r.Company)
                .Include(r => r.Carrier)
                .ToListAsync();

            List<ReservationDto> combinedReservations = new List<ReservationDto>();
            combinedReservations.AddRange(reservations.Select(reservation => ReservationDto.FromReservation(reservation)).ToList());
            combinedReservations.AddRange(recurringReservations.Select(reservation => ReservationDto.GenerateNormalReservationsFromRecurringReservation(reservation, from, to)).SelectMany(i => i));

            return combinedReservations;
        }

        private System.Linq.Expressions.Expression<Func<RecurringReservation, bool>> IsRecurringReservationInDateRange(DateTime from, DateTime to)
        {
            return reservation =>
                (reservation.FromDate == null || to == null || reservation.FromDate <= to) && (reservation.ToDate == null || from == null || reservation.ToDate >= from)
            ;
        }

        public async Task<ActionResult> Update([FromBody] Door door)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            if (door == null) throw new IncorrectRequest();

            Door oldDoor = await _context.Doors
              .Where(d => d.Id == door.Id)
              .Include(d => d.Warehouse)
              .Include(d => d.Availability)
                .ThenInclude(d => d.TimeWindows)
              .FirstOrDefaultAsync();

            if (door.Warehouse == null) throw new ModelNotFoundException();
            if (door.Warehouse.CompanyId != actor.Company.Id) throw new AuthenticationException();

            if (!door.Availability.Validate()) throw new IncorrectRequest();

            oldDoor.Name = door.Name;
            oldDoor.Description = door.Description;
            oldDoor.Properties = door.Properties;
            oldDoor.DailyPalletsLimit = door.DailyPalletsLimit;
            Availability.Update(_context, oldDoor.Availability, door.Availability);

            _context.Doors.Update(oldDoor);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [AllowAnonymous]
        public async Task<List<Door>> GetDoors(int id)
        {
            User actor = GetCurrentActor();

            var warehouse = await _context.Warehouses.Where(w => w.Id == id).Include(w => w.Permissions).ThenInclude(p => p.PermissionsForDoor).FirstOrDefaultAsync();
            if (warehouse == null)
            {
                throw new ApplicationException("not found");
            }

            if (!warehouse.canCarrierCreateAnonymousReservation && actor == null)
            {
                throw new ApplicationException("not found");
            }

            if (actor != null && actor.IsWarehouse() && !warehouse.canCarrierCreateAnonymousReservation)
            {
                if (warehouse.CompanyId != actor.Company.Id)
                {
                    throw new ApplicationException("not found");
                }
            }

            Permission permission = null;
            var permittedDoors = new List<int>();
            if (!warehouse.canCarrierCreateAnonymousReservation && actor != null && actor.IsCarrier())
            {
                permission = warehouse.Permissions.Where(p => p.CarrierId == actor.Id).FirstOrDefault();
                if (permission == null || permission.Status != PermissionStatus.Accepted)
                {
                    throw new ApplicationException("not found");
                }

                permittedDoors = permission.PermissionsForDoor.Select(p => p.DoorId).ToList();
            }

            var doors = await _context.Doors
                .Include(d => d.Warehouse)
                .Include(d => d.DoorFieldsFilters)
                .Where(d => d.WarehouseId == id)
                .Where(d => permission == null || permission.Type == PermissionType.ALL_DOORS || permission.Type == PermissionType.ONLY_SPECIFIC_DOORS && permittedDoors.Contains(d.Id))
                .ToListAsync();

            return doors;
        }

        public async Task<List<Door>> GetAllCompanyDoors()
        {
            WarehouseOnly();

            User actor = GetCurrentActor();

            var doors = await _context.Doors
                .Where(d => d.Warehouse.CompanyId == actor.Company.Id)
                .ToListAsync();

            return doors;
        }


        public async Task<List<DoorFieldsFilter>> GetDoorFieldsFilter(int id, [FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            var doorFieldsFilters = await _context.DoorFieldsFilters
                .Where(d => d.DoorId == id)
                .Where(d => d.Door.Warehouse.CompanyId == actor.Company.Id)
                .Include(d => d.ReservationField)
                .ThenInclude(f => f.reservationFieldNames)
                .ThenInclude(n => n.language)
                .ToListAsync();

            foreach (var filter in doorFieldsFilters)
            {
                var correspondingLanguage = filter.ReservationField.reservationFieldNames.Where(name => name.language.subdomain.Equals(lang)).FirstOrDefault();
                filter.ReservationField.Name = SettingsController.getNameOfFieldByLocale(filter.ReservationField, lang);
            }

            return doorFieldsFilters;
        }

        public async Task<List<TimeWindowFieldsFilter>> GetTimeWindowFieldsFilter(int id, [FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            var doorFieldsFilters = await _context.TimeWindowFieldsFilters
                .Where(d => d.TimeWindowId == id)
                .Include(d => d.ReservationField)
                .ThenInclude(f => f.reservationFieldNames)
                .ThenInclude(n => n.language)
                .ToListAsync();

            foreach (var filter in doorFieldsFilters)
            {
                var correspondingLanguage = filter.ReservationField.reservationFieldNames.Where(name => name.language.subdomain.Equals(lang)).FirstOrDefault();
                filter.ReservationField.Name = SettingsController.getNameOfFieldByLocale(filter.ReservationField, lang);
            }

            return doorFieldsFilters;
        }

        public class SetDoorFieldsFilterItem
        {
            public int ReservationFieldId { get; set; }
            public string[] Values { get; set; }
        }

        public class SetDoorFieldsFilterRequest
        {
            public List<SetDoorFieldsFilterItem> Items { get; set; }
        }

        public async Task<ActionResult> SetDoorFieldsFilter(int id, [FromBody] SetDoorFieldsFilterRequest request)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            var door = await _context.Doors.Where(d => d.Id == id).Where(d => d.Warehouse.CompanyId == actor.Company.Id).FirstOrDefaultAsync();
            if (door == null)
            {
                throw new ApplicationException("Not found");
            }

            var existingFields = await _context.DoorFieldsFilters.Where(d => d.DoorId == id).ToListAsync();
            _context.RemoveRange(existingFields);

            foreach (var item in request.Items)
            {
                var newFilter = new DoorFieldsFilter()
                {
                    Door = door,
                    ReservationFieldId = item.ReservationFieldId,
                    Values = item.Values,
                };

                _context.Add(newFilter);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        public async Task<ActionResult> SetTimeWindowFieldsFilter(int id, [FromBody] SetDoorFieldsFilterRequest request)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            var timeWindow = await _context.TimeWindows.Where(d => d.Id == id).FirstOrDefaultAsync();
            if (timeWindow == null)
            {
                throw new ApplicationException("Not found");
            }

            var existingFields = await _context.TimeWindowFieldsFilters.Where(d => d.TimeWindowId == id).ToListAsync();
            _context.RemoveRange(existingFields);

            foreach (var item in request.Items)
            {
                var newFilter = new TimeWindowFieldsFilter()
                {
                    TimeWindow = timeWindow,
                    ReservationFieldId = item.ReservationFieldId,
                    Values = item.Values,
                };

                _context.Add(newFilter);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        public class GetAvailableTimeWindowsRequest
        {
            public DateTime Date { get; set; }
            public int? ReservationIdForEditing { get; set; }
        }

        [AllowAnonymous]
        public async Task<ICollection<TimeWindow>> GetAvailableTimeWindows(int id, [FromBody] GetAvailableTimeWindowsRequest request)
        {
            var actor = GetCurrentActor();
            var door = await _context.Doors.Where(d => d.Id == id)
                                           .Include(d => d.Warehouse)
                                           .Include(d => d.Availability)
                                           .ThenInclude(a => a.TimeWindows)
                                           .ThenInclude(tw => tw.TimeWindowFieldsFilter)
                                           .FirstOrDefaultAsync();

            if (actor == null && !door.Warehouse.canCarrierCreateAnonymousReservation)
            {
                throw new AuthenticationException();
            }

            if (!door.Warehouse.canCarrierCreateAnonymousReservation && door.Warehouse.CompanyId != actor.Company.Id)
            {
                throw new AuthenticationException();
            }

            var holiday = await _context.Holidays.Where(h => h.Date == request.Date && h.CompanyId == door.Warehouse.CompanyId).FirstOrDefaultAsync();
            if (holiday != null)
            {
                return new List<TimeWindow>();
            }

            var reservations = await _context.Reservations.Where(r => r.Id != request.ReservationIdForEditing && r.DoorId == id && r.FixedTimeWindow != null)
                                                    .Where(r => r.Date.Date == request.Date.Date)
                                                    .Include(r => r.FixedTimeWindow)
                                                    .ThenInclude(w => w.Availability)
                                                    .ToListAsync();

            var allTimeWindows = door.Availability.TimeWindows.Where(tw => tw.BookableWeekdays.Contains((int)request.Date.DayOfWeek)).ToList();
            var timeWindowsSlots = allTimeWindows.Select(tw => tw.BookableSlots).ToArray();
            double[] timeWindowsPallets = allTimeWindows.Select(tw => (double)tw.BookablePallets).ToArray();

            var allPallets = reservations.Select(r => r.GetPalletsCount()).Sum();
            var remainingPalletsForDoorOnThisDay = (double)0;
            if (door.DailyPalletsLimit > 0)
            {
                remainingPalletsForDoorOnThisDay = door.DailyPalletsLimit - allPallets;
                if (remainingPalletsForDoorOnThisDay <= 0)
                {
                    return new List<TimeWindow>();
                }
            }


            // subtract available slots / pallets for time windows
            foreach (var reservation in reservations)
            {
                var timeWindowData = allTimeWindows.Select((timeWindow, index) => new { timeWindow, index })
                                                .FirstOrDefault(item => item.timeWindow.Id == reservation.FixedTimeWindow.Id);
                if (timeWindowData == null)
                {
                    continue;
                }

                var timeWindow = timeWindowData.timeWindow;
                var timeWindowIndex = timeWindowData.index;

                var areUnlimitedSlots = timeWindow.BookableSlots == 0;
                var areUnlimitedPallets = timeWindow.BookablePallets == 0;

                if (!areUnlimitedSlots)
                {
                    timeWindowsSlots[timeWindowIndex]--;
                }

                if (!areUnlimitedPallets)
                {
                    var palletsAmount = reservation.GetPalletsCount();
                    timeWindowsPallets[timeWindowIndex] -= palletsAmount;
                }
            }

            // remove time windows with spent pallets / slots
            for (int i = allTimeWindows.Count - 1; i >= 0; i--)
            {
                var timeWindow = allTimeWindows.ElementAt(i);
                var areUnlimitedSlots = timeWindow.BookableSlots == 0;
                var areUnlimitedPallets = timeWindow.BookablePallets == 0;

                if (!areUnlimitedSlots && timeWindowsSlots[i] <= 0)
                {
                    allTimeWindows.Remove(timeWindow);
                    continue;
                }

                if (!areUnlimitedPallets && timeWindowsPallets[i] < 0)
                {
                    allTimeWindows.Remove(timeWindow);
                    continue;
                }

                timeWindow.BookablePallets = timeWindowsPallets[i];
                timeWindow.BookableSlots = timeWindowsSlots[i];
            }

            // apply ceilling with door pallets limit (if exists)
            if (remainingPalletsForDoorOnThisDay > 0)
            {
                for (int i = 0; i < allTimeWindows.Count; i++)
                {
                    var timeWindow = allTimeWindows.ElementAt(i);
                    if (timeWindow.BookablePallets <= 0 || timeWindow.BookablePallets > remainingPalletsForDoorOnThisDay)
                    {
                        timeWindow.BookablePallets = remainingPalletsForDoorOnThisDay;
                    }
                }
            }

            return allTimeWindows.OrderBy(tw => tw.Start).ToList();
        }
    }
}