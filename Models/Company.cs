using Newtonsoft.Json;
using OwlApi.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{

    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string ContactPerson { get; set; }
        public string RealmName { get; set; }
        public bool ShowFirstTimeProfileSetupNotice { get; set; }
        public DateTime FirstSyncedAt { get; set; }
        public DateTime LastSyncedAt { get; set; }
        public DateTime SyncDate { get; set; }

        [InverseProperty("Company")]
        public ICollection<Warehouse> Warehouses { get; set; }

        [JsonConverter(typeof(JsonBConverter<UserMailSendingData>))]
        [Column(TypeName = "jsonb")]
        public string MailSendingData { get; set; }

        [InverseProperty("Company")]
        public ICollection<ReservationField> ReservationFields { get; set; }

        public int? DefaultMailLanguageId { get; set; }

        [ForeignKey("DefaultMailLanguageId")]
        public AppLanguage DefaultMailLanguage { get; set; }

        public bool SendContractInMail { get; set; } = true;

        public bool DisableTwoPhaseReservations { get; set; } = false;

        [InverseProperty("Company")]
        public File Image { get; set; }

        public UserMailSendingData GetMailSendingData()
        {
            if (MailSendingData == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<UserMailSendingData>(MailSendingData);
        }

        public void SetMailSendingData(UserMailSendingData data)
        {
            MailSendingData = JsonConvert.SerializeObject(data);
        }
    }

    public class CompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string ContactPerson { get; set; }
        public string AuthServerName { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string profilePictureUrl { get; set; }
        public bool DisableTwoPhaseReservations { get; set; } = false;


        public static CompanyDto FromCompany(Company company)
        {
            return new CompanyDto()
            {
                Id = company.Id,
                Name = company.Name,
                Address = company.Address,
                Phone = company.Phone,
                ContactPerson = company.ContactPerson,
                AuthServerName = company.RealmName,
                DisableTwoPhaseReservations = company.DisableTwoPhaseReservations
            };
        }
    }
}
