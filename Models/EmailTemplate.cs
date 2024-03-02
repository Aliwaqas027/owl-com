using OwlApi.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public enum EmailTemplateType
    {
        RESERVATION_CREATED,
        RECCURING_RESERVATION_CREATED,
        TWO_PHASE_RESERVATION_CREATED,
        RESERVATION_UPDATED,
        RECCURING_RESERVATION_UPDATED,
        TWO_PHASE_RESERVATION_UPDATED,
        RESERVATION_DELETED,
        RECCURING_RESERVATION_DELETED,
        TWO_PHASE_RESERVATION_DELETED,
        TWO_PHASE_RESERVATION_CONFIRMED,

    }

    public enum EmailTemplatePlaceholder
    {
        RECIPIENT_NAME,

    }

    public class EmailTemplate
    {
        public int Id { get; set; }

        public EmailTemplateType Type { get; set; }
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        public int LanguageId { get; set; }

        [ForeignKey("LanguageId")]
        public AppLanguage Language { get; set; }

        public string SubjectTemplate { get; set; }
        public string ContentTemplate { get; set; }

        public static EmailTemplateType OperationToType(EmailClient.ReservationOperation operation, EmailClient.ReservationType type)
        {
            if (type == EmailClient.ReservationType.STANDARD)
            {
                if (operation == EmailClient.ReservationOperation.CREATE)
                {
                    return EmailTemplateType.RESERVATION_CREATED;
                }
                else if (operation == EmailClient.ReservationOperation.UPDATE)
                {
                    return EmailTemplateType.RESERVATION_UPDATED;
                }
                else if (operation == EmailClient.ReservationOperation.DELETE)
                {
                    return EmailTemplateType.RESERVATION_DELETED;
                }
            }
            else if (type == EmailClient.ReservationType.RECURRING)
            {
                if (operation == EmailClient.ReservationOperation.CREATE)
                {
                    return EmailTemplateType.RECCURING_RESERVATION_CREATED;
                }
                else if (operation == EmailClient.ReservationOperation.UPDATE)
                {
                    return EmailTemplateType.RECCURING_RESERVATION_UPDATED;
                }
                else if (operation == EmailClient.ReservationOperation.DELETE)
                {
                    return EmailTemplateType.RECCURING_RESERVATION_DELETED;
                }
            }
            else if (type == EmailClient.ReservationType.TWO_PHASE)
            {
                if (operation == EmailClient.ReservationOperation.CREATE)
                {
                    return EmailTemplateType.TWO_PHASE_RESERVATION_CREATED;
                }
                else if (operation == EmailClient.ReservationOperation.UPDATE)
                {
                    return EmailTemplateType.RECCURING_RESERVATION_UPDATED;
                }
                else if (operation == EmailClient.ReservationOperation.DELETE)
                {
                    return EmailTemplateType.TWO_PHASE_RESERVATION_DELETED;
                }
            }
            else if (type == EmailClient.ReservationType.CONFIRM_TWO_PHASE)
            {
                if (operation == EmailClient.ReservationOperation.CREATE)
                {
                    return EmailTemplateType.TWO_PHASE_RESERVATION_CONFIRMED;
                }
            }

            return EmailTemplateType.RESERVATION_CREATED;
        }
    }
}
