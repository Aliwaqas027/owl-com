using ChoETL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OwlApi.Exceptions;
using OwlApi.Helpers;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;
using static OwlApi.Helpers.EmailClient;

namespace OwlApi.Controllers
{
    [Authorize]
    public class CarrierController : BaseController
    {
        private EmailClient _emailClient;
        private ReservationHelper _reservationHelper;

        public CarrierController(OwlApiContext context, IConfiguration configuration, EmailClient emailClient, ReservationHelper reservationHelper) : base(context, configuration)
        {
            _emailClient = emailClient;
            _reservationHelper = reservationHelper;
        }

        public async Task<List<Permission>> MyAcceptedWarehousePermissions()
        {
            User actor = GetCurrentActor();
            CarrierOnly();

            return await _context.Permissions.Where(p => p.CarrierId == actor.Id && p.Status == PermissionStatus.Accepted).Include(p => p.PermissionsForDoor).ToListAsync();
        }

        public async Task<List<Company>> MyList()
        {
            User actor = GetCurrentActor();
            CarrierOnly();

            var permissions = _context.Permissions.Where(p => p.CarrierId == actor.Id && p.Status == PermissionStatus.Accepted)
                        .Include(p => p.PermissionsForDoor)
                        .Include(p => p.Warehouse)
                        .ThenInclude(p => p.Company)
                        .Include(p => p.Warehouse)
                        .ThenInclude(p => p.Image)
                        .OrderBy(c => c.Warehouse.Company.Name)
                        .ToList();

            if (permissions.Count == 0)
            {
                return new List<Company>();
            }

            var groupedLists = new List<Company>();

            foreach (var permission in permissions)
            {
                var existingCompany = groupedLists.Find(r => r.Id == permission.Warehouse.CompanyId).FirstOrDefault<Company>();
                if (existingCompany == null)
                {
                    permission.Warehouse.Company.Warehouses = new List<Warehouse>() { permission.Warehouse };
                    groupedLists.Add(permission.Warehouse.Company);
                }
                else
                {
                    existingCompany.Warehouses.Add(permission.Warehouse);
                }
            }

            return groupedLists;
        }

        public async Task<Permission> CreatePermission(int id)
        {
            User actor = GetCurrentActor();
            CarrierOnly();

            Permission permission = _context.Permissions
              .Where(p => p.WarehouseId == id)
              .Where(p => p.CarrierId == actor.Id)
              .FirstOrDefault();
            if (permission != null) throw new IncorrectRequest();

            Warehouse warehouse = _context.Warehouses
              .Where(w => w.Id == id)
              .FirstOrDefault();

            if (warehouse == null) throw new ModelNotFoundException();

            permission = new Permission()
            {
                CarrierId = actor.Id,
                WarehouseId = id,
                Status = PermissionStatus.Pending,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                PermissionsForDoor = new List<PermissionForDoor>()
            };

            // auto accept if public warehouse
            if (warehouse.canCarrierCreateAnonymousReservation)
            {
                permission.Status = PermissionStatus.Accepted;
                permission.Type = PermissionType.ALL_DOORS;
            }

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return permission;
        }

