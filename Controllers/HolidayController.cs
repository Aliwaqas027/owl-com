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
using Z.EntityFramework.Plus;

namespace OwlApi.Controllers
{
    [Authorize]
    public class HolidayController : BaseController
    {

        public HolidayController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        public async Task<List<Holiday>> List()
        {
            User actor = GetCurrentActor();
            return await _context.Holidays.Where(h => h.CompanyId == actor.Company.Id).ToListAsync();
        }

        public class CreateRequest
        {
            public DateTime Date { get; set; }
        }

        public async Task<Holiday> Create([FromBody] CreateRequest request)
        {
            User actor = GetCurrentActor();
            Holiday h = new Holiday()
            {
                Company = actor.Company,
                Date = request.Date,
            };

            _context.Add(h);
            await _context.SaveChangesAsync();

            return h;
        }

        public async Task<IActionResult> Delete(int id)
        {
            User actor = GetCurrentActor();
            var holiday = await _context.Holidays.Where(h => h.CompanyId == actor.Company.Id && h.Id == id).FirstOrDefaultAsync();
            if (holiday == null)
            {
                return Ok();
            }

            _context.Remove(holiday);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}