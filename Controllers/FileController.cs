using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwlApi.Exceptions;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    public class FileController : BaseController
    {
        public FileController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(int id, List<IFormFile> files, string isRecurring = null, string reservationCode = null)
        {
            User actor = GetCurrentActor();

            if (files == null) throw new IncorrectRequest();

            bool isRecurringSafe = isRecurring != null && isRecurring == "true";

            if (isRecurringSafe)
            {
                RecurringReservation reservation = await _context.RecurringReservations
                  .Where(r => r.Id == id)
                  .Include(r => r.Warehouse)
                  .Include(r => r.Door)
                  .ThenInclude(d => d.Warehouse)
                  .FirstOrDefaultAsync();
                if (reservation == null)
                {
                    Console.WriteLine("Upload error: recurring reservation does not exist!");
                    throw new ModelNotFoundException();
                }

                if (actor == null)
                {
                    if (reservation.Code != reservationCode)
                    {
                        Console.WriteLine("Upload error: unauthed!");
                        throw new AuthenticationException();
                    }
                }
                else if (actor.IsCarrier())
                {
                    if (reservation.CarrierId != actor.Id)
                    {
                        Console.WriteLine("Upload error: recurring reservation wrong actor id!");
                        throw new AuthenticationException();
                    }
                }
                else
                {
                    var relevantWarehouse = reservation.Warehouse;
                    if (relevantWarehouse == null)
                    {
                        relevantWarehouse = reservation.Door.Warehouse;
                    }

                    if (relevantWarehouse == null)
                    {
                        throw new ApplicationException("Relevant warehouse is null");
                    }

                    if (relevantWarehouse.CompanyId != actor.Company.Id)
                    {
                        Console.WriteLine("Upload error: recurring reservation wrong actor id!");
                        throw new AuthenticationException();
                    }
                }
            }
            else
            {
                Reservation reservation = await _context.Reservations
                  .Where(r => r.Id == id)
                  .Include(r => r.Warehouse)
                  .Include(r => r.Door)
                  .ThenInclude(d => d.Warehouse)
                  .FirstOrDefaultAsync();
                if (reservation == null)
                {
                    Console.WriteLine("Upload error: reservation does not exist!");
                    throw new ModelNotFoundException();
                }

                if (actor == null)
                {
                    if (reservation.Code != reservationCode)
                    {
                        Console.WriteLine("Upload error: unauthed!");
                        throw new AuthenticationException();
                    }
                }
                else if (actor.IsCarrier())
                {
                    if (reservation.CarrierId != actor.Id)
                    {
                        Console.WriteLine("Upload error: recurring reservation wrong actor id!");
                        throw new AuthenticationException();
                    }
                }
                else
                {
                    var relevantWarehouse = reservation.Warehouse;
                    if (relevantWarehouse == null)
                    {
                        relevantWarehouse = reservation.Door.Warehouse;
                    }

                    if (relevantWarehouse == null)
                    {
                        throw new ApplicationException("Relevant warehouse is null");
                    }

                    if (relevantWarehouse.CompanyId != actor.Company.Id)
                    {
                        Console.WriteLine("Upload error: recurring reservation wrong actor id!");
                        throw new AuthenticationException();
                    }
                }
            }


            List<string> filePaths = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    string name = Path.GetFileName(formFile.FileName);
                    string fileName = $"{DateTime.UtcNow.Ticks}{name}";
                    string filePath = Path.Combine(_configuration.GetSection("FilePath").Value, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    int? ReservationId = null;
                    int? RecurringReservationId = null;
                    if (isRecurringSafe)
                    {
                        RecurringReservationId = id;
                    }
                    else
                    {
                        ReservationId = id;
                    }

                    Models.File file = new Models.File()
                    {
                        ReservationId = ReservationId,
                        RecurringReservationId = RecurringReservationId,
                        Name = name,
                        Path = filePath
                    };
                    _context.Files.Add(file);
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<Models.File> CreateFile(IFormFile file)
        {
            return await CreateFile(file, _configuration.GetSection("FilePath").Value);
        }

        public static async Task<Models.File> CreateFile(IFormFile file, string filesPath)
        {
            if (file == null) throw new IncorrectRequest();
            if (file.Length == 0) throw new IncorrectRequest();
            string name = Path.GetFileName(file.FileName);
            string fileName = $"{DateTime.UtcNow.Ticks}{name}";
            string filePath = Path.Combine(filesPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new Models.File()
            {
                Name = name,
                Path = filePath
            };
        }

        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            User actor = GetCurrentActor();
            WarehouseOrCarrier();

            Models.File fileModel = await CreateFile(file);

            if (actor.IsCarrier())
            {
                fileModel.UserId = actor.Id;
            }
            else if (actor.IsWarehouseAdmin())
            {
                fileModel.CompanyId = actor.Company.Id;
            }
            else
            {
                throw new ApplicationException("Not permitted");
            }

            _context.Files.Add(fileModel);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadWarehouseImage(int id, IFormFile file)
        {
            User actor = GetCurrentActor();
            WarehouseAdminOnly();

            var warehouse = await _context.Warehouses.Where(w => w.Id == id).Include(w => w.Company).FirstOrDefaultAsync();
            if (warehouse.Company.Id != actor.Company.Id)
            {
                throw new ApplicationException("Unauthorized");
            }

            var existingFile = await _context.Files.Where(f => f.WarehouseId == id).FirstOrDefaultAsync();
            if (existingFile != null)
            {
                _context.Remove(existingFile);
            }

            Models.File fileModel = await CreateFile(file);
            fileModel.WarehouseId = id;
            _context.Files.Add(fileModel);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Download(int id, string ending, [FromQuery] string reservationCode)
        {
            User actor = GetCurrentActor();
            Models.File file = await _context.Files
              .Where(f => f.Id == id)
              .Include(f => f.Reservation)
                .ThenInclude(r => r.Door)
                  .ThenInclude(w => w.Warehouse)
              .FirstOrDefaultAsync();
            if (file == null) throw new ModelNotFoundException();

            if (file.Reservation != null)
            {
                if (actor == null)
                {
                    if (reservationCode == null || file.Reservation.Code != reservationCode)
                    {
                        throw new ModelNotFoundException();
                    }
                }
                else
                {
                    if (file.Reservation.CarrierId != actor.Id && file.Reservation.Door.Warehouse.CompanyId != actor.Company.Id)
                    {
                        throw new AuthenticationException();
                    }
                }
            }

            var memory = new MemoryStream();
            using (FileStream stream = file.GetStream())
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return FileCT(memory, ending);
        }

        public async Task<IActionResult> ReservationDownload([FromQuery(Name = "name")] string name)
        {
            var memory = new MemoryStream();
            var path = Path.Combine(_configuration.GetSection("FilePath").Value, name);
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return FileCT(memory, name);
        }

        public async Task<IActionResult> ReservationPdf(int id, [FromQuery(Name = "token")] string token, [FromQuery(Name = "type")] string type)
        {
            User actor = GetCurrentActor();

            Models.File file = await _context.Files
                          .Where(f => f.Id == id && f.PdfToken == token)
                          .FirstOrDefaultAsync();

            if (file == null)
            {
                throw new ModelNotFoundException();
            }

            var memory = new MemoryStream();
            using (FileStream stream = file.GetStream())
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var fileName = Path.GetFileName(file.Path);

            return FileCT(memory, fileName);
        }
    }
}