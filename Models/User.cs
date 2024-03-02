using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using OwlApi.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class UserMailSendingData
    {
        public bool? SendAttachmentForReservationCreated { get; set; }
        public bool? SendAttachmentForReservationEdited { get; set; }
        public bool? SendAttachmentForReservationDeleted { get; set; }
        public bool? SendMailsForReservationCreated { get; set; }
        public bool? SendMailsForReservationEdited { get; set; }
        public bool? SendMailsForReservationDeleted { get; set; }
        public bool? SendMailsForRecurringReservationCreated { get; set; }
        public bool? SendMailsForRecurringReservationEdited { get; set; }
        public bool? SendMailsForRecurringReservationDeleted { get; set; }
        public bool? SendMailsForTwoPhaseReservationCreated { get; set; }
        public bool? SendMailsForTwoPhaseReservationConfirmed { get; set; }
        public bool? SendMailsForTwoPhaseReservationEdited { get; set; }
        public bool? SendMailsForTwoPhaseReservationDeleted { get; set; }
    }

    public enum DataTableFieldNamesDisplayMode
    {
        RESERVATION_TIME,
        PRESENT_TIME
    }

    public class UserRole
    {
        public static string CompanyAdmin { get { return "company_admin"; } }
        public static string Warehouse { get { return "owl_warehouse"; } }
        public static string Carrier { get { return "owl_carrier"; } }
    }

    public class User : IUser<int>
    {
        public int Id { get; set; }
        public string KeycloakId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string DisplayDateFormat { get; set; } = "DD. MM. yyyy";
        public string Title { get; set; }
        public string Address { get; set; }
        public DateTime FirstSyncedAt { get; set; }
        public DateTime LastSyncedAt { get; set; }
        public DateTime SyncDate { get; set; }

        [Column(TypeName = "jsonb")]
        public List<string> Roles { get; set; }

        public DataTableFieldNamesDisplayMode dataTableFieldNamesDisplayMode { get; set; } = DataTableFieldNamesDisplayMode.RESERVATION_TIME;

        public string PhoneNumber { get; set; }

        [JsonConverter(typeof(JsonBConverter<UserMailSendingData>))]
        [Column(TypeName = "jsonb")]
        public string MailSendingData { get; set; }

        [InverseProperty("Carrier")]
        public ICollection<Permission> Permisisons { get; set; }
        [InverseProperty("Carrier")]
        public ICollection<Reservation> Reservations { get; set; }

        [InverseProperty("User")]
        public File Image { get; set; }

        [NotMapped]
        public string UserName
        {
            get => Email;
            set => Email = value;
        }

        public bool IsCarrier()
        {
            return Roles.Contains(UserRole.Carrier);
        }

        public bool IsWarehouseAdmin()
        {
            return Roles.Contains(UserRole.CompanyAdmin);
        }

        public bool IsWarehouse()
        {
            return IsWarehouseAdmin() || Roles.Contains(UserRole.Warehouse);
        }

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

    public class UserExcerptDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string title { get; set; }
        public string profilePictureUrl { get; set; }

        public static UserExcerptDto FromUser(User user)
        {
            if (user == null)
            {
                return null;
            }

            return new UserExcerptDto()
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                title = user.Title,
                profilePictureUrl = user.Image?.GetFileUrl(),
            };
        }
    }

    public class SyncUserDto
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string[] Roles { get; set; }
        public CompanyDto Company { get; set; }

        public string AuthServerId { get; set; }

        public DateTime LastUpdatedAt { get; set; }
    }
}
