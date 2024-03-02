using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    public class SyncController : BaseController
    {
        public SyncController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        public class ProfileSyncData
        {
            public SyncUserDto user;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> SyncProfile()
        {
            var actor = GetCurrentActor();
            if (actor != null && actor.IsCarrier())
            {
                return Ok();
            }

            var dashUrl = _configuration["URL:Dashboard"];
            var syncUrl = $"{dashUrl}/api/sync/getCompanyAndUserSyncData";
            var syncBody = new JObject();

            Claim keycloakIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            syncBody.Add("token", _configuration["Auth:Dash"]);
            syncBody.Add("reference", 0);
            syncBody.Add("authServerUserId", keycloakIdClaim.Value);

            var syncDataJson = await HttpHelper.JsonPostRequest(syncUrl, syncBody);
            var syncData = JsonConvert.DeserializeObject<ProfileSyncData>(syncDataJson.response.ToString());

            var user = await _context.Users.Where(u => u.KeycloakId == syncData.user.AuthServerId).FirstOrDefaultAsync();
            var company = await _context.Companies.Where(c => c.RealmName == syncData.user.Company.AuthServerName).FirstOrDefaultAsync();
            if (company == null)
            {
                var language = await _context.AppLanguages.FirstOrDefaultAsync();
                company = _context.Companies.Add(new Company()
                {
                    FirstSyncedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    LastSyncedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    SyncDate = syncData.user.Company.LastUpdatedAt,
                    Name = syncData.user.Company.Name,
                    Address = syncData.user.Company.Address,
                    Phone = syncData.user.Company.Phone,
                    ContactPerson = syncData.user.Company.ContactPerson,
                    RealmName = syncData.user.Company.AuthServerName,
                    DefaultMailLanguage = language,
                    ShowFirstTimeProfileSetupNotice = true
                }).Entity;

                var languages = _context.AppLanguages.ToList();
                var fieldsForCompany = ReservationField.GetDefaultReservationFields(languages, company, null, null);
                _context.ReservationFields.AddRange(fieldsForCompany);
            }
            else
            {
                company.LastSyncedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                company.SyncDate = DateTime.SpecifyKind(syncData.user.Company.LastUpdatedAt, DateTimeKind.Utc);
                company.Name = syncData.user.Company.Name;
                company.Address = syncData.user.Company.Address;
                company.Phone = syncData.user.Company.Phone;
                company.ContactPerson = syncData.user.Company.ContactPerson;
                company.RealmName = syncData.user.Company.AuthServerName;
                _context.Companies.Update(company);
            }

            if (user == null)
            {
                user = new User()
                {
                    FirstSyncedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    LastSyncedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    SyncDate = DateTime.SpecifyKind(syncData.user.LastUpdatedAt, DateTimeKind.Utc),
                    Name = syncData.user.FirstName + " " + syncData.user.LastName,
                    Company = company,
                    Email = syncData.user.Email,
                    Roles = syncData.user.Roles.ToList(),
                    KeycloakId = syncData.user.AuthServerId,
                };

                var userEnt = _context.Users.Add(user);
                _context.ContactMails.Add(new ContactMail()
                {
                    User = userEnt.Entity,
                    Email = user.Email
                });
            }
            else
            {
                user.LastSyncedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                user.SyncDate = DateTime.SpecifyKind(syncData.user.LastUpdatedAt, DateTimeKind.Utc);
                user.Name = syncData.user.FirstName + " " + syncData.user.LastName;
                user.Company = company;
                user.Email = syncData.user.Email;
                user.Roles = syncData.user.Roles.ToList();
                user.KeycloakId = syncData.user.AuthServerId;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        public class GetWarehouseSyncDataRequest
        {
            public RequestRef reference { get; set; }
            public string token { get; set; }
            public string authServerUserId { get; set; }
        }

        public class SyncRampDto
        {
            public int Id { get; set; }
            public string Code { get; set; }
        }

        public class SyncWarehouseDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<SyncRampDto> Ramps { get; set; }
        }

        public class WarehouseSyncData
        {
            public List<SyncWarehouseDto> Warehouses { get; set; }
        }

        public async Task<WarehouseSyncData> GetWarehousesSyncData([FromBody] GetWarehouseSyncDataRequest request)
        {
            CheckToken(request.token, request.reference);
            var user = await _context.Users.Where(u => u.KeycloakId == request.authServerUserId)
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync();
            if (user == null)
            {
                throw new ApplicationException("Not found");
            }

            var warehouses = await _context.Warehouses.Where(w => w.CompanyId == user.Company.Id).Include(w => w.Doors).ToListAsync();
            var syncWarehouses = new List<SyncWarehouseDto>();
            foreach (var warehouse in warehouses)
            {
                var syncRamps = new List<SyncRampDto>();
                foreach (var ramp in warehouse.Doors)
                {
                    syncRamps.Add(new SyncRampDto()
                    {
                        Id = ramp.Id,
                        Code = ramp.Name
                    });
                }

                syncWarehouses.Add(new SyncWarehouseDto()
                {
                    Ramps = syncRamps,
                    Id = warehouse.Id,
                    Name = warehouse.Name
                });
            }

            return new WarehouseSyncData()
            {
                Warehouses = syncWarehouses
            };
        }

        public class AssignYamasArrivalToReservationRequest
        {
            public RequestRef reference { get; set; }
            public string token { get; set; }
            public string ReservationCode { get; set; }
            public int YAMASArrivalId { get; set; }
        }
        public async Task<ActionResult> AssignYamasArrivalToReservation([FromBody] AssignYamasArrivalToReservationRequest request)
        {
            CheckToken(request.token, request.reference);
            var reservation = await _context.Reservations.Where(r => r.Code == request.ReservationCode).FirstOrDefaultAsync();
            if (reservation == null)
            {
                throw new ApplicationException("Not found");
            }

            reservation.YAMASArrivalId = request.YAMASArrivalId;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
