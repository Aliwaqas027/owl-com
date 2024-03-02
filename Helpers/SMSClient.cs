using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OwlApi.Helpers
{
    public class SMSClient
    {
        private const string BASE_URL = "http://panel.smspm.com/gateway/";
        private const string SENDER = "SMSPM.com";

        private HttpClient _client;

        public SMSClient(IConfiguration config)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(BASE_URL + config.GetSection("SMS").GetValue<string>("Key") + "/");
            Console.WriteLine(_client.BaseAddress);
        }

        public Task<HttpResponseMessage> Send(string number, string text)
        {
            string url = "api.v1/send?"
              + $"phone={number}"
              + $"&sender={SENDER}"
              + $"&message={text}"
              + "&output=json";

            return _client.GetAsync(url);
        }
    }
}
