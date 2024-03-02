using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwlApi.Exceptions;
using OwlApi.Helpers;
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
    public class ManageController : BaseController
    {
        private EmailClient _emailClient;
        private ReservationHelper _reservationHelper;

        public ManageController(OwlApiContext context, IConfiguration configuration, EmailClient emailClient, ReservationHelper reservationHelper) : base(context, configuration)
        {
            _emailClient = emailClient;
            _reservationHelper = reservationHelper;
        }

        public List<Permission> Permissions()
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            List<Permission> permissions = _context.Permissions
              .Where(p => p.Warehouse.CompanyId == actor.Company.Id)
              .OrderByDescending(p => p.CreatedAt)
              .Include(p => p.Warehouse)
              .ThenInclude(w => w.Doors.OrderBy(d => d.Name))
              .Include(p => p.Carrier)
              .Include(p => p.PermissionsForDoor)
              .ThenInclude(d => d.Door)
              .ToList();

            return permissions;
        }

        private Permission GetPermission(int id)
        {
            Permission permission = _context.Permissions
              .Where(p => p.Id == id)
              .Include(p => p.Warehouse)
              .Include(p => p.PermissionsForDoor)
              .FirstOrDefault();

            if (permission == null) throw new ModelNotFoundException();
            User actor = GetCurrentActor();
            if (permission.Warehouse.CompanyId != actor.Company.Id) throw new AuthenticationException();
            return permission;
        }

        public class AcceptRequest
        {
            public PermissionType Type { get; set; }
            public List<int> AllowedDoors { get; set; }
        }

        public async Task<IActionResult> Accept(int id, [FromBody] AcceptRequest request)
        {
            Permission permission = GetPermission(id);

            permission.Status = PermissionStatus.Accepted;
            permission.Type = request.Type;

            _context.RemoveRange(permission.PermissionsForDoor);

            if (request.Type == PermissionType.ONLY_SPECIFIC_DOORS)
            {
                var doors = await _context.Doors.Where(d => d.WarehouseId == permission.WarehouseId && request.AllowedDoors.Contains(d.Id)).ToListAsync();

                foreach (var door in doors)
                {
                    var permissionForDoor = new PermissionForDoor()
                    {
                        Door = door,
                        Permission = permission,
                    };
                    _context.Add(permissionForDoor);
                }
            }

            await _context.SaveChangesAsync();
            return Ok("");
        }

        public async Task<IActionResult> Decline(int id)
        {
            Permission permission = GetPermission(id);

            permission.Status = PermissionStatus.Declined;
            await _context.SaveChangesAsync();

            return Ok("");
        }

        public async Task<IActionResult> Cancel(int id)
        {
            Permission permission = GetPermission(id);

            permission.Status = PermissionStatus.Declined;
            await _context.SaveChangesAsync();

            return Ok("");
        }

        public async Task<IActionResult> DeletePermission(int id)
        {
            Permission permission = GetPermission(id);

            _context.RemoveRange(permission.PermissionsForDoor);
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            return Ok("");
        }

        public class GetMyArrivalRequest
        {
            public string Code { get; set; }
        }

        [AllowAnonymous]
        public async Task<Reservation> GetMyArrival(int id, [FromBody] GetMyArrivalRequest request)
        {
            User actor = GetCurrentActor();

            IQueryable<Reservation> query = _context.Reservations
              .Where(r => r.Id == id)
              .Include(r => r.Door)
                .ThenInclude(d => d.Warehouse)
              .Include(r => r.Carrier)
              .Include(r => r.Files)
              .Where(r => r.Code == request.Code);

            var r = await query.FirstOrDefaultAsync();
            if (r == null)
            {
                throw new ApplicationException();
            }

            return r;
        }

        public async Task<Reservation> GetMyPendingTwoPhaseReservation(int id)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            IQueryable<Reservation> query = _context.Reservations
              .Where(r => r.Id == id && r.DoorId == null)
              .Include(r => r.Warehouse)
              .Include(r => r.Carrier)
              .Include(r => r.Files)
              .Where(r => r.Warehouse.CompanyId == actor.Company.Id);

            var r = await query.FirstOrDefaultAsync();
            if (r == null)
            {
                throw new ApplicationException();
            }

            return r;
        }

        public class GetReservationCompanyIdResponse
        {
            public int CompanyId { get; set; }
        }

        [AllowAnonymous]
        public async Task<GetReservationCompanyIdResponse> GetReservationCompanyId(int id, [FromBody] GetMyArrivalRequest request)
        {
            var reservation = await _context.Reservations
             .Where(r => r.Id == id)
             .Where(r => r.Code == request.Code)
             .Include(r => r.Warehouse)
             .Include(r => r.Door.Warehouse)
             .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw new ModelNotFoundException();
            }

            var warehouse = reservation.Warehouse ?? reservation.Door.Warehouse;

            return new GetReservationCompanyIdResponse()
            {
                CompanyId = warehouse.CompanyId
            };
        }

        [AllowAnonymous]
        public async Task<Reservation> EditReservation(int id, [FromBody] Reservation reservation)
        {
            if (reservation == null) throw new IncorrectRequest();

            User actor = GetCurrentActor();

            await _reservationHelper.CheckReservationValidity(reservation, actor, id);

            Reservation reservationToUpdate = _context.Reservations
              .Where(r => r.Id == id)
              .Include(r => r.Carrier)
              .Include(r => r.Warehouse)
              .ThenInclude(w => w.Company)
              .Include(r => r.Warehouse)
              .ThenInclude(w => w.CreatedBy)
              .Include(r => r.Door)
              .ThenInclude(d => d.Warehouse)
              .ThenInclude(w => w.Company)
              .Include(r => r.Door)
              .ThenInclude(d => d.Warehouse)
              .ThenInclude(d => d.Image)
              // .Where(r => r.DoorId == door.Id)
              .FirstOrDefault();
            if (reservationToUpdate == null) throw new ModelNotFoundException();

            var isConfirmOfTwoPhase = reservationToUpdate.DoorId == null;

            if (isConfirmOfTwoPhase && (actor == null || actor.IsCarrier()))
            {
                throw new IncorrectRequest();
            }

            if (actor == null)
            {
                if (reservationToUpdate.Code != reservation.Code)
                {
                    throw new IncorrectRequest();
                }
            }
            else if (actor.IsCarrier())
            {
                if (reservationToUpdate.Carrier.Id != actor.Id)
                {
                    throw new IncorrectRequest();
                }
            }
            else
            {
                if (reservationToUpdate.Door != null)
                {
                    if (reservationToUpdate.Door.Warehouse.CompanyId != actor.Company.Id)
                    {
                        throw new IncorrectRequest();
                    }
                }
                else
                {
                    if (reservationToUpdate.Warehouse.CompanyId != actor.Company.Id)
                    {
                        throw new IncorrectRequest();
                    }
                }
            }

            reservationToUpdate.Data = reservation.Data;
            if (reservation.DoorId != null)
            {
                reservationToUpdate.DoorId = reservation.DoorId;
            }

            reservationToUpdate.Date = reservation.Date;
            reservationToUpdate.Start = reservation.Start;
            reservationToUpdate.End = reservation.End;

            if (reservation.additionalContactEmail != null && reservation.additionalContactEmail.Trim().Length > 0)
            {
                reservationToUpdate.additionalContactEmail = reservation.additionalContactEmail;
            }

            _context.Reservations.Update(reservationToUpdate);
            await _context.SaveChangesAsync();

            reservationToUpdate.Door = _context.Doors.Where(w => w.Id == reservationToUpdate.DoorId).FirstOrDefault();
            reservationToUpdate.Warehouse = _context.Warehouses.Where(w => w.Id == reservationToUpdate.Door.WarehouseId).Include(w => w.Company).Include(w => w.Image).FirstOrDefault();

            return reservation;
        }
    }
}