        public async Task<List<ReservationDto>> MyReservations()
        {
            User actor = GetCurrentActor();

            if (actor == null)
            {
                throw new AuthenticationException();
            }

            bool forCarrier = true;
            if (actor.IsCarrier())
            {
                forCarrier = true;
            }
            else if (actor.IsWarehouse())
            {
                forCarrier = false;
            }
            else
            {
                throw new AuthenticationException();
            }

            var reservations = await _context.Reservations
              .Include(r => r.Door)
                .ThenInclude(r => r.Warehouse)
                .ThenInclude(r => r.Company)
              .Include(r => r.Warehouse)
                .ThenInclude(r => r.Company)
              .Where(r => !forCarrier || r.CarrierId == actor.Id)
              .Where(r => forCarrier || r.Door.Warehouse.CompanyId == actor.Company.Id || r.Warehouse.CompanyId == actor.Company.Id)
              .Where(r => r.Date >= DateTime.UtcNow.Date)
              .Where(r => r.YAMASArrivalId == null)
              .Include(r => r.Carrier)
              .Include(r => r.Files)
              .OrderBy(r => r.Date)
              .ThenBy(r => r.Start)
              .ToListAsync();

            var reccuringReservations = await _context.RecurringReservations
              .Include(r => r.Door)
                .ThenInclude(r => r.Warehouse)
                .ThenInclude(r => r.Company)
              .Include(r => r.Files)
              .Include(r => r.Warehouse)
                .ThenInclude(r => r.Company)
              .Where(r => !forCarrier || r.CarrierId == actor.Id)
              .Where(r => forCarrier || r.Door.Warehouse.CompanyId == actor.Company.Id)
              .Where(r => !(r.ToDate != null && r.ToDate < DateTime.UtcNow.Date)) // filter out past recurring reservations
              .Include(r => r.Carrier)
              .OrderBy(r => r.CreatedAt)
              .ThenBy(r => r.Start)
              .ToListAsync();

            var reservationDtos = new List<ReservationDto>();
            foreach (var reservation in reservations)
            {
                reservationDtos.Add(ReservationDto.FromReservation(reservation));
            }

            foreach (var reservation in reccuringReservations)
            {
                reservationDtos.Add(ReservationDto.FromRecurringReservation(reservation));
            }

            return reservationDtos;
        }

        public class ReservationsArchiveRequest
        {
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string SearchString { get; set; } = "";
            public string From { get; set; } = null;
            public string To { get; set; } = null;
            public string WarehouseId { get; set; } = null;
            public string DoorId { get; set; } = null;
            public string Cols { get; set; } = null;
            public string lang { get; set; } = null;
        }

        public class ReservationsArchiveResponse
        {
            public List<ReservationDto> data { get; set; }

            public int totalCount { get; set; }
        }

        public async Task<ReservationsArchiveResponse> ReservationsArchive([FromQuery] ReservationsArchiveRequest archiveParams)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            var query = GetQueryFromReservationsArchiveRequest(actor, archiveParams);
            int pageSize = Math.Min(archiveParams.PageSize, 20);

