using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OwlApi.Helpers;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    [Authorize]
    public class AuthenticationController : BaseController
    {
        KeycloakClient keycloakClient;
        public AuthenticationController(OwlApiContext context, IConfiguration configuration, KeycloakClient keycloakClient) : base(context, configuration)
        {
            this.keycloakClient = keycloakClient;
        }

        public class RegisterCarrierRequest
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public string PhoneNumber { get; set; }
            public string Password { get; set; }
        }
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCarrier([FromBody] RegisterCarrierRequest request)
        {
            User existingEmail = _context.Users
              .Where(u => u.Email.Equals(request.Email))
              .FirstOrDefault();

            if (existingEmail != null)
            {
                return Conflict();
            }

            var keycloakUser = await keycloakClient.CreateUserForCompany(_configuration["Authentication:CarrierRealm"], request.Email, request.Password,
                request.Name, "", null, new string[] { UserRole.Carrier });

            var user = new User()
            {
                FirstSyncedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                LastSyncedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                SyncDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                Name = request.Name,
                Title = request.Title,
                Company = null,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Roles = new List<string>() { UserRole.Carrier },
                KeycloakId = keycloakUser.Id,
            };

            var userEnt = _context.Users.Add(user);
            _context.ContactMails.Add(new ContactMail()
            {
                User = userEnt.Entity,
                Email = user.Email
            });

            var languages = _context.AppLanguages.ToList();

            _context.ReservationFields.AddRange(new List<ReservationField>());

            await _context.SaveChangesAsync();

            return Ok("");
        }

        public class UpdateProfileRequest
        {
            public string Name { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string Title { get; set; }
        }
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var actor = GetCurrentActor();
            var company = actor.Company;
            if (company == null && actor.IsCarrier())
            {
                company = new Company()
                {
                    RealmName = _configuration["Authentication:CarrierRealm"]
                };
            }

            var authorization = "Bearer " + await keycloakClient.GetAdminAccessToken();
            await keycloakClient.UpdateUserForCompany(company, actor.KeycloakId, null, request.Name, null, authorization, null);

            actor.Address = request.Address;
            actor.PhoneNumber = request.PhoneNumber;
            actor.Name = request.Name;
            actor.Title = request.Title;

            _context.Update(actor);
            await _context.SaveChangesAsync();

            return Ok("");
        }

        public class ChangePasswordRequest
        {
            public string Password { get; set; }
        }
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var actor = GetCurrentActor();
            var company = actor.Company;
            if (company == null && actor.IsCarrier())
            {
                company = new Company()
                {
                    RealmName = _configuration["Authentication:CarrierRealm"]
                };
            }

            var authorization = "Bearer " + await keycloakClient.GetAdminAccessToken();
            await keycloakClient.UpdateUserForCompany(company, actor.KeycloakId, request.Password, null, null, authorization, null);
            return Ok("");
        }
    }
}
