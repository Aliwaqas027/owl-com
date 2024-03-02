using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;

namespace OwlApi.Models
{
    public enum ReservationFieldType
    {
        Number,
        String,
        Select,
        Checkbox,
        Country,
        Date
    }

    public enum ReservationFieldSpecialMeaningField
    {
        YAMAS_DRIVER_CODE,
        DRIVER_NAME,
        TRANSPORT_COMPANY,
        TRUCK_REGISTRATION_NUMBER,
        TRANSPORT_COMPANY_COUNTRY,
        EMAIL,
        NUMBER_OF_PALLETS,
        LOADING_COUNTRY,
        DRIVER_SURNAME,
        NUMBER_OF_HALF_PALLETS
    }

    public class ReservationFieldSpecialMeaning
    {
        public ReservationFieldSpecialMeaningField Field { get; set; }
        public ReservationFieldType Type { get; set; }
    }

    public class SelectValuesData
    {
        [JsonProperty("values")]
        public List<string> Values { get; set; }

        [JsonProperty("base64Images")]
        public List<string> base64Images { get; set; }
    }

    public class ReservationFieldNameData
    {
        public string Name { get; set; }
        public int LanguageId { get; set; }
    }

    public class ReservationField
    {
        public int Id { get; set; }

        public int SequenceNumber { get; set; }

        [NotMapped]
        public string Name { get; set; }

        [InverseProperty("reservationField")]
        public ICollection<ReservationFieldName> reservationFieldNames { get; set; }

        public bool Required { get; set; }

        public string Default { get; set; }

        public int? Min { get; set; }

        public int? Max { get; set; }

        public bool IsMultiLine { get; set; } = false;

        public bool ShowInMail { get; set; } = true;

        public string HelpText { get; set; } = "";

        public bool HideField { get; set; } = false;

        public bool HideForCarriers { get; set; } = false;

        public ReservationFieldSpecialMeaningField? SpecialMeaning { get; set; } = null;

        [NotMapped]
        public ReservationFieldNameData[] ReservationFieldNamesData { get; set; }

        public ReservationFieldType Type { get; set; }

        [Column(TypeName = "jsonb")]
        public SelectValuesData SelectValues { get; set; }

        [NotMapped]
        public string Value { get; set; }

        public int? CompanyId { get; set; }

        // user level
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        public int? WarehouseId { get; set; }

        // warehouse level
        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; }

        public int? ImportantFieldWarehouseId { get; set; }

        // important warehouse field for filtering out ramps
        [ForeignKey("ImportantFieldWarehouseId")]
        public Warehouse ImportantFieldWarehouse { get; set; }

        public int? DoorId { get; set; }

        // door level
        [ForeignKey("DoorId")]
        public Door Door { get; set; }

        public int? DerivedFromFieldId { get; set; }

        // door level
        [ForeignKey("DerivedFromFieldId")]
        public ReservationField DerivedFromField { get; set; }

        public void unattachAndDeriveFrom(bool shouldDeriveFrom = true)
        {
            if (shouldDeriveFrom)
            {
                DerivedFromFieldId = Id;
            }

            Id = 0;
            DoorId = null;
            CompanyId = null;
            WarehouseId = null;
            ImportantFieldWarehouseId = null;
            reservationFieldNames = null;
        }

        public static List<ReservationField> GetDefaultReservationFields(List<AppLanguage> allLanguages, Company Company, Warehouse Warehouse, Door Door)
        {
            int sequenceCounter = 0;
            return new List<ReservationField>() {
                GenerateReservationField(allLanguages, "Pallets", sequenceCounter++, true, ReservationFieldSpecialMeaningField.NUMBER_OF_PALLETS, true, false, false, ReservationFieldType.Number, Company, Warehouse, null,  Door),
                GenerateReservationField(allLanguages, "Delivered", sequenceCounter++, false, null, true, false, false, ReservationFieldType.Number, Company, Warehouse, null, Door),
                GenerateReservationField(allLanguages, "Ordered", sequenceCounter++, false, null, true, false, false, ReservationFieldType.Number, Company, Warehouse, null,Door),
                GenerateReservationField(allLanguages, "Description", sequenceCounter++, false, null, true, false, false, ReservationFieldType.String, Company, Warehouse, null,Door),
                GenerateReservationField(allLanguages, "Value", sequenceCounter++, false, null, true,  false, false, ReservationFieldType.String, Company, Warehouse, null, Door),
                GenerateReservationField(allLanguages, "Order number", sequenceCounter++, false, null, true, false,false,  ReservationFieldType.String, Company, Warehouse, null, Door),
                GenerateReservationField(allLanguages, "Driver present", sequenceCounter++, false, null, true, false,false,  ReservationFieldType.Checkbox, Company, Warehouse, null, Door),
                GenerateReservationField(allLanguages, "Driver name", sequenceCounter++, false, ReservationFieldSpecialMeaningField.DRIVER_NAME, true, false,false, ReservationFieldType.String, Company, Warehouse, null,Door),
                GenerateReservationField(allLanguages, "Driver surname", sequenceCounter++, false, ReservationFieldSpecialMeaningField.DRIVER_SURNAME, true, false,false, ReservationFieldType.String, Company, Warehouse, null,Door),
                GenerateReservationField(allLanguages, "Registration number", sequenceCounter++, false,  ReservationFieldSpecialMeaningField.TRUCK_REGISTRATION_NUMBER, true, false, false, ReservationFieldType.String, Company, Warehouse, null, Door),
                GenerateReservationField(allLanguages, "Comment", sequenceCounter++, false, null, true, false, false, ReservationFieldType.String, Company, Warehouse, null, Door)
            };
        }

