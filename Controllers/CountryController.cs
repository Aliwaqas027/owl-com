using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwlApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    [Authorize]
    public class CountryController : BaseController
    {
        public CountryController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        [AllowAnonymous]
        public async Task<List<Country>> List()
        {
            List<Country> countries = await _context.Countries
             .OrderBy(c => c.name)
             .ToListAsync();

            return countries;
        }
    }
}