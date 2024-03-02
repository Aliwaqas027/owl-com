using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwlApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    public enum RequestRef
    {
        YAMAS
    }

    public class BaseController : Controller
    {
        protected readonly OwlApiContext _context;
        protected readonly IConfiguration _configuration;
        private User actor = null;

        public BaseController(OwlApiContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        protected User GetCurrentActor()
        {
            if (actor != null) return actor;

            Claim claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null && claim.Value != null)
            {
                actor = _context.Users
                   .Where(u => u.KeycloakId == claim.Value)
                   .Include(u => u.Company)
                        .ThenInclude(c => c.Image)
                   .Include(u => u.Image)
                   .FirstOrDefault();
            }

            return actor;
        }

        protected void WarehouseAdminOnly()
        {
            User actor = GetCurrentActor();
            if (actor == null || !actor.IsWarehouseAdmin()) throw new AuthenticationException();
        }

        protected void WarehouseOnly()
        {
            User actor = GetCurrentActor();
            if (actor == null || !actor.IsWarehouse()) throw new AuthenticationException();
        }

        protected void CarrierOnly()
        {
            User actor = GetCurrentActor();
            if (actor == null || !actor.IsCarrier()) throw new AuthenticationException();
        }

        protected void WarehouseOrCarrier()
        {
            User actor = GetCurrentActor();
            if (actor == null || !actor.IsCarrier() && !actor.IsWarehouse()) throw new AuthenticationException();
        }

        protected async Task<string> ReadRequestBody()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        protected IActionResult FileCT(Stream stream, string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(fileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return File(stream, contentType, fileName);
        }

        protected IActionResult FileCT(byte[] data, string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(fileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return File(data, contentType, fileName);
        }

        protected void CheckToken(string token, RequestRef reference)
        {
            string refToken = null;
            if (reference == RequestRef.YAMAS)
            {
                refToken = _configuration["Auth:YAMAS"];
            }

            if (refToken == null || token != refToken)
            {
                throw new ApplicationException("Auth error");
            }
        }
    }
}
