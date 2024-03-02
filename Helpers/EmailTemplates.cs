using Microsoft.Extensions.Configuration;
using OwlApi.Models;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OwlApi.Helpers
{
    public class EmailTemplates
    {
        private string serverDomain;
        private bool isProduction = false;

        public EmailTemplates(IConfiguration config)
        {
            serverDomain = config.GetSection("ServerDomain").Value;
            var isProductionFromConfig = config.GetValue<bool?>("IsProduction");
            if (isProductionFromConfig != null)
            {
                isProduction = (bool)isProductionFromConfig;
            }
        }

        public class EmailHeaderData
        {
            public string warehouseImageUrl { get; set; }
            public string carrierImageUrl { get; set; }
            public string warehouseName { get; set; }
            public string carrierTitle { get; set; }
        }
        private string Header(EmailHeaderData data)
        {
            return $@"
            <table style=""width: 100%; border-collapse: collapse; max-width: 1078px; font-size: 16px;"">
                <tr>
                    <td style=""border-collapse: collapse; border-bottom: 4px solid #ecf0f1; font-weight: bold;"">
                        {HeaderProfileImage(data.warehouseImageUrl, data.warehouseName)} 
                    </td>
                    <td style=""border-collapse: collapse; border-bottom: 4px solid #ecf0f1; text-align: right;"">
                        {HeaderProfileImage(data.carrierImageUrl, data.carrierTitle)} 
                    </td>
                </tr>
            </table>";
        }

        private string HeaderProfileImage(string profileImageUrl, string name)
        {
            if (profileImageUrl != null && profileImageUrl.Trim().Length > 0)
            {
                return $@"<img src=""{profileImageUrl}"" alt=""{name}"" height=""75"" style=""max-height: 75px;"">";
            }

            return $@"<span>{name}</span>";
        }

        private string ImageToBase64(string path)
        {
            if (path == null)
            {
                return null;
            }

            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                string file = Convert.ToBase64String(bytes);
                return file;
            }
            catch (Exception e)
            {
                Console.WriteLine("Base64 convert failed");
                Console.WriteLine(e);
            }

            return null;
        }
        private string Footer()
        {
            return $@"
            <table style=""width: 100%; border-collapse:collapse; max-width: 1078px;font-size: 16px;"">
                <tr>
                    <td style=""border-collapse:collapse; border-top: 4px solid #ecf0f1; font-size: 11px; text-align: center;"">
                        <small>Powered by OWL by Omniopti</small>
                    </td>
                </tr>
            </table>";
        }

        private string handleNullOrEmpty(object val)
        {
            if (val == null)
            {
                return "-";
            }

            var stringRep = val.ToString();

            if (stringRep.ToString().Trim().Length == 0)
            {
                return "-";
            }

            return stringRep.ToString();
        }

        public class EmailReservationData
        {
            public int reservationId { get; set; }
            public List<ReservationField> reservationData { get; set; }
            public User carrier { get; set; }
            public Warehouse warehouse { get; set; }
            public Door door { get; set; }
            public DateTime? date { get; set; }
            public TimeSpan? start { get; set; }
            public string reservationCode { get; set; }
            public TimeSpan? end { get; set; }
            public EmailClient.ReservationType type { get; set; }
            public EmailClient.ReservationOperation operation { get; set; }
            public string pdfToken { get; set; }
            public int pdfFileId { get; set; }
            public bool isCountryCombination { get; set; }
        }

        private string GetReservationDataHtml(EmailReservationData data)
        {
            var emailData = data.reservationData.Where(f => f.ShowInMail).Select(f => new string[] { string.Join(" / ", f.reservationFieldNames.Select(r => r.name.ToUpper())), f.Value ?? "-" });

            var bodyHtml = "<ol>";
            foreach (string[] row in emailData)
            {
                bodyHtml += "<li>";

                foreach (string item in row)
                {
                    if (item != null)
                    {
                        bodyHtml += $@"<span style=""white-space: pre-line"">{item}</span><br>";
                    }
                }

                bodyHtml += "<br><br></li>";
            }

            bodyHtml += "</ol>";

            return bodyHtml;
        }

        private string GetReservationLink(int reservationId, string reservationCode)
        {
            return GetLink(serverDomain + $"/reservation/{reservationId}/{reservationCode}");
        }

        public class EmailData
        {
            public string subject { get; set; }
            public string html { get; set; }
        }

        public class EmailReservationConfirmationData
        {
            public EmailHeaderData headerData { get; set; }

            public EmailReservationData reservationData { get; set; }
        }

        public class EmailTemplateReplacements
        {
            public string recipient_name { get; set; }

            public string warehouse_name { get; set; }
            public string warehouse_company_name { get; set; }
            public string warehouse_address { get; set; }
            public string warehouse_email { get; set; }
            public string warehouse_phone { get; set; }
            public string warehouse_location { get; set; }
            public string warehouse_worktime_from { get; set; }
            public string warehouse_worktime_to { get; set; }
            public string warehouse_max_arrival_inaccuracy { get; set; }

            public string carrier_company_name { get; set; }
            public string carrier_address { get; set; }
            public string carrier_contact_person_name { get; set; }
            public string carrier_phone { get; set; }

            public string reservation_date { get; set; }
            public string reservation_time { get; set; }

            public string reservation_data { get; set; }
            public string reservation_code { get; set; }
            public string reservation_link { get; set; }

            public bool is_combination { get; set; }
        }


        public EmailData ReservationConfirmationMail(EmailReservationConfirmationData data, EmailTemplate template)
        {
            var recipientName = data.reservationData.carrier?.Name;
            if (recipientName == null)
            {
                recipientName = ReservationField.FindFieldByMeaning(data.reservationData.reservationData, ReservationFieldSpecialMeaningField.DRIVER_NAME)?.Value;
                recipientName += ReservationField.FindFieldByMeaning(data.reservationData.reservationData, ReservationFieldSpecialMeaningField.DRIVER_SURNAME)?.Value;
            }

            var dateString = "";
            if (data.reservationData.date.HasValue)
            {
                dateString = data.reservationData.date.Value.ToString("dd. MM. yyyy");
            }

            dynamic replacements = new EmailTemplateReplacements()
            {
                recipient_name = recipientName ?? "",

                warehouse_name = data.reservationData.warehouse.Name,
                warehouse_company_name = data.reservationData.warehouse.Company.Name,
                warehouse_address = data.reservationData.warehouse.Address,
                warehouse_email = GetEmailLink(data.reservationData.warehouse),
                warehouse_phone = GetPhoneLink(data.reservationData.warehouse),
                warehouse_location = GetMapsLink(data.reservationData.warehouse),
                warehouse_worktime_from = GetWorkTimeFrom(data.reservationData.warehouse),
                warehouse_worktime_to = GetWorkTimeTo(data.reservationData.warehouse),
                warehouse_max_arrival_inaccuracy = GetMaxArrivalInacurracy(data.reservationData.warehouse),

                carrier_company_name = data.reservationData.carrier?.Title ?? "-",
                carrier_address = data.reservationData.carrier?.Address ?? "-",
                carrier_contact_person_name = data.reservationData.carrier?.Name ?? "-",
                carrier_phone = data.reservationData.carrier?.PhoneNumber ?? "-",

                reservation_date = dateString,
                reservation_time = data.reservationData.start + " - " + data.reservationData.end,

                reservation_data = GetReservationDataHtml(data.reservationData),
                reservation_code = data.reservationData.reservationCode,
                reservation_link = GetReservationLink(data.reservationData.reservationId, data.reservationData.reservationCode),

                is_combination = data.reservationData.isCountryCombination
            };

            var html = $@"
            <!DOCTYPE html>
            <html>
                <head>
                    <title></title>
                    <meta charset=""utf-8"">
                </head>
                <body>
                    {Header(data.headerData)}
                    {TemplateContent(replacements, template)}
                    {Footer()}
                </body>
            </html>
            ";

            var subject = TemplateSubject(replacements, template);
            if (!isProduction)
            {
                subject = $@"TEST {subject}";
            }

            return new EmailData()
            {
                subject = TemplateSubject(replacements, template),
                html = html
            };
        }

        private string PdfLink(EmailReservationData data)
        {
            return $@"
            <p>PDF dokument lahko snamete <a href=""{serverDomain}/api/file/reservationPdf/{data.pdfFileId}?token={data.pdfToken}"">tukaj</a>.</p>
            ";
        }

        public string GetMapsLink(Warehouse warehouse)
        {
            if (warehouse.Latitude == null || warehouse.Longitude == null)
            {
                return "-";
            }

            return GetLink($@"https://www.google.com/maps/place/{warehouse.Latitude},{warehouse.Longitude}");
        }

        public string GetEmailLink(Warehouse warehouse)
        {
            if (warehouse.ContactEmail == null)
            {
                return "-";
            }

            return GetLink(warehouse.ContactEmail, "mailto");
        }

        private string GetLink(string linkTo, string linkType = null)
        {
            string type = "";
            if (linkType != null)
            {
                type = linkType + ":";
            }

            return $@"<a href=""{type}{linkTo}"">{linkTo}</a>";
        }

        public string GetPhoneLink(Warehouse warehouse)
        {
            if (warehouse.ContactPhone == null)
            {
                return "-";
            }

            return GetLink(warehouse.ContactPhone, "tel");
        }

        public string GetWorkTimeFrom(Warehouse warehouse)
        {
            if (warehouse.Availability == null || warehouse.Availability.WorkTimeFrom == null)
            {
                return "-";
            }

            return warehouse.Availability.WorkTimeFrom.ToString();
        }


        public string GetWorkTimeTo(Warehouse warehouse)
        {
            if (warehouse.Availability == null || warehouse.Availability.WorkTimeTo == null)
            {
                return "-";
            }

            return warehouse.Availability.WorkTimeTo.ToString();
        }


        public string GetMaxArrivalInacurracy(Warehouse warehouse)
        {
            if (warehouse.Availability == null || warehouse.Availability.MaxArrivalInacurracy == null)
            {
                return "-";
            }

            return warehouse.Availability.MaxArrivalInacurracy.ToString();
        }

        public string TemplateContent(EmailTemplateReplacements replacements, EmailTemplate template)
        {
            var scribanTemplate = Template.Parse(template.ContentTemplate);
            var result = scribanTemplate.Render(replacements);
            return result;
        }

        public string TemplateSubject(EmailTemplateReplacements replacements, EmailTemplate template)
        {
            var scribanTemplate = Template.Parse(template.SubjectTemplate);
            var result = scribanTemplate.Render(replacements);
            return result;
        }
    }
}
