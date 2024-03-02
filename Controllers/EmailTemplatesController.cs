using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwlApi.Exceptions;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    [Authorize]
    public class EmailTemplatesController : BaseController
    {
        public EmailTemplatesController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        public class SetEmailTemplateRequest
        {
            public EmailTemplateType Type { get; set; }
            public string SubjectTemplate { get; set; }
            public string ContentTemplate { get; set; }
            public int LanguageId { get; set; }
        }

        public async Task<EmailTemplate> SetEmailTemplate([FromBody] SetEmailTemplateRequest request)
        {
            WarehouseAdminOnly();
            var actor = GetCurrentActor();
            var language = await _context.AppLanguages.Where(l => l.Id == request.LanguageId).FirstOrDefaultAsync();
            if (language == null)
            {
                throw new ApplicationException("Not found");
            }

            var templateOfType = await _context.EmailTemplates.Where(t => t.Type == request.Type && t.CompanyId == actor.Company.Id && t.LanguageId == request.LanguageId).FirstOrDefaultAsync();
            if (templateOfType == null)
            {
                var addedTemplate = _context.EmailTemplates.Add(new EmailTemplate()
                {
                    Company = actor.Company,
                    ContentTemplate = request.ContentTemplate,
                    Language = language,
                    SubjectTemplate = request.SubjectTemplate,
                    Type = request.Type
                });
                await _context.SaveChangesAsync();
                return addedTemplate.Entity;
            }
            else
            {
                templateOfType.ContentTemplate = request.ContentTemplate;
                templateOfType.SubjectTemplate = request.SubjectTemplate;
                var updatedTemplate = _context.EmailTemplates.Update(templateOfType);
                await _context.SaveChangesAsync();
                return updatedTemplate.Entity;
            }
        }

        public async Task<List<EmailTemplate>> GetEmailTemplates(int id)
        {
            var actor = GetCurrentActor();
            var templates = await _context.EmailTemplates.Where(t => t.CompanyId == actor.Company.Id && t.LanguageId == id).ToListAsync();
            return templates;
        }

        public class GetEmailTemplateRequest
        {
            public EmailTemplateType Type { get; set; }
            public int LanguageId { get; set; }
        }

        public async Task<EmailTemplate> GetEmailTemplate([FromBody] GetEmailTemplateRequest request)
        {
            var actor = GetCurrentActor();
            var language = await _context.AppLanguages.Where(l => l.Id == request.LanguageId).FirstOrDefaultAsync();
            if (language == null)
            {
                throw new ApplicationException("Not found");
            }

            var template = await _context.EmailTemplates.Where(t => t.CompanyId == actor.Company.Id && t.Type == request.Type && t.LanguageId == request.LanguageId).FirstOrDefaultAsync();
            if (template == null)
            {
                return new EmailTemplate()
                {
                    Language = language,
                    ContentTemplate = "",
                    SubjectTemplate = "",
                    Type = request.Type,
                    Company = actor.Company,
                };
            };

            return template;
        }

        public enum AttachmentType
        {
            COMPANY,
            WAREHOUSE,
            DOOR
        }

        [DisableRequestSizeLimit]
        public async Task<File> UploadAttachment(int id, [FromForm] IFormFile attachmentFile, [FromForm] AttachmentType type, [FromForm] int? languageId)
        {
            User actor = GetCurrentActor();
            WarehouseAdminOnly();

            Company company = null;
            Warehouse warehouse = null;
            Door door = null;


            if (type == AttachmentType.COMPANY)
            {
                company = actor.Company;
            }
            else if (type == AttachmentType.WAREHOUSE)
            {
                warehouse = await _context.Warehouses.Where(w => w.Id == id && w.CompanyId == actor.Company.Id).FirstOrDefaultAsync();
                if (warehouse == null)
                {
                    throw new ApplicationException("Warehouse not found");
                }
            }
            else if (type == AttachmentType.DOOR)
            {
                door = await _context.Doors.Where(d => d.Id == id && d.Warehouse.CompanyId == actor.Company.Id).FirstOrDefaultAsync();
                if (door == null)
                {
                    throw new ApplicationException("Door not found");
                }
            }
            else
            {
                throw new ApplicationException("Invalid type");
            }

            File fileModel = await FileController.CreateFile(attachmentFile, _configuration.GetSection("FilePath").Value);
            fileModel.CompanyAttachment = company;
            fileModel.WarehouseAttachment = warehouse;
            fileModel.DoorAttachment = door;
            var addedFile = _context.Files.Add(fileModel);
            await _context.SaveChangesAsync();

            return addedFile.Entity;
        }

        public class UpdateAttachmentRequest
        {
            public int? LanguageId { get; set; }
        }

        public async Task<File> UpdateAttachment(int id, [FromBody] UpdateAttachmentRequest request)
        {
            var actor = GetCurrentActor();
            var file = await ValidateAccessToAttachment(id);
            if (file == null)
            {
                throw new ModelNotFoundException();
            }

            if (request.LanguageId == null)
            {
                file.Language = null;
                file.LanguageId = null;
                _context.Update(file);
            }
            else
            {
                AppLanguage language = await _context.AppLanguages.Where(l => l.Id == request.LanguageId).FirstOrDefaultAsync();
                if (language != null)
                {
                    file.Language = language;
                    _context.Update(file);
                }
            }


            await _context.SaveChangesAsync();

            return file;
        }

        public async Task<List<File>> GetCompanyAttachments()
        {
            var actor = GetCurrentActor();
            var files = await _context.Files.Where(f => f.CompanyAttachmentId == actor.Company.Id).Include(a => a.Language).ToListAsync();
            return files;
        }

        public async Task<List<File>> GetWarehouseAttachments(int id)
        {
            var actor = GetCurrentActor();
            var files = await _context.Files.Where(f => f.WarehouseAttachmentId == id && f.WarehouseAttachment.CompanyId == actor.Company.Id).Include(a => a.Language).ToListAsync();
            return files;
        }

        public async Task<List<File>> GetDoorAttachments(int id)
        {
            var actor = GetCurrentActor();
            var files = await _context.Files.Where(f => f.DoorAttachmentId == id && f.DoorAttachment.Warehouse.CompanyId == actor.Company.Id).Include(f => f.Language).ToListAsync();
            return files;
        }

        public async Task<ActionResult> DeleteAttachment(int id)
        {
            var actor = GetCurrentActor();
            WarehouseAdminOnly();

            var file = await ValidateAccessToAttachment(id);

            _context.Files.Remove(file);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<File> ValidateAccessToAttachment(int id)
        {
            var actor = GetCurrentActor();

            var file = await _context.Files.Where(f => f.Id == id)
                                        .Include(f => f.CompanyAttachment)
                                        .Include(f => f.WarehouseAttachment)
                                        .Include(f => f.DoorAttachment)
                                        .ThenInclude(d => d.Warehouse)
                                        .FirstOrDefaultAsync();

            if (file.CompanyAttachment != null)
            {
                if (file.CompanyAttachment.Id != actor.Company.Id)
                {
                    throw new ApplicationException("not allowed");
                }
            }
            else if (file.WarehouseAttachment != null)
            {
                if (file.WarehouseAttachment.CompanyId != actor.Company.Id)
                {
                    throw new ApplicationException("not allowed");
                }
            }
            else if (file.DoorAttachment != null)
            {
                if (file.DoorAttachment.Warehouse.CompanyId != actor.Company.Id)
                {
                    throw new ApplicationException("not allowed");
                }
            }
            else
            {
                throw new ApplicationException("not allowed");
            }

            return file;
        }

        public static void GeneratDefaultEmailTemplatesForCompany(OwlApiContext context, long companyId)
        {
            var company = context.Companies.Where(c => c.Id == companyId).FirstOrDefault();
            if (company == null)
            {
                return;
            }

            var existingTemplates = context.EmailTemplates.Where(t => t.CompanyId == companyId).ToList();
            var allLanguages = context.AppLanguages.ToList();

            var emailTemplateTypes = (EmailTemplateType[])Enum.GetValues(typeof(EmailTemplateType));
            foreach (var type in emailTemplateTypes)
            {
                foreach (var language in allLanguages)
                {
                    var existingTemplate = existingTemplates.Where(t => t.LanguageId == language.Id && t.Type == type).FirstOrDefault();
                    if (existingTemplate != null)
                    {
                        continue;
                    }

                    var contentTemplate = GetDefaultTemplate(language, type);
                    var subjectTemplate = GetDefaultSubject(language, type);
                    var template = new EmailTemplate()
                    {
                        Company = company,
                        ContentTemplate = contentTemplate,
                        SubjectTemplate = subjectTemplate,
                        Language = language,
                        Type = type,
                    };

                    context.Add(template);
                }
            }
        }

        public static string GetDefaultTemplate(AppLanguage language, EmailTemplateType type)
        {
            if (language.localeId == "en-US")
            {
                return GetDefaultTemplateEnglish(type);
            }
            else if (language.localeId == "sl")
            {
                return GetDefaultTemplateSlovenian(type);
            }

            throw new Exception("GetDefaultTemplate: unknown language " + language.localeId);
        }

        public static string GetDefaultTemplateSlovenian(EmailTemplateType Type)
        {
            return "<p>Pozdravljeni {{recipient_name}},</p><p>" + GetIntroTextSlovenian(Type) + "</p><p><br></p><p>Koda Vaše rezervacije je:</p><p><strong>{{reservation_code}}</strong></p><p><br></p><p>Rezervacijo si lahko ogledate in urejate na tej povezavi: {{reservation_link}}.</p><p><br></p><p><strong>Informacije o prevozniku:</strong></p><p>Podjetje: {{carrier_company_name}}</p><p>Naslov: {{carrier_address}}</p><p>Kontaktna oseba: {{carrier_contact_person_name}}</p><p>Kontaktni telefon: {{carrier_phone}}</p><p><br></p><p><strong>Informacije o skladišču:</strong></p><p>Naziv: {{warehouse_name}}</p><p>Naslov: {{warehouse_address}}</p><p>Kontaktni email: {{warehouse_email}}</p><p>Kontakni telefon: {{warehouse_phone}}</p><p>Lokacija: {{warehouse_location}}</p><p><br></p><p><strong>Pomembni podatki o dostavi<br></strong></p><p>Skladišče je odprto med {{warehouse_worktime_from}} in {{warehouse_worktime_to}}. Prosimo, opravite dostavo v okolici {{warehouse_max_arrival_inaccuracy}} rezervacijskega časa.</p><p><br></p><p><strong>Čas rezervacije:</strong></p><p>{{reservation_date}}; {{reservation_time}}</p><p><br></p><p><strong>Podatki v rezervaciji:</strong></p><p>{{reservation_data}}</p><p><br></p><p>Lep pozdrav,</p><p>{{warehouse_company_name}}</p>";
        }

        public static string GetDefaultTemplateEnglish(EmailTemplateType Type)
        {
            return "<p>Hello {{recipient_name}},</p><p>" + GetIntroTextEnglish(Type) + "</p><p><br></p><p>The code for your reservation is:</p><p><strong>{{reservation_code}}</strong></p><p><br></p><p>You can view and edit your reservation here: {{reservation_link}}.</p><p><br></p><p><strong>Carrier information:</strong></p><p>Company: {{carrier_company_name}}</p><p>Address: {{carrier_address}}</p><p>Contact person: {{carrier_contact_person_name}}</p><p>Contact phone: {{carrier_phone}}</p><p><br></p><p><strong>Warehouse information:</strong></p><p>Name: {{warehouse_name}}</p><p>Address: {{warehouse_address}}</p><p>Contact email: {{warehouse_email}}</p><p>Contact phone: {{warehouse_phone}}</p><p>Location: {{warehouse_location}}</p><p><br></p><p><strong>Important delivery information</strong></p><p>Warehouse works from {{warehouse_worktime_from}} to {{warehouse_worktime_to}}. Please come within {{warehouse_max_arrival_inaccuracy}} of the reservation time.</p><p><br></p><p><strong>Reservation time:</strong></p><p>{{reservation_date}}; {{reservation_time}}</p><p><br></p><p><strong>Reservation data:</strong></p><p>{{reservation_data}}</p><p><br></p><p>Best regards,</p><p>{{warehouse_company_name}}</p>";
        }

        public static string GetIntroTextSlovenian(EmailTemplateType Type)
        {
            if (Type == EmailTemplateType.RESERVATION_CREATED)
            {
                return "Rezervacija je bila ustvarjena.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_CREATED)
            {
                return "Ponavljajoča rezervacija je bila ustvarjena.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_CREATED)
            {
                return "Dvofazna rezervacija je bila ustvarjena.";
            }
            else if (Type == EmailTemplateType.RESERVATION_UPDATED)
            {
                return "Rezervacija je bila urejena.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_UPDATED)
            {
                return "Ponavljajoča rezervacija je bila urejena.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_UPDATED)
            {
                return "Dvofazna rezervacija je bila urejena.";
            }
            else if (Type == EmailTemplateType.RESERVATION_DELETED)
            {
                return "Rezervacija je bila izbrisana.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_DELETED)
            {
                return "Ponavljajoča rezervacija je bila izbrisana.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_DELETED)
            {
                return "Dvofazna rezervacija je bila izbrisana.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_CONFIRMED)
            {
                return "Dvofazna rezervacija je bila potrjena.";
            }

            throw new Exception("GetIntroTextSlovenian invalid type" + Type);
        }

        public static string GetIntroTextEnglish(EmailTemplateType Type)
        {
            if (Type == EmailTemplateType.RESERVATION_CREATED)
            {
                return "Reservation has been created.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_CREATED)
            {
                return "Recurring reservation has been created.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_CREATED)
            {
                return "Two-phase reservation has been created.";
            }
            else if (Type == EmailTemplateType.RESERVATION_UPDATED)
            {
                return "Reservation has been updated.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_UPDATED)
            {
                return "Recurring reservation has been updated.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_UPDATED)
            {
                return "Two-phase reservation has been updated.";
            }
            else if (Type == EmailTemplateType.RESERVATION_DELETED)
            {
                return "Reservation has been deleted.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_DELETED)
            {
                return "Recurring reservation has been deleted.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_DELETED)
            {
                return "Two-phase reservation has been deleted.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_CONFIRMED)
            {
                return "Two-phase reservation has been confirmed.";
            }

            throw new Exception("GetIntroTextEnglish invalid type " + Type);
        }

        public static string GetDefaultSubject(AppLanguage language, EmailTemplateType type)
        {
            if (language.localeId == "en-US")
            {
                return GetDefaultSubjectEnglish(type);
            }
            else if (language.localeId == "sl")
            {
                return GetDefaultSubjectSlovenian(type);
            }

            throw new Exception("GetDefaultSubject: unknown language " + language.localeId);
        }

        public static string GetDefaultSubjectSlovenian(EmailTemplateType type)
        {
            return "Potrditev dostave: {{carrier_title}} {{reservation_date}} {{reservation_time}} " + GetSubjectTextSlovenian(type);
        }

        public static string GetDefaultSubjectEnglish(EmailTemplateType type)
        {
            return "Delivery confirmation: {{carrier_title}} {{reservation_date}} {{reservation_time}} " + GetSubjectTextEnglish(type);
        }

        public static string GetSubjectTextSlovenian(EmailTemplateType Type)
        {
            if (Type == EmailTemplateType.RESERVATION_CREATED)
            {
                return "Rezervacija ustvarjena.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_CREATED)
            {
                return "Ponavljajoča rezervacija ustvarjena.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_CREATED)
            {
                return "Dvofazna rezervacija ustvarjena.";
            }
            else if (Type == EmailTemplateType.RESERVATION_UPDATED)
            {
                return "Rezervacija urejena.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_UPDATED)
            {
                return "Ponavljajoča rezervacija urejena.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_UPDATED)
            {
                return "Dvofazna rezervacija urejena.";
            }
            else if (Type == EmailTemplateType.RESERVATION_DELETED)
            {
                return "Rezervacija izbrisana.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_DELETED)
            {
                return "Ponavljajoča rezervacija izbrisana.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_DELETED)
            {
                return "Dvofazna rezervacija izbrisana.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_CONFIRMED)
            {
                return "Dvofazna rezervacija potrjena.";
            }

            throw new Exception("GetSubjectTextSlovenian invalid type " + Type);
        }

        public static string GetSubjectTextEnglish(EmailTemplateType Type)
        {
            if (Type == EmailTemplateType.RESERVATION_CREATED)
            {
                return "Reservation created.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_CREATED)
            {
                return "Recurring reservation created.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_CREATED)
            {
                return "Two-phase reservation created.";
            }
            else if (Type == EmailTemplateType.RESERVATION_UPDATED)
            {
                return "Reservation updated.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_UPDATED)
            {
                return "Recurring reservation updated.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_UPDATED)
            {
                return "Two-phase reservation updated.";
            }
            else if (Type == EmailTemplateType.RESERVATION_DELETED)
            {
                return "Reservation deleted.";
            }
            else if (Type == EmailTemplateType.RECCURING_RESERVATION_DELETED)
            {
                return "Recurring reservation deleted.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_DELETED)
            {
                return "Two-phase reservation deleted.";
            }
            else if (Type == EmailTemplateType.TWO_PHASE_RESERVATION_CONFIRMED)
            {
                return "Two-phase reservation confirmed.";
            }

            throw new Exception("GetSubjectTextEnglish invalid type " + Type);
        }
    }
}