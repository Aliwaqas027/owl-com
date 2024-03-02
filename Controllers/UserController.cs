using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class UserController : BaseController
    {
        public UserController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        public async Task<User> GetProfile()
        {
            var actor = GetCurrentActor();
            if (actor == null)
            {
                throw new ApplicationException("Not allowed");
            }

            return actor;
        }

        public async Task<User> Get(int id)
        {
            User actor = GetCurrentActor();
            if (actor.Id != id) throw new AuthenticationException();
            User user = await _context.Users
              .Where(u => u.Id == id)
              .FirstAsync();
            return user;
        }

        public class ChangeOWLDataRequest
        {
            public DataTableFieldNamesDisplayMode dataTableFieldNamesDisplayMode { get; set; }
            public string displayDateFormat { get; set; }
        }

        public async Task<IActionResult> ChangeOWLData([FromBody] ChangeOWLDataRequest data)
        {
            User actor = GetCurrentActor();
            actor.dataTableFieldNamesDisplayMode = data.dataTableFieldNamesDisplayMode;
            actor.DisplayDateFormat = data.displayDateFormat;
            _context.Update(actor);
            await _context.SaveChangesAsync();
            return Ok();
        }


        public class UpdateContactMailsRequest
        {
            public List<string> mails { get; set; }
        }

        public async Task<IActionResult> UpdateContactMails([FromBody] UpdateContactMailsRequest data)
        {
            User actor = GetCurrentActor();
            if (actor.IsCarrier())
            {
                var currentMails = await _context.ContactMails.Where(c => c.UserId == actor.Id).ToListAsync();
                _context.ContactMails.BulkDelete(currentMails);

                foreach (string mail in data.mails)
                {
                    _context.ContactMails.Add(new ContactMail()
                    {
                        Email = mail,
                        User = actor
                    });
                }
            }
            else
            {
                WarehouseAdminOnly();
                var currentMails = await _context.ContactMails.Where(c => c.CompanyId == actor.Company.Id).ToListAsync();
                _context.ContactMails.BulkDelete(currentMails);

                foreach (string mail in data.mails)
                {
                    _context.ContactMails.Add(new ContactMail()
                    {
                        Email = mail,
                        Company = actor.Company
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok("");
        }


        public class AddContactMailRequest
        {
            public string mail;
        }

        public async Task<ContactMail> AddContactMail([FromBody] AddContactMailRequest request)
        {

            User actor = GetCurrentActor();

            var mail = request.mail.Trim();
            var newMail = new ContactMail()
            {
                Email = mail,
            };

            if (actor.IsCarrier())
            {
                newMail.User = actor;
            }
            else
            {
                WarehouseAdminOnly();
                newMail.Company = actor.Company;
            }

            _context.ContactMails.Add(newMail);
            await _context.SaveChangesAsync();
            return newMail;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteContactMail(int id)
        {
            User actor = GetCurrentActor();
            if (actor.IsCarrier())
            {
                var mail = await _context.ContactMails.Where(m => m.Id == id && m.UserId == actor.Id).FirstOrDefaultAsync();
                _context.ContactMails.Remove(mail);
            }
            else
            {
                WarehouseAdminOnly();
                var mail = await _context.ContactMails.Where(m => m.Id == id && m.CompanyId == actor.Company.Id).FirstOrDefaultAsync();
                _context.ContactMails.Remove(mail);
            }

            await _context.SaveChangesAsync();

            return Ok("");
        }

        public async Task<UserMailSendingData> GetMailSendingData()
        {
            User actor = GetCurrentActor();
            if (actor.IsCarrier())
            {
                return actor.GetMailSendingData();
            }

            return actor.Company.GetMailSendingData();
        }

        public async Task<List<ContactMail>> GetContactMails()
        {
            User actor = GetCurrentActor();
            if (actor.IsCarrier())
            {
                return await _context.ContactMails.Where(c => c.UserId == actor.Id).ToListAsync();
            }

            return await _context.ContactMails.Where(c => c.CompanyId == actor.Company.Id).ToListAsync();
        }

        public async Task<ActionResult> UpdateMailSendingData([FromBody] UserMailSendingData data)
        {
            User actor = GetCurrentActor();
            if (actor.IsCarrier())
            {
                actor.SetMailSendingData(data);
                _context.Update(actor);
            }
            else
            {
                WarehouseAdminOnly();
                actor.Company.SetMailSendingData(data);
                _context.Update(actor.Company);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}