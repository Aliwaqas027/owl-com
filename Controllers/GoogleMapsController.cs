using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OwlApi.Helpers;
using System;
using System.Threading.Tasks;

namespace OwlApi.Controllers
{
    [Authorize]
    public class GoogleMapsController : BaseController
    {
        public string ApiKey;

        public GoogleMapsController(OwlApiContext context, IConfiguration configuration, EmailClient emailClient, ReservationHelper reservationHelper) : base(context, configuration)
        {
            ApiKey = _configuration.GetSection("GoogleMapsAPIKey").Value;
        }

        public class AutocompleteRequest
        {
            public string Input { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> Autocomplete([FromQuery] AutocompleteRequest request)
        {
            var response = await HttpHelper.JsonGetRequest(
            $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input=razvanj&types=geocode&key=AIzaSyDD8Y8xoUdJqRFi8SdrWuoeSTb-DwmY5T0"
            );

            JObject res = JObject.Parse(response.response);
            JArray predictions = (JArray)res["predictions"];

            for (int i = 0; i < predictions.Count; i++)
            {

            }


            Console.WriteLine(response);
            return Ok();
        }

    }
}