        public static ReservationField FindFieldByMeaning(List<ReservationField> fields, ReservationFieldSpecialMeaningField specialMeaning)
        {
            return fields.Where(field => field.SpecialMeaning == specialMeaning).FirstOrDefault();
        }

        private static ReservationField GenerateReservationField(List<AppLanguage> languages, string name,
            int sequenceCounter, bool required, ReservationFieldSpecialMeaningField? specialMeaningField, bool showInMail, bool hideField, bool HideForCarriers, ReservationFieldType type,
            Company Company, Warehouse Warehouse, Warehouse ImportantFieldWarehouse, Door Door)
        {
            var reservationField = new ReservationField()
            {
                SequenceNumber = sequenceCounter,
                Required = required,
                SpecialMeaning = specialMeaningField,
                ShowInMail = showInMail,
                HideField = hideField,
                HideForCarriers = HideForCarriers,
                Min = 0,
                Type = type,
                Company = Company,
                Warehouse = Warehouse,
                ImportantFieldWarehouse = ImportantFieldWarehouse,
                Door = Door
            };

            var names = GenerateReservationFieldNames(languages, name, reservationField);
            reservationField.reservationFieldNames = names;
            return reservationField;
        }

        private static List<ReservationFieldName> GenerateReservationFieldNames(List<AppLanguage> languages, string name, ReservationField field)
        {
            var fieldNames = new List<ReservationFieldName>();

            for (int i = 0; i < languages.Count; i++)
            {
                var currentLanguage = languages[i];
                fieldNames.Add(new ReservationFieldName()
                {
                    language = currentLanguage,
                    name = name,
                    reservationField = field
                });
            }

            return fieldNames;
        }

        public static List<ReservationFieldSpecialMeaning> GetSpecialMeanings()
        {
            return new List<ReservationFieldSpecialMeaning>() {
                new ReservationFieldSpecialMeaning() {
                    Field = ReservationFieldSpecialMeaningField.YAMAS_DRIVER_CODE,
                    Type = ReservationFieldType.String
                },
                new ReservationFieldSpecialMeaning() {
                    Field  = ReservationFieldSpecialMeaningField.DRIVER_NAME,
                    Type = ReservationFieldType.String
                },
                new ReservationFieldSpecialMeaning() {
                    Field  = ReservationFieldSpecialMeaningField.DRIVER_SURNAME,
                    Type = ReservationFieldType.String
                },
                new ReservationFieldSpecialMeaning() {
                    Field  = ReservationFieldSpecialMeaningField.TRANSPORT_COMPANY,
                    Type = ReservationFieldType.String
                },
                new ReservationFieldSpecialMeaning() {
                    Field  = ReservationFieldSpecialMeaningField.TRUCK_REGISTRATION_NUMBER,
                    Type = ReservationFieldType.String
                },
                new ReservationFieldSpecialMeaning() {
                    Field  = ReservationFieldSpecialMeaningField.TRANSPORT_COMPANY_COUNTRY,
                    Type = ReservationFieldType.Country
                },
                new ReservationFieldSpecialMeaning() {
                    Field  = ReservationFieldSpecialMeaningField.LOADING_COUNTRY,
                    Type = ReservationFieldType.Country
                },
                new ReservationFieldSpecialMeaning() {
                    Field  = ReservationFieldSpecialMeaningField.EMAIL,
                    Type = ReservationFieldType.String
                },
                new ReservationFieldSpecialMeaning() {
                    Field  = ReservationFieldSpecialMeaningField.NUMBER_OF_PALLETS,
                    Type = ReservationFieldType.Number
                 },
                new ReservationFieldSpecialMeaning() {
                    Field  = ReservationFieldSpecialMeaningField.NUMBER_OF_HALF_PALLETS,
                    Type = ReservationFieldType.Number
                 },
            };
        }
    }
}

