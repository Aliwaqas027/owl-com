using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    [Authorize]
    public class SettingsController : BaseController
    {
        public SettingsController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        public List<ReservationField> GetCompanyReservationFields([FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();
            return GetReservationFields(p => p.CompanyId == actor.Company.Id, lang);
        }

        public List<ReservationField> GetWarehouseReservationFields(int id, [FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();
            WarehouseOrCarrier();

            var warehouse = _context.Warehouses.Where(w => w.Id == id);
            if (warehouse == null) throw new AuthenticationException();

            return GetReservationFields(p => p.WarehouseId == id, lang);
        }

        [AllowAnonymous]
        public async Task<List<ReservationField>> GetImportantWarehouseReservationFields(int id, [FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();
            var warehouse = await _context.Warehouses.Where(w => w.Id == id && (actor != null || w.canCarrierCreateAnonymousReservation)).FirstOrDefaultAsync();
            if (warehouse == null) throw new AuthenticationException();
            return GetReservationFields(p => p.ImportantFieldWarehouseId == id, lang);
        }

        [AllowAnonymous]
        public List<ReservationField> GetDoorReservationFields(int id, [FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();

            var door = _context.Doors.Where(d => d.Id == id && (actor != null || d.Warehouse.canCarrierCreateAnonymousReservation));
            if (door == null) throw new AuthenticationException();

            return GetReservationFields(p => p.DoorId == id, lang);
        }

        public ActionResult SetCompanyDefaultReservationFields()
        {
            User actor = GetCurrentActor();
            var languages = _context.AppLanguages.ToList();
            var fieldsForUser = ReservationField.GetDefaultReservationFields(languages, actor.Company, null, null);
            _context.ReservationFields.AddRange(fieldsForUser);
            _context.SaveChanges();

            return Ok();
        }


        public ActionResult SetWarehouseDefaultReservationFields(int id)
        {
            User actor = GetCurrentActor();
            WarehouseOrCarrier();

            var warehouse = _context.Warehouses.Where(w => w.Id == id).Include(w => w.ReservationFields).FirstOrDefault();
            if (warehouse == null) throw new AuthenticationException();

            var accountReservationFields = _context.ReservationFields.AsNoTracking().Where(f => f.CompanyId == actor.Company.Id).ToList();

            var newReservationFields = new List<ReservationField>();
            foreach (var field in accountReservationFields)
            {
                field.unattachAndDeriveFrom();
                field.Warehouse = warehouse;
                _context.Add(field);
            }

            _context.SaveChanges();

            return Ok();
        }

        public ActionResult SetDoorDefaultReservationFields(int id)
        {
            User actor = GetCurrentActor();
            WarehouseOrCarrier();

            var door = _context.Doors.Where(d => d.Id == id).Include(d => d.Warehouse).FirstOrDefault();
            if (door == null) throw new AuthenticationException();

            var warehouseReservationFields = _context.ReservationFields.AsNoTracking().Where(f => f.WarehouseId == door.Warehouse.Id).ToList();

            var newReservationFields = new List<ReservationField>();
            foreach (var field in warehouseReservationFields)
            {
                field.unattachAndDeriveFrom();
                field.Door = door;
                _context.Add(field);
            }

            _context.SaveChanges();

            return Ok();
        }

        private List<ReservationField> GetReservationFields(System.Linq.Expressions.Expression<Func<ReservationField, bool>> predicate, string lang)
        {
            var fieldsList = _context.ReservationFields.Where(predicate).Include(f => f.reservationFieldNames).ThenInclude(n => n.language).OrderBy(f => f.SequenceNumber).ToList();
            foreach (var field in fieldsList)
            {
                var correspondingLanguage = field.reservationFieldNames.Where(name => name.language.subdomain.Equals(lang)).FirstOrDefault();
                field.Name = getNameOfFieldByLocale(field, lang);
            }

            return fieldsList;
        }

        public static string getNameOfFieldByLocale(ReservationField field, string lang)
        {
            if (field.reservationFieldNames == null)
            {
                return field.Name ?? "";
            }
            var correspondingLanguage = field.reservationFieldNames.Where(name => name.language.subdomain.Equals(lang)).FirstOrDefault();
            if (correspondingLanguage != null)
            {
                return correspondingLanguage.name;
            }

            var defaultLanguage = field.reservationFieldNames.Where(name => name.language.subdomain.Equals("")).FirstOrDefault();
            if (defaultLanguage != null)
            {
                return defaultLanguage.name;
            }

            var firstLanguage = field.reservationFieldNames.FirstOrDefault();
            if (firstLanguage != null)
            {
                return firstLanguage.name;
            }

            return field.Name ?? "";
        }

        public async Task<ReservationField> AddReservationField([FromBody] ReservationField data, [FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();

            if (!validateReservationField(data, actor))
            {
                throw new AuthenticationException();
            }

            var duplicatedLanguageId = makeSureReserationFieldNameIsUnique(data, null);
            if (duplicatedLanguageId != null)
            {
                throw new ApplicationException("Duplicate name for language " + duplicatedLanguageId);
            }

            var lastField = _context.ReservationFields.Where(f => f.CompanyId == data.CompanyId && f.DoorId == data.DoorId && f.WarehouseId == data.WarehouseId).OrderByDescending(f => f.SequenceNumber).FirstOrDefault();
            if (lastField == null)
            {
                data.SequenceNumber = 1;
            }
            else
            {
                data.SequenceNumber = lastField.SequenceNumber + 1;
            }

            var field = _context.ReservationFields.Add(data);

            var fieldNames = data.ReservationFieldNamesData.Select(data => new ReservationFieldName()
            {
                languageId = data.LanguageId,
                name = data.Name,
                reservationField = field.Entity
            }).ToList();

            _context.ReservationFieldNames.AddRange(fieldNames);

            await _context.SaveChangesAsync();

            var fieldAdded = _context.ReservationFields.Where(f => f.Id == field.Entity.Id).Include(f => f.reservationFieldNames).ThenInclude(n => n.language).First();

            fieldAdded.Name = getNameOfFieldByLocale(fieldAdded, lang);

            return field.Entity;
        }

        public async Task<ReservationField> EditReservationField(int id, [FromBody] ReservationField data, [FromQuery(Name = "lang")] string lang)
        {
            User actor = GetCurrentActor();

            if (!validateReservationField(data, actor))
            {
                throw new AuthenticationException();
            }

            var existingField = await _context.ReservationFields.Where(f => f.Id == id).FirstOrDefaultAsync();
            if (existingField == null)
            {
                throw new ApplicationException("Not found");
            }

            var duplicatedLanguageId = makeSureReserationFieldNameIsUnique(data, id);
            if (duplicatedLanguageId != null)
            {
                throw new ApplicationException("Duplicate name for language " + duplicatedLanguageId);
            }

            _context.Entry(existingField).State = EntityState.Detached;

            data.Id = id;
            data.SequenceNumber = existingField.SequenceNumber;

            var field = _context.ReservationFields.Update(data);

            var oldFieldNames = _context.ReservationFieldNames.Where(f => f.fieldId == id).ToList();
            _context.ReservationFieldNames.RemoveRange(oldFieldNames);

            var newFieldNames = data.ReservationFieldNamesData.Select(data => new ReservationFieldName()
            {
                languageId = data.LanguageId,
                name = data.Name,
                reservationField = field.Entity
            }).ToList();
            _context.ReservationFieldNames.AddRange(newFieldNames);

            await _context.SaveChangesAsync();

            var fieldEdited = _context.ReservationFields.Where(f => f.Id == field.Entity.Id).Include(f => f.reservationFieldNames).ThenInclude(n => n.language).First();
            fieldEdited.Name = getNameOfFieldByLocale(fieldEdited, lang);

            return field.Entity;
        }

        public class ReservationFieldSequences
        {
            public int Id { get; set; }
            public int SequenceNumber { get; set; }
        }

        public class EditReservationFieldSequencesRequest
        {
            public List<ReservationFieldSequences> Fields { get; set; }
        }

        public async Task<ActionResult> EditReservationFieldSequences([FromBody] EditReservationFieldSequencesRequest data)
        {
            User actor = GetCurrentActor();

            var fieldIds = data.Fields.Select(f => f.Id).ToList();
            var existingFields = await _context.ReservationFields.Where(f => fieldIds.Contains(f.Id)).ToListAsync();

            foreach (var fieldInfo in existingFields)
            {
                if (!validateReservationField(fieldInfo, actor))
                {
                    throw new AuthenticationException();
                }

                fieldInfo.SequenceNumber = data.Fields.Find(f => f.Id == fieldInfo.Id).SequenceNumber;
                _context.ReservationFields.Update(fieldInfo);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveReservationField(int id)
        {
            User actor = GetCurrentActor();

            var field = await _context.ReservationFields.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (field == null)
            {
                throw new ApplicationException("Not found");
            }

            if (!validateReservationField(field, actor))
            {
                throw new AuthenticationException();
            }

            var derivedFields = _context.ReservationFields.Where(f => f.DerivedFromFieldId == field.Id).ToList();
            foreach (var derivedField in derivedFields)
            {
                derivedField.DerivedFromFieldId = null;
                _context.ReservationFields.Update(derivedField);
            }

            var oldFieldNames = _context.ReservationFieldNames.Where(f => f.fieldId == id).ToList();
            _context.ReservationFieldNames.RemoveRange(oldFieldNames);
            _context.ReservationFields.Remove(field);

            await _context.SaveChangesAsync();

            return Ok("");
        }

        private bool validateReservationField(ReservationField field, User actor)
        {
            if (field.SpecialMeaning != null)
            {
                var fieldSpecialMeanings = ReservationField.GetSpecialMeanings();
                var matchingSpecialMeaning = fieldSpecialMeanings.Find(meaning => meaning.Field == field.SpecialMeaning);
                if (matchingSpecialMeaning == null)
                {
                    field.SpecialMeaning = null;
                }
                else
                {
                    field.Type = matchingSpecialMeaning.Type;
                }
            }

            if (field.WarehouseId != null || field.ImportantFieldWarehouseId != null)
            {
                var warehouseId = field.WarehouseId;
                if (warehouseId == null)
                {
                    warehouseId = field.ImportantFieldWarehouseId;
                }

                var warehouse = _context.Warehouses.Where(w => w.Id == warehouseId && w.CompanyId == actor.Company.Id);
                if (warehouse == null)
                {
                    return false;
                }

                field.CompanyId = null;
                field.DoorId = null;
                if (field.WarehouseId != null)
                {
                    field.ImportantFieldWarehouseId = null;
                }
                else
                {
                    field.WarehouseId = null;
                }

                return true;
            }

            if (field.DoorId != null)
            {
                var door = _context.Doors.Where(d => d.Id == field.DoorId && d.Warehouse.CompanyId == actor.Company.Id);
                if (door == null)
                {
                    return false;
                }

                field.WarehouseId = null;
                field.CompanyId = null;
                field.ImportantFieldWarehouseId = null;
                return true;
            }

            field.CompanyId = actor.Company.Id;
            field.WarehouseId = null;
            field.DoorId = null;
            field.ImportantFieldWarehouseId = null;
            return true;
        }

        private int? makeSureReserationFieldNameIsUnique(ReservationField field, int? id)
        {
            List<ReservationField> reservationFields = new List<ReservationField>();
            if (field.WarehouseId != null)
            {
                reservationFields = _context.ReservationFields.Where(f => f.WarehouseId == field.WarehouseId && f.Id != id)
                                                                .Include(f => f.reservationFieldNames)
                                                                .ToList();
            }
            else if (field.DoorId != null)
            {
                reservationFields = _context.ReservationFields.Where(f => f.DoorId == field.DoorId && f.Id != id)
                                                                .Include(f => f.reservationFieldNames)
                                                                .ToList();
            }
            else if (field.CompanyId != null)
            {
                reservationFields = _context.ReservationFields.Where(f => f.CompanyId == field.CompanyId && f.Id != id)
                                                                .Include(f => f.reservationFieldNames)
                                                                .ToList();
            }

            foreach (var reservationField in reservationFields)
            {
                foreach (var fieldName in field.ReservationFieldNamesData)
                {
                    var reservationFieldLanguageName = reservationField.reservationFieldNames.Where(name => name.languageId == fieldName.LanguageId && name.name == fieldName.Name).FirstOrDefault();
                    if (reservationFieldLanguageName == null)
                    {
                        continue;
                    }

                    return reservationFieldLanguageName.languageId;
                }
            }

            return null;
        }

        public List<AppLanguage> GetLanguages()
        {
            WarehouseOrCarrier();
            return _context.AppLanguages.ToList();
        }

        public List<ReservationFieldSpecialMeaning> GetReservationFieldsSpecialMeanings()
        {
            WarehouseOrCarrier();
            return ReservationField.GetSpecialMeanings();
        }

        public class SetEmailGeneralSettingsRequest
        {
            public int DefaultMailLanguageId { get; set; }
            public bool SendContractInMail { get; set; }
        }
        public async Task<IActionResult> SetEmailGeneralSettings([FromBody] SetEmailGeneralSettingsRequest request)
        {
            WarehouseAdminOnly();
            var actor = GetCurrentActor();

            var language = await _context.AppLanguages.Where(l => l.Id == request.DefaultMailLanguageId).FirstOrDefaultAsync();
            if (language == null)
            {
                throw new ApplicationException("Not found");
            }

            actor.Company.DefaultMailLanguage = language;
            actor.Company.SendContractInMail = request.SendContractInMail;

            _context.Update(actor.Company);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> DismissFirstTimeProfileSetupNotice()
        {
            WarehouseAdminOnly();
            var actor = GetCurrentActor();

            actor.Company.ShowFirstTimeProfileSetupNotice = false;
            _context.Update(actor.Company);
            await _context.SaveChangesAsync();
            return Ok();
        }

        public class SetOWLCompanySettingsRequest
        {
            public bool DisableTwoPhaseReservations { get; set; }
        }
        public async Task<IActionResult> SetOWLCompanySettings([FromBody] SetOWLCompanySettingsRequest request)
        {
            WarehouseAdminOnly();
            var actor = GetCurrentActor();

            actor.Company.DisableTwoPhaseReservations = request.DisableTwoPhaseReservations;

            _context.Update(actor.Company);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}