            List<Reservation> reservations = await query.Skip((archiveParams.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            for (int i = 0; i < reservations.Count; i++)
            {
                var reservation = reservations.ElementAt(i);
                reservation.Carrier.Reservations = null;

                await addRelevantNamesToReservationFields(reservation, archiveParams.lang, actor.dataTableFieldNamesDisplayMode);
            }

            return new ReservationsArchiveResponse()
            {
                data = reservations.Select(r => ReservationDto.FromReservation(r)).ToList(),
                totalCount = query.Count(),
            };
        }

        public async Task<List<ReservationDto>> ReservationsArchiveAll([FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            List<Reservation> reservations = await _context.Reservations
                .Where(r => r.Date < DateTime.UtcNow || r.YAMASArrivalId != null)
                .Include(r => r.Door)
                .ThenInclude(r => r.Warehouse)
                .Include(r => r.Carrier)
                .Include(r => r.Files)
                .Where(r => r.Door.Warehouse.CompanyId == actor.Company.Id)
                .OrderBy(r => r.Date)
                .ThenBy(r => r.Start)
                .ToListAsync();

            for (int i = 0; i < reservations.Count; i++)
            {
                var reservation = reservations.ElementAt(i);
                if (reservation.Carrier != null)
                {
                    reservation.Carrier.Reservations = null;
                }

                await addRelevantNamesToReservationFields(reservation, lang, actor.dataTableFieldNamesDisplayMode);
            }

            return reservations.Select(r => ReservationDto.FromReservation(r)).ToList();
        }

        private async Task addRelevantNamesToReservationFields(Reservation reservation, string lang, DataTableFieldNamesDisplayMode fieldNamesDisplayMode)
        {
            var reservationFields = reservation.GetData();
            var currentFields = new List<ReservationField>();
            if (fieldNamesDisplayMode == DataTableFieldNamesDisplayMode.PRESENT_TIME)
            {
                var fieldIds = reservationFields.Select(field => field.Id).ToList();
                currentFields = await _context.ReservationFields.AsNoTracking().Where(field => fieldIds.Contains(field.Id)).Include(field => field.reservationFieldNames).ThenInclude(name => name.language).AsNoTracking().ToListAsync();
            }

            foreach (var field in reservationFields)
            {
                var currentField = currentFields.Find(fieldDb => fieldDb.Id == field.Id);
                if (currentField == null)
                {
                    field.Name = SettingsController.getNameOfFieldByLocale(field, lang);
                }
                else
                {
                    field.Name = SettingsController.getNameOfFieldByLocale(currentField, lang);
                }
            }

            reservation.SetData(reservationFields);
        }

        public async Task<IActionResult> ReservationsArchiveCSV([FromQuery] ReservationsArchiveRequest archiveParams)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            const string EMPTY_VALUE = "-";

            var query = GetQueryFromReservationsArchiveRequest(actor, archiveParams);
            var reservations = await query.ToListAsync();

            var archiveCols = new List<long>();
            if (archiveParams.Cols != null)
            {
                var archiveColsStr = archiveParams.Cols.Split(",");
                foreach (var colStr in archiveColsStr)
                {
                    long colIdParsed;
                    var parseSuccess = long.TryParse(colStr, out colIdParsed);
                    if (parseSuccess)
                    {
                        archiveCols.Add(colIdParsed);
                    }
                }
            }

            JArray reservationsForJson = new JArray();
            var allColumns = new List<string>();

            foreach (var reservation in reservations)
            {
                JObject reservationForJson = new JObject();
                reservationForJson.Add("Date", reservation.Date.ToString("dd'.'MM'.'yyyy", CultureInfo.InvariantCulture));
                reservationForJson.Add("Start", reservation.Start);
                reservationForJson.Add("End", reservation.End);
                reservationForJson.Add("Warehouse", reservation.Door.Warehouse.Name);
                reservationForJson.Add("Door", reservation.Door.Name);
                reservationForJson.Add("Carrier", reservation.Carrier.Name);

                await addRelevantNamesToReservationFields(reservation, archiveParams.lang, actor.dataTableFieldNamesDisplayMode);

                var fields = reservation.GetData();
                int fieldIndex = 1;
                foreach (var field in fields)
                {
                    if (!archiveCols.IsNullOrEmpty())
                    {
                        if (!archiveCols.Contains(field.Id))
                        {
                            continue;
                        }
                    }

                    var name = field.Name;
                    if (name == null || name.Length == 0)
                    {
                        continue;
                    }

                    var value = field.Value;
                    if (value == null)
                    {
                        value = EMPTY_VALUE;
                    }

                    if (name.Trim().Length == 0)
                    {
                        name = "F_" + fieldIndex.ToString();
                    }

                    if (!reservationForJson.ContainsKey(name))
                    {
                        reservationForJson.Add(name, value);
                    }

                    if (!allColumns.Contains(name))
                    {
                        allColumns.Add(name);
                    }

                    fieldIndex++;
                }

                reservationsForJson.Add(reservationForJson);
            }

            foreach (JObject item in reservationsForJson)
            {
                foreach (var column in allColumns)
                {
                    if (!item.ContainsKey(column))
                    {
                        item.Add(column, EMPTY_VALUE);
                    }
                }
            }


            string json = reservationsForJson.ToString();

            StringBuilder csv = new StringBuilder();
            using (var p = new ChoJSONReader(new StringReader(json)))
            {
                using (var w = new ChoCSVWriter(new StringWriter(csv)).WithFirstLineHeader())
                {
                    w.Write(p);
                }
            }

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(csv.ToString());
            writer.Flush();
            stream.Position = 0;

            return FileCT(stream, "archive.csv");
        }

        private IQueryable<Reservation> GetQueryFromReservationsArchiveRequest(User actor, ReservationsArchiveRequest archiveParams)
        {
            DateTime? fromTime = parseDateString(archiveParams.From);
            DateTime? toTime = parseDateString(archiveParams.To);

            string searchString = "";
            if (archiveParams.SearchString != null)
            {
                searchString = archiveParams.SearchString.Trim().ToLower();
            }

            int[] warehouseIds = parseListOfIds(archiveParams.WarehouseId);
            int[] doorIds = parseListOfIds(archiveParams.DoorId);

            IQueryable<Reservation> query = _context.Reservations
             .Where(r => r.DoorId != null)
             .Where(r => r.Date < DateTime.UtcNow)
             .Include(r => r.Door)
               .ThenInclude(r => r.Warehouse)
             .Include(r => r.Carrier)
             .Include(r => r.Files)
             .Where(r => r.Door.Warehouse.CompanyId == actor.Company.Id);

            if (fromTime != null)
            {
                query = query.Where(r => r.Date >= fromTime.Value.Date);
            }

            if (toTime != null)
            {
                query = query.Where(r => r.Date <= toTime.Value.Date);
            }

            if (searchString != null && searchString.Length > 0)
            {
                query = query.Where(r =>
                    r.Door.Name.ToLower().Contains(searchString) ||
                    r.Door.Warehouse.Name.ToLower().Contains(searchString) ||
                    r.Carrier.Name.ToLower().Contains(searchString) ||
                    r.Carrier.Email.ToLower().Contains(searchString) ||
                    r.Carrier.Title.ToLower().Contains(searchString)
                );
            }

            if (warehouseIds.Length > 0)
            {
                query = query.Where(a => warehouseIds.Contains(a.Door.WarehouseId));
            }

            if (doorIds.Length > 0)
            {
                query = query.Where(a => doorIds.Contains(a.Door.Id));
            }

            query = query
                .OrderBy(r => r.Date)
                .ThenBy(r => r.Start);

            return query;
        }

        private int[] parseListOfIds(string listOfIds)
        {
            int[] ids = { };
            if (listOfIds != null)
            {
                ids = listOfIds.Split(",").Select(stri =>
                {
                    int parsedId;
                    var parseS = int.TryParse(stri, out parsedId);

                    if (parseS)
                    {
                        return parsedId;
                    }

                    return -1;
                }).Where(id => id > 0).ToArray();
            }

            return ids;
        }

        private DateTime? parseDateString(string dateStr)
        {
            if (dateStr == null)
            {
                return null;
            }

            try
            {
                return DateTime.ParseExact(dateStr, "d. M. yyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception e) { }

            return null;
        }

        [AllowAnonymous]
        public async Task<Reservation> Reserve([FromBody] Reservation reservation, [FromQuery(Name = "lang")] string lang)
        {
            Console.WriteLine("Reserve endpoint");

            if (reservation == null) throw new IncorrectRequest();
            User actor = GetCurrentActor();
            if (actor != null)
            {
                reservation.CarrierId = actor.Id;
            }
            else
            {
                reservation.CarrierId = null;
            }

            reservation.CreatedAt = DateTime.UtcNow;

            var language = await _context.AppLanguages.Where(l => l.localeId == lang).FirstOrDefaultAsync();
            reservation.Language = language;

            await _reservationHelper.CheckReservationValidity(reservation, actor);

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            _context.ReservationStatusUpdates.Add(getReservationStatusUpdate(ReservationStatus.AwaitingArrival, reservation, actor));
            await _context.SaveChangesAsync();

            reservation.Door = _context.Doors.Where(w => w.Id == reservation.DoorId).FirstOrDefault();
            reservation.Warehouse = _context.Warehouses.Where(w => w.Id == reservation.Door.WarehouseId).Include(w => w.Company).Include(w => w.Image).FirstOrDefault();

            return reservation;
        }


        public class SendReservationEmailRequest
        {
            public string reservationCode { get; set; }
            public ReservationOperation operation { get; set; }
            public EmailClient.ReservationType type { get; set; }
        }

        [AllowAnonymous]
        public async Task<ActionResult> SendReservationEmail(int id, [FromBody] SendReservationEmailRequest body, [FromQuery(Name = "lang")] string lang)
        {
            if (body.type == EmailClient.ReservationType.STANDARD)
            {
                var reservation = await _context.Reservations.Where(r => r.Id == id)
                    .Include(r => r.Carrier).Include(r => r.Door.Warehouse.Company).Include(r => r.Door.Warehouse.Image).Include(r => r.Door.Warehouse.Availability)
                    .FirstOrDefaultAsync();
                CanModifyReservation(reservation, body.reservationCode);
                await _emailClient.SendReservationConfirmationMail(reservation.additionalContactEmail, reservation.Carrier, reservation.Id, reservation.Code, reservation.GetData(), reservation.Door.Warehouse, reservation.Door,
                           reservation.Date, reservation.Start, reservation.End, body.operation, body.type, lang);
            }
            else if (body.type == EmailClient.ReservationType.TWO_PHASE || body.type == EmailClient.ReservationType.CONFIRM_TWO_PHASE)
            {
                var reservation = await _context.Reservations.Where(r => r.Id == id)
                    .Include(r => r.Carrier).Include(r => r.Warehouse).Include(r => r.Warehouse.Company).Include(r => r.Warehouse.Image).Include(r => r.Warehouse.Availability)
                    .FirstOrDefaultAsync();
                CanModifyReservation(reservation, body.reservationCode);
                await _emailClient.SendReservationConfirmationMail(reservation.additionalContactEmail, reservation.Carrier, reservation.Id, reservation.Code, reservation.GetData(), reservation.Warehouse, reservation.Door,
                           reservation.Date, reservation.Start, reservation.End, body.operation, body.type, lang);
            }
            else
            {
                var reservation = await _context.RecurringReservations.Where(r => r.Id == id)
                    .Include(r => r.Carrier).Include(r => r.Door.Warehouse.Company).Include(r => r.Door.Warehouse.Image).Include(r => r.Door.Warehouse.Availability)
                    .FirstOrDefaultAsync();
                CanModifyReservation(reservation, body.reservationCode);
                await _emailClient.SendReservationConfirmationMail(reservation.additionalContactEmail, reservation.Carrier, reservation.Id, reservation.Code, reservation.GetData(), reservation.Door.Warehouse, reservation.Door,
                           null, reservation.Start, reservation.End, body.operation, body.type, lang);
            }

            return Ok();
        }

        private void CanModifyReservation(Reservation reservation, string reservationCode)
        {
            var actor = GetCurrentActor();
            if (actor == null && (reservationCode == null || reservation.Code != reservationCode))
            {
                throw new ApplicationException("Not authorized");
            }
        }

        private void CanModifyReservation(RecurringReservation reservation, string reservationCode)
        {
            var actor = GetCurrentActor();
            if (actor == null && reservation.Code != reservationCode)
            {
                throw new ApplicationException("Not authorized");
            }
        }

        public async Task<Reservation> ReserveTwoPhase([FromBody] Reservation reservation)
        {
            if (reservation == null) throw new IncorrectRequest();
            if (reservation.WarehouseId == null) throw new IncorrectRequest();

            User actor = GetCurrentActor();
            WarehouseOrCarrier();

            reservation.CarrierId = actor.Id;

            // if (reservation.Date < DateTime.UtcNow.Date) throw new IncorrectRequest();

            reservation.CreatedAt = DateTime.UtcNow;
            reservation.Date = DateTime.UtcNow;
            await reservation.GenerateCode(_context);

            // todo: validation

            await _reservationHelper.CanReserve(actor, (int)reservation.WarehouseId, reservation);

            reservation.ReservationStatusUpdates.Add(getReservationStatusUpdate(ReservationStatus.AwaitingArrival, reservation, actor));

            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();

            reservation.Warehouse = _context.Warehouses.Where(w => w.Id == reservation.WarehouseId).Include(w => w.Image).Include(w => w.Company).FirstOrDefault();

            return reservation;
        }

        public class CancelReservationRequest
        {
            public string Code { get; set; }
        }

        [AllowAnonymous]
        public async Task<ActionResult> CancelReservation(int id, [FromBody] CancelReservationRequest request, [FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();

            Reservation reservation = await _context.Reservations
              .Where(r => r.Id == id)
              .Include(r => r.Files)
              .Include(r => r.Carrier)
              .ThenInclude(c => c.Image)
              .Include(r => r.Warehouse)
              .ThenInclude(d => d.Company)
              .Include(r => r.Door.Warehouse.Image)
              .Include(r => r.Door.Warehouse.Company)
              .Include(r => r.ReservationStatusUpdates)
              .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw new AuthenticationException();
            }

            if (actor == null)
            {
                if (request.Code != reservation.Code)
                {
                    throw new AuthenticationException();
                }
            }
            else if (actor.IsCarrier())
            {
                if (reservation.CarrierId != actor.Id) { throw new AuthenticationException(); }
            }
            else if (actor.IsWarehouse())
            {
                Warehouse refWarehouse = null;
                if (reservation.Warehouse != null)
                {
                    refWarehouse = reservation.Warehouse;
                }
                else if (reservation.Door.Warehouse != null)
                {
                    refWarehouse = reservation.Door.Warehouse;
                }

                if (refWarehouse == null)
                {
                    throw new ApplicationException("Ref warehouse is null!");
                }

                if (refWarehouse.CompanyId != actor.Company.Id) throw new AuthenticationException();
            }

            foreach (var statusUpdate in reservation.ReservationStatusUpdates)
            {
                _context.ReservationStatusUpdates.Remove(statusUpdate);
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            if (reservation.Door == null)
            {
                await _emailClient.SendTwoPhaseReservationDeletedMail(reservation.Carrier, reservation, lang);
            }
            else
            {
                reservation.Warehouse = reservation.Door.Warehouse;
                await _emailClient.SendReservationDeletedMail(reservation.Carrier, reservation, lang);
            }

            return Ok();
        }

        public async Task<RecurringReservation> ReserveRecurring([FromBody] RecurringReservation reservation, [FromQuery(Name = "lang")] string lang)
        {
            Console.WriteLine("ReserveRecurring endpoint");

            if (reservation == null) throw new IncorrectRequest();
            User actor = GetCurrentActor();
            if (actor != null)
            {
                reservation.CarrierId = actor.Id;
            }
            else
            {
                reservation.CarrierId = null;
            }

            var language = await _context.AppLanguages.Where(l => l.localeId == lang).FirstOrDefaultAsync();
            reservation.Language = language;

            if (reservation.Start > reservation.End)
            {
                Console.WriteLine("Start after end");
                throw new ApplicationException("Start after end");
            }

            reservation.CreatedAt = DateTime.UtcNow;
            reservation.GenerateCode(_context);

            if (reservation.RecurrenceRule != null)
            {
                if (reservation.RecurrenceRule.Trim().Length == 0)
                {
                    reservation.RecurrenceRule = null;
                }
                else
                {

                    var days = reservation.RecurrenceRule.Split(",");
                    foreach (string d in days)
                    {
                        try
                        {
                            int dayNum = int.Parse(d);
                            if (dayNum < (int)DayOfWeek.Sunday || dayNum > (int)DayOfWeek.Saturday)
                            {
                                Console.WriteLine("Invalid recurrence rule");
                                throw new ApplicationException("Invalid recurrence rule");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Invalid recurrence rule");
                            throw new ApplicationException("Invalid recurrence rule");
                        }
                    }
                }
            }

            Door door = _context.Doors
                  .Where(d => d.Id == reservation.DoorId)
                  .Include(d => d.Warehouse)
                  .Include(d => d.Availability)
                  .Include(d => d.Availability.TimeWindows
                    .Where(tw => tw.Start <= reservation.Start)
                    .Where(tw => tw.End >= reservation.End)
                  )
                  .FirstOrDefault();

            if (door == null)
            {
                Console.WriteLine("Door is missing");
                throw new ApplicationException("Door is missing");
            };

            if (door.Availability.TimeWindows.Count == 0)
            {
                Console.WriteLine("Door has no time windows");
                throw new ApplicationException("Door has no time windows");
            }

            DoorProperties properties = door.GetProperties();
            var palletsCount = reservation.GetPalletsCount();

            if (properties.Type == Models.ReservationType.Fixed)
            {
                bool flag = false;
                foreach (TimeWindow tw in door.Availability.TimeWindows)
                {
                    if (tw.Start == reservation.Start && tw.End == reservation.End)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    Console.WriteLine("Fixed reservation time window not found");
                    throw new ApplicationException("Fixed reservation time window not found");
                }
            }
            if (properties.Type == Models.ReservationType.Calculated)
            {
                TimeSpan duration = properties.BaseTime + palletsCount * properties.TimePerPallet;
                var diff = duration + reservation.Start - reservation.End;
                if (diff >= TimeSpan.FromSeconds(1))
                {
                    Console.WriteLine("Calculated reservation time window not matching");
                    throw new ApplicationException("Calculated reservation time window not matching " + (duration + reservation.Start).ToString());
                }
            }

            var warehouse = door.Warehouse;

            if (actor == null && !warehouse.canCarrierCreateAnonymousReservation)
            {
                throw new ApplicationException("Permission denied");
            }

            if (actor != null && !warehouse.canCarrierCreateAnonymousReservation)
            {
                if (actor.IsCarrier())
                {

                    Permission permission = _context.Permissions
                                           .Where(p => p.WarehouseId == door.WarehouseId)
                                           .Where(p => p.CarrierId == actor.Id)
                                           .Include(p => p.PermissionsForDoor)
                                           .FirstOrDefault();

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

            _context.RecurringReservations.Add(reservation);
            await _context.SaveChangesAsync();

            reservation.Door = _context.Doors.Where(w => w.Id == reservation.DoorId).FirstOrDefault();
            reservation.Warehouse = _context.Warehouses.Where(w => w.Id == reservation.Door.WarehouseId).Include(w => w.Company).Include(w => w.Image).FirstOrDefault();

            return reservation;
        }

        public class CancelRecurringReservationRequest
        {
            public bool completeCancel { get; set; } = false;
        }

        public async Task<bool> CancelRecurringReservation(int id, [FromBody] CancelRecurringReservationRequest req, [FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();
            WarehouseOrCarrier();

            RecurringReservation r = await _context.RecurringReservations
              .Where(r => r.Id == id)
              .Include(r => r.Files)
              .Include(r => r.Carrier)
              .Include(r => r.Door)
              .ThenInclude(d => d.Warehouse)
              .ThenInclude(d => d.Image)
              .Include(r => r.Door)
              .ThenInclude(d => d.Warehouse)

              .ThenInclude(d => d.Company)
              .Where(r => r.CarrierId == actor.Id || r.Door.Warehouse.CompanyId == actor.Company.Id)
              .FirstOrDefaultAsync();

            if (r == null) throw new ModelNotFoundException();

            if (req.completeCancel)
            {
                _context.RecurringReservations.Remove(r);
            }
            else
            {
                r.ToDate = DateTime.UtcNow.Date;
            }

            await _context.SaveChangesAsync();

            r.Warehouse = r.Door.Warehouse;
            await _emailClient.SendRecurringReservationDeletedMail(r.Carrier, r, lang);

            return true;
        }

        public class ChangeReservationStatusRequest
        {
            public ReservationStatus status;
        }
        public class ChangeReservationStatusResponse
        {
            public DateTime time;
            public ReservationStatus status;
        }
        public async Task<ChangeReservationStatusResponse> ChangeReservationStatus(int id, [FromBody] ChangeReservationStatusRequest request)
        {
            User actor = GetCurrentActor();
            WarehouseOnly();

            Reservation reservation = await _context.Reservations
              .Where(r => r.Id == id)
              .Include(r => r.Door)
                .ThenInclude(d => d.Warehouse)
              .Where(r => r.Door.Warehouse.CompanyId == actor.Company.Id)
              .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw new ModelNotFoundException();
            }

            var update = getReservationStatusUpdate(request.status, reservation, actor);
            _context.ReservationStatusUpdates.Add(update);

            await _context.SaveChangesAsync();

            return new ChangeReservationStatusResponse()
            {
                time = update.CreatedAt,
                status = request.status
            };
        }

        private ReservationStatusUpdate getReservationStatusUpdate(ReservationStatus status, Reservation reservation, User user)
        {
            return new ReservationStatusUpdate()
            {
                CreatedAt = DateTime.UtcNow,
                Reservation = reservation,
                status = status,
                User = user
            };
        }


        public class GetReservationRequest
        {
            public bool IsRecurring { get; set; }
            public string Code { get; set; }
        }

        [AllowAnonymous]
        public async Task<ReservationDto> GetReservation(int id, [FromBody] GetReservationRequest request)
        {
            User actor = GetCurrentActor();

            bool? forCarrier = null;
            if (actor != null)
            {
                if (actor.IsCarrier())
                {
                    forCarrier = true;
                }
                else if (actor.IsWarehouse())
                {
                    forCarrier = false;
                }
                else
                {
                    throw new AuthenticationException();
                }
            }

            if (request.IsRecurring)
            {
                var reccuringReservation = await _context.RecurringReservations
                  .Where(r => r.Id == id)
                  .Include(r => r.Door)
                    .ThenInclude(r => r.Warehouse)
                    .ThenInclude(r => r.Company)
                  .Include(r => r.Files)
                  .Include(r => r.Warehouse)
                    .ThenInclude(r => r.Company)
                  .Where(r => actor == null || forCarrier == false || r.CarrierId == actor.Id)
                  .Where(r => actor == null || forCarrier == true || r.Door.Warehouse.CompanyId == actor.Company.Id || r.Warehouse.CompanyId == actor.Company.Id)
                  .Where(r => actor != null || r.Code == request.Code)
                  .Where(r => !(r.ToDate != null && r.ToDate < DateTime.UtcNow.Date)) // filter out past recurring reservations
                  .Include(r => r.Carrier)
                  .OrderBy(r => r.CreatedAt)
                  .ThenBy(r => r.Start)
                  .FirstOrDefaultAsync();

                return ReservationDto.FromRecurringReservation(reccuringReservation);
            }
            else
            {
                // todo unify
                var reservation = await _context.Reservations
                  .Where(r => r.Id == id)
                  .Include(r => r.Door)
                    .ThenInclude(r => r.Warehouse)
                    .ThenInclude(r => r.Company)
                  .Include(r => r.Warehouse)
                    .ThenInclude(r => r.Company)
                  .Where(r => actor == null || forCarrier == false || r.CarrierId == actor.Id)
                  .Where(r => actor == null || forCarrier == true || r.Door.Warehouse.CompanyId == actor.Company.Id || r.Warehouse.CompanyId == actor.Company.Id)
                  .Where(r => actor != null || r.Code == request.Code)
                  .Where(r => r.Date >= DateTime.UtcNow.Date)
                  .Include(r => r.Carrier)
                  .Include(r => r.Files)
                  .OrderBy(r => r.Date)
                  .ThenBy(r => r.Start)
                  .FirstOrDefaultAsync();

                return ReservationDto.FromReservation(reservation);
            }
        }
    }
}