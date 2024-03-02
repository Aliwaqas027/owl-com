using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    public class CompanyController : BaseController
    {

        public CompanyController(OwlApiContext context, IConfiguration configuration) : base(context, configuration)
        {
        }

        public class CompanyExistsRequest
        {
            public string Company { get; set; }
        }

        public class CompanyExistsResponse
        {
            public bool Exists { get; set; }
        }

        [AllowAnonymous]
        public async Task<CompanyExistsResponse> CompanyExists([FromBody] CompanyExistsRequest request)
        {
            if (request.Company == "master")
            {
                return new CompanyExistsResponse()
                {
                    Exists = false
                };
            }
            if (request.Company == _configuration["Authentication:CarrierRealm"])
            {
                return new CompanyExistsResponse()
                {
                    Exists = true
                };
            }

            var dashUrl = _configuration["URL:Dashboard"];
            var existsUrl = $"{dashUrl}/api/company/companyExists";

            JObject req = new JObject();
            req.Add("Company", request.Company);

            var existsJson = await HttpHelper.JsonPostRequest(existsUrl, req);

            return new CompanyExistsResponse()
            {
                Exists = (bool)JObject.Parse(existsJson.response)["exists"]
            };
        }

        [AllowAnonymous]
        public async Task<CompanyDto> Get(int id)
        {
            var company = await _context.Companies.Where(c => c.Id == id).FirstOrDefaultAsync();
            if (company == null)
            {
                throw new ApplicationException("Not found");
            }

            return CompanyDto.FromCompany(company);
        }

        public class DriverDto
        {
            public string code { get; set; }
            public Country transportCompanyCountry { get; set; }
            public string name { get; set; }
            public string surname { get; set; }
            public string email { get; set; }
            public string transportCompany { get; set; }
        }

        public class GetDriverByCodeRequest
        {
            public string code { get; set; }
            public string name { get; set; }
            public string surname { get; set; }
        }

        public async Task<DriverDto> GetDriverByCode([FromBody] GetDriverByCodeRequest request)
        {
            var actor = GetCurrentActor();
            var dashUrl = _configuration["URL:YAMAS"];
            var syncUrl = $"{dashUrl}/api/driver/getDriverByCode";
            var syncBody = new JObject();

            syncBody.Add("token", _configuration["Auth:YAMAS"]);
            syncBody.Add("reference", 0);
            syncBody.Add("code", request.code);
            syncBody.Add("name", request.name + " " + request.surname);
            syncBody.Add("companyRealmName", actor.Company.RealmName);

            var driverJson = await HttpHelper.JsonPostRequest(syncUrl, syncBody);
            var driver = JsonConvert.DeserializeObject<DriverDto>(driverJson.response.ToString());

            return driver;
        }

        public class GetReservationByCodeRequest
        {
            public string code { get; set; }
            public string token { get; set; }
            public string companyRealmName { get; set; }
            public RequestRef requestRef { get; set; }
        }

        public class GetReservationByCodeResponse
        {
            public int id { get; set; }
            public string code { get; set; }
            public int WarehouseId { get; set; }
            public string driverCode { get; set; }
            public string driverName { get; set; }
            public string driverCompany { get; set; }
            public string driverRegistration { get; set; }
            public Country driverCompanyCounty { get; set; }
            public Country loadingCountry { get; set; }
            public string driverEmail { get; set; }

            public bool isRecurring { get; set; }
        }

        [AllowAnonymous]
        public async Task<GetReservationByCodeResponse> GetReservationByCode([FromBody] GetReservationByCodeRequest request)
        {
            CheckToken(request.token, request.requestRef);
            List<ReservationField> data = null;

            var reservation = await _context.Reservations
                .Where(r => r.Code == request.code && r.Door.Warehouse.Company.RealmName == request.companyRealmName)
                .Include(r => r.Door.Warehouse)
                .FirstOrDefaultAsync();


            var response = new GetReservationByCodeResponse()
            {
            };

            if (reservation == null)
            {
                var recReservation = await _context.RecurringReservations
                    .Where(r => r.Code == request.code && r.Door.Warehouse.Company.RealmName == request.companyRealmName)
                    .Include(r => r.Door.Warehouse)
                    .FirstOrDefaultAsync();

                if (recReservation != null)
                {
                    data = recReservation.GetData();
                    response.id = recReservation.Id;
                    response.code = recReservation.Code;
                    response.WarehouseId = recReservation.Door.Warehouse.Id;
                    response.isRecurring = true;
                }
            }
            else
            {
                data = reservation.GetData();
                response.id = reservation.Id;
                response.code = reservation.Code;
                response.WarehouseId = reservation.Door.Warehouse.Id;
                response.isRecurring = false;
            }

            if (data == null)
            {
                throw new ApplicationException("not found");
            }

            var codeField = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.YAMAS_DRIVER_CODE);
            var nameField = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.DRIVER_NAME);
            var surnameField = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.DRIVER_SURNAME);
            var companyField = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.TRANSPORT_COMPANY);
            var registrationField = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.TRUCK_REGISTRATION_NUMBER);
            var companyCoutryField = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.TRANSPORT_COMPANY_COUNTRY);
            var loadingCountryField = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.LOADING_COUNTRY);
            var emailField = ReservationField.FindFieldByMeaning(data, ReservationFieldSpecialMeaningField.EMAIL);

            Country companyCounty = null;
            if (companyCoutryField != null)
            {
                int countryId;
                if (int.TryParse(companyCoutryField.Value, out countryId))
                {
                    companyCounty = await _context.Countries.Where(c => c.Id == countryId).FirstOrDefaultAsync();
                }
            }

            Country loadingCountry = null;
            if (loadingCountryField != null)
            {
                int countryId2;
                if (int.TryParse(loadingCountryField.Value, out countryId2))
                {
                    loadingCountry = await _context.Countries.Where(c => c.Id == countryId2).FirstOrDefaultAsync();
                }
            }

            response.driverCode = codeField?.Value;
            response.driverName = nameField?.Value + " " + surnameField?.Value;
            response.driverCompany = companyField?.Value;
            response.driverRegistration = registrationField?.Value;
            response.driverCompanyCounty = companyCounty;
            response.loadingCountry = loadingCountry;
            response.driverEmail = emailField?.Value;

            return response;
        }

        public async Task<List<AppLanguage>> GetAllLanguages()
        {
            var languages = _context.AppLanguages.ToList();
            return languages;
        }
    }
}
