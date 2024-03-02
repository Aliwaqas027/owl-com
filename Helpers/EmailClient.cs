using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OwlApi.Helpers
{
    public class EmailClient
    {
        private SmtpClient smtpClient;
        private IConfiguration config;
        private static Random random = new Random();
        private readonly IServiceScopeFactory _scopeFactory;
        private string filePath;
        private string pdfScriptPath;
        private string pythonPath;

        private string serverDomain;

        private EmailTemplates emailTemplates;

        public enum ReservationType
        {
            STANDARD,
            TWO_PHASE,
            CONFIRM_TWO_PHASE,
            RECURRING
        }

        public enum ReservationOperation
        {
            CREATE,
            UPDATE,
            DELETE
        }

        public EmailClient(IConfiguration config, IServiceScopeFactory scopeFactory, EmailTemplates templates)
        {
            smtpClient = new SmtpClient(config.GetSection("SMTP").GetValue<string>("Url"))
            {
                Port = 587,
                Credentials = new NetworkCredential(config.GetSection("SMTP").GetValue<string>("Username"), config.GetSection("SMTP").GetValue<string>("Password")),
                EnableSsl = true,
            };

            filePath = config.GetSection("FilePath").Value;
            pdfScriptPath = config.GetSection("PdfScriptPath").Value;
            pythonPath = config.GetSection("PythonPath").Value;
            serverDomain = config.GetSection("ServerDomain").Value;

            _scopeFactory = scopeFactory;

            emailTemplates = templates;

            this.config = config;
        }

        private bool WantsToReceiveMail(User user, ReservationOperation operation, ReservationType type)
        {
            var mailData = user.GetMailSendingData();
            return WantsToReceiveMail(mailData, operation, type);
        }

        private bool WantsToReceiveMail(Company company, ReservationOperation operation, ReservationType type)
        {
            var mailData = company.GetMailSendingData();
            return WantsToReceiveMail(mailData, operation, type);
        }

        private bool WantsToReceiveMail(UserMailSendingData mailData, ReservationOperation operation, ReservationType type)
        {
            // by default: true
            if (mailData == null)
            {
                return true;
            }

            bool? settingInQuestion = null;
            if (operation == ReservationOperation.DELETE)
            {
                if (type == ReservationType.CONFIRM_TWO_PHASE || type == ReservationType.TWO_PHASE)
                {
                    settingInQuestion = mailData.SendMailsForTwoPhaseReservationDeleted;
                }
                else if (type == ReservationType.STANDARD)
                {
                    settingInQuestion = mailData.SendMailsForReservationDeleted;
                }
                else if (type == ReservationType.RECURRING)
                {
                    settingInQuestion = mailData.SendMailsForRecurringReservationDeleted;
                }
            }
            else if (operation == ReservationOperation.UPDATE)
            {
                if (type == ReservationType.CONFIRM_TWO_PHASE || type == ReservationType.TWO_PHASE)
                {
                    settingInQuestion = mailData.SendMailsForTwoPhaseReservationEdited;
                }
                else if (type == ReservationType.STANDARD)
                {
                    settingInQuestion = mailData.SendMailsForReservationEdited;
                }
                else if (type == ReservationType.RECURRING)
                {
                    settingInQuestion = mailData.SendMailsForRecurringReservationEdited;
                }
            }
            else
            {
                if (type == ReservationType.CONFIRM_TWO_PHASE)
                {
                    settingInQuestion = mailData.SendMailsForTwoPhaseReservationConfirmed;
                }
                else if (type == ReservationType.TWO_PHASE)
                {
                    settingInQuestion = mailData.SendMailsForTwoPhaseReservationCreated;
                }
                else if (type == ReservationType.STANDARD)
                {
                    settingInQuestion = mailData.SendMailsForReservationCreated;
                }
                else if (type == ReservationType.RECURRING)
                {
                    settingInQuestion = mailData.SendMailsForRecurringReservationCreated;
                }
            }

            return settingInQuestion != false;
        }

        private List<string> GetContactMails(User user, ReservationOperation operation, ReservationType type)
        {
            if (user == null)
            {
                return new List<string>();
            }

            var wantsToReceiveMail = WantsToReceiveMail(user, operation, type);
            if (!wantsToReceiveMail)
            {
                return new List<string>();
            }

            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                OwlApiContext _context = scope.ServiceProvider.GetRequiredService<OwlApiContext>();
                return _context.ContactMails.Where(c => user.Company == null ? c.UserId == user.Id : user.Company.Id == c.CompanyId).Select(c => c.Email).ToList();
            }
        }

        private List<string> GetContactMails(List<ReservationField> data, string additionalContactEmail)
        {
            var contactEmails = new List<string>();
            if (additionalContactEmail != null)
            {
                contactEmails.Add(additionalContactEmail);
            }

            var field = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.EMAIL);
            if (field != null)
            {
                contactEmails.Add(field.Value);
            }

            return contactEmails;
        }

        private List<string> GetContactMails(Company company, ReservationOperation operation, ReservationType type)
        {
            var wantsToReceiveMail = WantsToReceiveMail(company, operation, type);
            if (!wantsToReceiveMail)
            {
                return new List<string>();
            }

            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                OwlApiContext _context = scope.ServiceProvider.GetRequiredService<OwlApiContext>();
                return _context.ContactMails.Where(c => c.CompanyId == company.Id).Select(c => c.Email).ToList();
            }
        }

        private List<Models.File> GetAttachments(Company company, Warehouse warehouse, Door door, AppLanguage language)
        {
            if (company == null && warehouse == null && door == null)
            {
                return new List<Models.File>();
            }

            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                OwlApiContext _context = scope.ServiceProvider.GetRequiredService<OwlApiContext>();
                var attachments = new List<Models.File>();
                if (company != null)
                {
                    var companyAttachments = _context.Files.Where(a => a.CompanyAttachmentId == company.Id && (a.LanguageId == null || a.LanguageId == language.Id)).ToList();
                    attachments.AddRange(companyAttachments);
                }

                if (warehouse != null)
                {
                    var warehouseAttachments = _context.Files.Where(a => a.WarehouseAttachmentId == warehouse.Id && (a.LanguageId == null || a.LanguageId == language.Id)).ToList();
                    attachments.AddRange(warehouseAttachments);
                }

                if (door != null)
                {
                    var doorAttachments = _context.Files.Where(a => a.DoorAttachmentId == door.Id && (a.LanguageId == null || a.LanguageId == language.Id)).ToList();
                    attachments.AddRange(doorAttachments);
                }

                return attachments;
            }
        }

        public void Send(string subject, string body, string from, List<string> to, List<Models.File> attachmentsFiles)
        {
            if (to.Count == 0)
            {
                return;
            }

            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(from),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(to[0]);
                for (int i = 1; i < to.Count; i++)
                {
                    mailMessage.Bcc.Add(to[i]);
                }

                if (attachmentsFiles != null)
                {
                    foreach (var attachmentFile in attachmentsFiles)
                    {
                        try
                        {
                            var attachment = new Attachment(attachmentFile.Path, MediaTypeNames.Application.Octet);
                            attachment.Name = attachmentFile.Name;
                            mailMessage.Attachments.Add(attachment);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Mail attachment error");
                            Console.WriteLine(e.Message);
                        }
                    }
                }

                smtpClient.Send(mailMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine("Send email error");
                Console.WriteLine(e.Message);
            }
        }


        public async Task SendReservationDeletedMail(User carrier, Reservation reservation, string lang)
        {
            var html = await SendReservationConfirmationMail(reservation.additionalContactEmail, carrier, reservation.Id, reservation.Code, reservation.GetData(), reservation.Warehouse, reservation.Door,
                reservation.Date, reservation.Start, reservation.End, ReservationOperation.DELETE, ReservationType.STANDARD, lang);
        }

        public async Task SendTwoPhaseReservationDeletedMail(User carrier, Reservation reservation, string lang)
        {
            var html = await SendReservationConfirmationMail(reservation.additionalContactEmail, carrier, reservation.Id, reservation.Code, reservation.GetData(), reservation.Warehouse, reservation.Door,
                reservation.Date, reservation.Start, reservation.End, ReservationOperation.DELETE, ReservationType.TWO_PHASE, lang);
        }

        public async Task SendRecurringReservationDeletedMail(User carrier, RecurringReservation reservation, string lang)
        {
            var html = await SendReservationConfirmationMail(reservation.additionalContactEmail, carrier, reservation.Id, reservation.Code, reservation.GetData(), reservation.Warehouse, reservation.Door,
                null, reservation.Start, reservation.End, ReservationOperation.DELETE, ReservationType.RECURRING, lang);
        }

        private string GetImageUrl(Models.File image)
        {
            return image == null ? "" : @$"{serverDomain}/api/file/download/{image.Id}/{image.Name}";
        }

        private Models.File CreatePdfFile(int reservationId, ReservationType type, ReservationOperation operation)
        {
            var name = GetPdfFileName(reservationId, type, operation);
            var path = Path.Combine(filePath, name);

            var pdfToken = GeneratePdfToken();

            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                OwlApiContext _context = scope.ServiceProvider.GetRequiredService<OwlApiContext>();
                int? recurringReservationId = null;
                int? normalReservationId = null;
                if (type == ReservationType.RECURRING)
                {
                    recurringReservationId = reservationId;
                }
                else
                {
                    normalReservationId = reservationId;
                }

                Models.File file = new Models.File()
                {
                    RecurringReservationId = recurringReservationId,
                    ReservationId = normalReservationId,
                    Name = name,
                    Path = path,
                    PdfToken = pdfToken
                };

                if (operation != ReservationOperation.DELETE)
                {
                    _context.Files.Add(file);
                    _context.SaveChanges();
                }

                return file;
            }
        }

        private AppLanguage GetLanguage(string lang)
        {
            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                OwlApiContext _context = scope.ServiceProvider.GetRequiredService<OwlApiContext>();
                var language = _context.AppLanguages.Where(l => l.localeId == lang).FirstOrDefault();
                if (language == null)
                {
                    language = _context.AppLanguages.Where(l => l.localeId == "en-US").FirstOrDefault();
                }

                return language;
            }
        }

        public EmailTemplate GetEmailTemplate(ReservationOperation operation, ReservationType type, AppLanguage language, int companyId)
        {
            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                OwlApiContext _context = scope.ServiceProvider.GetRequiredService<OwlApiContext>();

                var emailTemplateType = EmailTemplate.OperationToType(operation, type);
                var emailTemplate = _context.EmailTemplates.Where(t => t.CompanyId == companyId && t.LanguageId == language.Id && t.Type == emailTemplateType).FirstOrDefault();
                if (emailTemplate == null)
                {
                    throw new ApplicationException("todo: default template");
                }

                return emailTemplate;
            }
        }

        public class GetDriverContractRequest
        {
            public string code { get; set; }
            public string name { get; set; }
        }

        private async Task<Models.File> GetDriverContractAttachment(int reservationId, List<ReservationField> data, AppLanguage language, Company company)
        {
            var yamasUrl = config["URL:YAMAS"];
            var syncUrl = $"{yamasUrl}/api/driverContract/fillDriverContractTemplateForSigningOwl";
            var syncBody = new JObject();

            syncBody.Add("token", config["Auth:YAMAS"]);
            syncBody.Add("reference", 0);
            syncBody.Add("countryId", language.CountryId);
            syncBody.Add("contractType", 1);
            syncBody.Add("companyRealmName", company.RealmName);

            var driverCode = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.YAMAS_DRIVER_CODE);
            var transportCompany = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.TRANSPORT_COMPANY);
            var driverName = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.DRIVER_NAME);
            var driverSurname = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.DRIVER_SURNAME);
            var driverFullName = driverName?.Value + " " + driverSurname?.Value;

            var registrationNumber = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.TRUCK_REGISTRATION_NUMBER);

            syncBody.Add("driverCode", driverCode?.Value);
            syncBody.Add("transportCompany", transportCompany?.Value);
            syncBody.Add("driverFullName", driverFullName);
            syncBody.Add("registrationNumber", registrationNumber?.Value);

            try
            {
                var response = await HttpHelper.JsonPostRequest(syncUrl, syncBody);
                string filesPath = config.GetSection("FilePath").Value;
                string fileName = $"{DateTime.UtcNow.Ticks}_yamas_contract.pdf";
                string filePath = Path.Combine(filesPath, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, await response.responseRaw.Content.ReadAsByteArrayAsync());

                var file = new Models.File()
                {
                    Name = fileName,
                    Path = filePath,
                    ReservationId = reservationId
                };

                using (IServiceScope scope = _scopeFactory.CreateScope())
                {
                    OwlApiContext _context = scope.ServiceProvider.GetRequiredService<OwlApiContext>();
                    var addedFile = _context.Files.Add(file);
                    await _context.SaveChangesAsync();
                    return addedFile.Entity;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<bool> IsCountryCombination(List<ReservationField> data, Company company)
        {
            var yamasUrl = config["URL:YAMAS"];
            var syncUrl = $"{yamasUrl}/api/country/getCombinationByCountries";
            var syncBody = new JObject();

            var transportCompany = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.TRANSPORT_COMPANY_COUNTRY);
            var loadingCountry = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.LOADING_COUNTRY);

            if (transportCompany == null || loadingCountry == null)
            {
                return false;
            }

            syncBody.Add("token", config["Auth:YAMAS"]);
            syncBody.Add("reference", 0);
            syncBody.Add("companyRealmName", company.RealmName);
            syncBody.Add("transportCompanyCountryId", transportCompany.Value);
            syncBody.Add("loadingCountryId", loadingCountry.Value);

            try
            {
                var response = await HttpHelper.JsonPostRequest(syncUrl, syncBody);
                var obj = JObject.Parse(response.response);
                return obj != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public async Task<string> SendReservationConfirmationMail(string additionalContactEmail, User carrier, int reservationId, string reservationCode, List<ReservationField> data, Warehouse warehouse, Door door, DateTime? date, TimeSpan? start, TimeSpan? end, ReservationOperation operation, ReservationType type, string lang)
        {
            try
            {
                var language = GetLanguage(lang);
                var template = GetEmailTemplate(operation, type, language, warehouse.Company.Id);

                List<string> carrierEmails = GetContactMails(carrier, operation, type);
                List<string> driverEmails = GetContactMails(data, additionalContactEmail);
                List<string> companyEmails = GetContactMails(warehouse.Company, operation, type);

                var pdfFile = CreatePdfFile(reservationId, type, operation);

                var isCountryCombination = await IsCountryCombination(data, warehouse.Company);

                var emailData = emailTemplates.ReservationConfirmationMail(new EmailTemplates.EmailReservationConfirmationData()
                {
                    headerData = new EmailTemplates.EmailHeaderData()
                    {
                        warehouseImageUrl = GetImageUrl(warehouse.Image),
                        carrierImageUrl = GetImageUrl(carrier?.Image),
                        warehouseName = warehouse.Name,
                        carrierTitle = carrier?.Title ?? ""
                    },
                    reservationData = new EmailTemplates.EmailReservationData()
                    {
                        carrier = carrier,
                        date = date,
                        door = door,
                        end = end,
                        type = type,
                        operation = operation,
                        reservationData = data,
                        start = start,
                        warehouse = warehouse,
                        reservationId = reservationId,
                        reservationCode = reservationCode,
                        pdfToken = pdfFile.PdfToken,
                        pdfFileId = pdfFile.Id,
                        isCountryCombination = isCountryCombination
                    },
                }, template);

                GeneratePdfAndWriteToFile(emailData.html, pdfFile.Path);

                var attachments = GetReservationFiles(reservationId, type);
                attachments.Add(pdfFile);

                Models.File driverContractFile = null;
                if (warehouse.Company.SendContractInMail)
                {
                    driverContractFile = await GetDriverContractAttachment(reservationId, data, language, warehouse.Company);
                }

                var attachmentsForCompany = new List<Models.File>();
                var generalAttachments = GetAttachments(warehouse.Company, warehouse, door, language);

                if (ShouldSendAttachments(warehouse.Company, operation))
                {
                    attachmentsForCompany.AddRange(attachments);
                    attachmentsForCompany.AddRange(generalAttachments);
                    if (driverContractFile != null)
                    {
                        attachmentsForCompany.Add(driverContractFile);
                    }
                }

                var attachmentsForCarrier = new List<Models.File>();

                if (ShouldSendAttachments(carrier, operation))
                {
                    attachmentsForCarrier.AddRange(attachments);
                    attachmentsForCarrier.AddRange(generalAttachments);
                    if (driverContractFile != null)
                    {
                        attachmentsForCarrier.Add(driverContractFile);
                    }
                }

                var attachmentsForDriver = new List<Models.File>();
                attachmentsForDriver.AddRange(attachments);
                attachmentsForDriver.AddRange(generalAttachments);
                if (driverContractFile != null)
                {
                    attachmentsForDriver.Add(driverContractFile);
                }

                Send(emailData.subject, emailData.html, "info@omniopti.eu", companyEmails, attachmentsForCompany);
                Send(emailData.subject, emailData.html, "info@omniopti.eu", carrierEmails, attachmentsForCarrier);
                Send(emailData.subject, emailData.html, "info@omniopti.eu", driverEmails, attachmentsForDriver);

                return emailData.html;
            }
            catch (Exception e)
            {
                Console.WriteLine("Send mail error");
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        private bool ShouldSendAttachments(User user, ReservationOperation operation)
        {
            if (user == null)
            {
                return false;
            }

            var sendingData = user.GetMailSendingData();
            return ShouldSendAttachments(sendingData, operation);
        }

        private bool ShouldSendAttachments(Company company, ReservationOperation operation)
        {
            var sendingData = company.GetMailSendingData();
            return ShouldSendAttachments(sendingData, operation);
        }

        private bool ShouldSendAttachments(UserMailSendingData sendingData, ReservationOperation operation)
        {
            if (sendingData == null)
            {
                return true;
            }

            if (operation == ReservationOperation.CREATE)
            {
                return sendingData.SendAttachmentForReservationCreated ?? true;
            }
            else if (operation == ReservationOperation.UPDATE)
            {
                return sendingData.SendAttachmentForReservationEdited ?? true;
            }
            else if (operation == ReservationOperation.DELETE)
            {
                return sendingData.SendAttachmentForReservationDeleted ?? true;
            }

            return true;
        }

        private List<Models.File> GetReservationFiles(int reservationId, ReservationType type)
        {
            var files = new List<Models.File>();

            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                OwlApiContext _context = scope.ServiceProvider.GetRequiredService<OwlApiContext>();
                if (type == ReservationType.RECURRING)
                {
                    files = _context.Files.Where(f => f.PdfToken == null && f.RecurringReservationId == reservationId).ToList();
                }
                else
                {
                    files = _context.Files.Where(f => f.PdfToken == null && f.ReservationId == reservationId).ToList();
                }
            }

            return files;
        }

        private string GeneratePdfToken()
        {
            int length = 30;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GetPdfFileName(int reservationId, ReservationType type, ReservationOperation operation)
        {
            return $"{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")}_{reservationId}_{type}_{operation}.pdf";
        }

        private bool GeneratePdfAndWriteToFile(string html, string filePath)
        {
            try
            {
                var res = new RunCmd().Run(pythonPath, pdfScriptPath, new string[] { html, filePath });
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error pdf");
                Console.WriteLine(e.Message);
            }

            return false;
        }
    }
}
