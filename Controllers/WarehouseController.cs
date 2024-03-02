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
    public class WarehouseController : BaseController
    {
        public WarehouseController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        public async Task<List<WarehouseCompanyListItem>> List()
        {
            var actor = GetCurrentActor();

            List<Company> result = await _context.Companies
              .OrderBy(c => c.Name)
              .Include(u => u.Warehouses.OrderBy(w => w.Name))
              .ThenInclude(w => w.Doors.OrderBy(w => w.Name))
              .Include(u => u.Warehouses.OrderBy(w => w.Name))
              .ThenInclude(w => w.Image)
              .ToListAsync();

            return ToWarehouseCompanyListItems(result);
        }

        private List<WarehouseCompanyListItem> ToWarehouseCompanyListItems(List<Company> companies, bool limit = false)
        {
            var actor = GetCurrentActor();
            var warehousesCompanies = new List<WarehouseCompanyListItem>();
            var isCarrier = actor != null && actor.IsCarrier();

            var permissions = new List<Permission>();
            if (isCarrier)
            {
                permissions = _context.Permissions.Where(p => p.CarrierId == actor.Id).Include(p => p.PermissionsForDoor).ToList();
            }

            foreach (var wu in companies)
            {
                var amIParticipant = false;
                if (actor != null)
                {
                    amIParticipant = isCarrier ? false : wu.Id == actor.Company.Id;
                }

                // don't show other warehouses to warehouse managers
                if (actor != null && actor.IsWarehouse() && !amIParticipant)
                {
                    continue;
                }

                var listItem = new WarehouseCompanyListItem()
                {
                    company = WarehouseCompany.FromCompany(wu, amIParticipant),
                    warehouses = wu.Warehouses.Select(w => WarehouseListItem.FromWarehouse(w, amIParticipant, permissions, isCarrier)).ToList()
                };

                if (limit)
                {
                    if (actor != null && isCarrier)
                    {
                        listItem.warehouses = listItem.warehouses.Where(w => w.permission?.Status == PermissionStatus.Accepted).ToList();
                    }
                    else if (actor == null)
                    {
                        listItem.warehouses = listItem.warehouses.Where(w => w.canCarrierCreateAnonymousReservation).ToList();
                    }
                }

                warehousesCompanies.Add(listItem);
            }

            foreach (var warehouseCompany in warehousesCompanies)
            {
                warehouseCompany.warehouses = warehouseCompany.warehouses.OrderBy(w => w.name).ToList();
                foreach (var warehouse in warehouseCompany.warehouses)
                {
                    warehouse.doors = warehouse.doors.OrderBy(w => w.name).ToList();
                }
            }

            return warehousesCompanies;
        }

        [AllowAnonymous]
        public async Task<WarehouseCompanyListItem> BookableWarehouses(int id)
        {
            List<Company> result = await _context.Companies
                .Where(c => c.Id == id)
                .Include(u => u.Warehouses.OrderBy(w => w.Name))
                .ThenInclude(w => w.Doors)
                .Include(u => u.Warehouses.OrderBy(w => w.Name))
                .ThenInclude(w => w.Image)
                .OrderBy(u => u.Name)
                .ToListAsync();

            var companies = ToWarehouseCompanyListItems(result, true);



            if (companies.Count == 0)
            {
                throw new ApplicationException("Not found");
            }

            return companies[0];
        }

        public async Task<List<WarehouseCompanyListItem>> MyList()
        {
            var warehousesList = await List();
            var myWarehouseList = new List<WarehouseCompanyListItem>();
            foreach (var w in warehousesList)
            {
                if (w.company.amIParticipant)
                {
                    myWarehouseList.Add(w);
                    continue;
                }

                var warehouses = w.warehouses.Where(w => w.permission?.Status == PermissionStatus.Accepted).OrderBy(w => w.name);
                if (warehouses.Count() > 0)
                {
                    w.warehouses = warehouses.ToList();
                    myWarehouseList.Add(w);
                }
            }

            return myWarehouseList;
        }

        [AllowAnonymous]
        public async Task<Warehouse> Get(int id)
        {
            User actor = GetCurrentActor();
            var warehouse = await _context.Warehouses
              .Where(w => w.Id == id)
              .Include(w => w.Company)
              .Include(w => w.CreatedBy)
              .Include(w => w.Availability)
                .ThenInclude(a => a.TimeWindows)
              .Include(w => w.Doors.OrderBy(d => d.Name))
                .ThenInclude(d => d.Availability)
                  .ThenInclude(a => a.TimeWindows)
              .Include(w => w.Permissions)
                .ThenInclude(p => p.PermissionsForDoor)
              .Include(w => w.Image)
              .FirstOrDefaultAsync();

            if (actor == null)
            {
                if (!warehouse.canCarrierCreateAnonymousReservation)
                {
                    throw new ApplicationException("Not authed!");
                }
            }

            if (!warehouse.canCarrierCreateAnonymousReservation && actor != null && actor.IsCarrier())
            {
                var permission = warehouse.Permissions.Where(p => p.CarrierId == actor.Id).FirstOrDefault();
                if (permission == null)
                {
                    warehouse.Doors = new List<Door>();
                }
                else
                {
                    if (permission.Type == PermissionType.ONLY_SPECIFIC_DOORS)
                    {
                        warehouse.Doors = warehouse.Doors.Where(door =>
                            permission.PermissionsForDoor.Any(p => p.DoorId == door.Id)
                        ).ToList();
                    }
                    else if (permission.Type == PermissionType.ONLY_TWO_PHASE)
                    {
                        warehouse.Doors = new List<Door>();
                    }
                }
            }

            return warehouse;
        }

        public async Task<ActionResult> Update([FromBody] Warehouse warehouse)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            Warehouse oldWarehouse = _context.Warehouses
              .Where(w => w.Id == warehouse.Id)
              .Include(w => w.Availability)
                .ThenInclude(a => a.TimeWindows)
              .FirstOrDefault();

            if (oldWarehouse == null) throw new ModelNotFoundException();
            if (oldWarehouse.CompanyId != actor.Company.Id) throw new AuthenticationException();

            oldWarehouse.Name = warehouse.Name;
            oldWarehouse.Description = warehouse.Description;
            oldWarehouse.Address = warehouse.Address;
            oldWarehouse.Latitude = warehouse.Latitude;
            oldWarehouse.Longitude = warehouse.Longitude;
            oldWarehouse.ContactEmail = warehouse.ContactEmail;
            oldWarehouse.ContactPhone = warehouse.ContactPhone;
            oldWarehouse.canCarrierDeleteReservation = warehouse.canCarrierDeleteReservation;
            oldWarehouse.canCarrierCreateAnonymousReservation = warehouse.canCarrierCreateAnonymousReservation;
            oldWarehouse.canCarrierEditReservation = warehouse.canCarrierEditReservation;
            Availability.Update(_context, oldWarehouse.Availability, warehouse.Availability);

            _context.Warehouses.Update(oldWarehouse);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class AddDoorRequest
        {
            public string name { get; set; }
            public string description { get; set; }
        }
        public async Task<DoorExcerptDto> AddDoor(int id, [FromBody] AddDoorRequest request)
        {
            User actor = GetCurrentActor();
            WarehouseAdminOnly();

            Warehouse warehouse = await _context.Warehouses
              .Where(w => w.Id == id)
              .Include(w => w.Availability)
                .ThenInclude(a => a.TimeWindows)
              .FirstOrDefaultAsync();

            Availability availability = new Availability()
            {
                MinimumNotice = warehouse.Availability.MinimumNotice,
                GranularityMinutes = warehouse.Availability.GranularityMinutes,
                TimeWindows = new List<TimeWindow>(),
                MaxArrivalInacurracy = warehouse.Availability.MaxArrivalInacurracy,
                WorkTimeFrom = warehouse.Availability.WorkTimeFrom,
                WorkTimeTo = warehouse.Availability.WorkTimeTo
            };
            foreach (TimeWindow tw in warehouse.Availability.TimeWindows)
            {
                availability.TimeWindows.Add(new TimeWindow()
                {
                    Start = tw.Start,
                    End = tw.End,
                    BookableWeekdays = new List<int> { 0, 1, 2, 3, 4, 5, 6 }
                });
            }

            Door door = new Door()
            {
                WarehouseId = warehouse.Id,
                Name = request.name,
                Description = request.description,
                Availability = availability
            };
            door.SetProperties(new DoorProperties()
            {
                Type = ReservationType.Free,
                BaseTime = TimeSpan.Zero,
                TimePerPallet = TimeSpan.Zero
            });

            var addedDoor = _context.Doors.Add(door);

            var warehouseReservationFields = _context.ReservationFields.AsNoTracking().Where(f => f.WarehouseId == warehouse.Id).Include(f => f.reservationFieldNames).AsNoTracking().ToList();

            foreach (var field in warehouseReservationFields)
            {
                var translations = field.reservationFieldNames;
                field.unattachAndDeriveFrom();
                field.Door = addedDoor.Entity;
                _context.Add(field);

                foreach (var fieldName in translations)
                {
                    fieldName.unattach();
                    fieldName.reservationField = field;
                    _context.Add(fieldName);
                }
            }

            await _context.SaveChangesAsync();

            return DoorExcerptDto.FromDoor(door);
        }

        public class AddWarehouseRequest
        {
            public string name { get; set; }
            public string description { get; set; }
            public string address { get; set; }
            public string contactEmail { get; set; }
            public string contactPhone { get; set; }
            public bool canCarrierEditReservation { get; set; }
            public bool canCarrierDeleteReservation { get; set; }
            public bool canCarrierCreateAnonymousReservation { get; set; }
        }
        public async Task<WarehouseExcerptDto> AddWarehouse([FromBody] AddWarehouseRequest request)
        {
            User actor = GetCurrentActor();
            WarehouseAdminOnly();

            Warehouse warehouse = new Warehouse()
            {
                CreatedBy = actor,
                Company = actor.Company,
                Name = request.name,
                Description = request.description,
                Address = request.address,
                ContactEmail = request.contactEmail,
                ContactPhone = request.contactPhone,
                canCarrierEditReservation = request.canCarrierEditReservation,
                canCarrierDeleteReservation = request.canCarrierDeleteReservation,
                canCarrierCreateAnonymousReservation = request.canCarrierCreateAnonymousReservation,
                Availability = new Availability()
                {
                    MinimumNotice = TimeSpan.Zero,
                    GranularityMinutes = 15,
                    TimeWindows = new List<TimeWindow>() {
                        new TimeWindow()
                        {
                        Start = TimeSpan.FromHours(9),
                        End = TimeSpan.FromHours(10)
                        }
                    }
                },
                CreatedAt = DateTime.UtcNow
            };

            var addedWarehouse = _context.Warehouses.Add(warehouse);
            var addedWarehouseEntity = addedWarehouse.Entity;

            var accountReservationFields = _context.ReservationFields.AsNoTracking().Where(f => f.CompanyId == actor.Company.Id).Include(f => f.reservationFieldNames).AsNoTracking().ToList();

            var newReservationFields = new List<ReservationField>();
            foreach (var field in accountReservationFields)
            {
                var translations = field.reservationFieldNames;
                field.unattachAndDeriveFrom();
                field.Warehouse = addedWarehouseEntity;
                _context.Add(field);

                foreach (var fieldName in translations)
                {
                    fieldName.unattach();
                    fieldName.reservationField = field;
                    _context.Add(fieldName);
                }
            }

            await _context.SaveChangesAsync();

            return WarehouseExcerptDto.FromWarehouse(warehouse);
        }

        public async Task<ActionResult> DeleteWarehouse(int id)
        {
            User actor = GetCurrentActor();
            Warehouse warehouse = await _context.Warehouses
              .Where(d => d.Id == id)
              .Where(d => d.CompanyId == actor.Company.Id)
              .Include(d => d.Permissions)
              .Include(d => d.ReservationFields)
              .Include(d => d.Doors)
              .ThenInclude(d => d.Availability)
              .Include(d => d.Doors)
              .ThenInclude(d => d.ReservationFields)
              .FirstOrDefaultAsync();

            if (warehouse == null) throw new AuthenticationException();

            foreach (var door in warehouse.Doors)
            {
                var reservations = await _context.Reservations.Where(r => r.DoorId == door.Id).Include(r => r.Files).Include(r => r.ReservationStatusUpdates).ToListAsync();
                var recReservations = await _context.RecurringReservations.Where(r => r.DoorId == door.Id).Include(r => r.Files).ToListAsync();

                _context.RemoveRange(reservations);
                _context.RemoveRange(recReservations);

                _context.Doors.Remove(door);
            }

            var images = await _context.Files.Where(f => f.WarehouseId == warehouse.Id).ToListAsync();
            _context.RemoveRange(images);

            _context.Warehouses.Remove(warehouse);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}