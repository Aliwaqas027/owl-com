using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OwlApi.Helpers
{
    public class OptimapiClient : IDisposable
    {
        private OptimapiServer _server;
        private HttpClient _client;
        public OptimapiClient(OptimapiServer server)
        {
            _server = server;
            _client = new HttpClient();
            _client.BaseAddress = new Uri(server.Url);
        }

        private string GetAuthenticatedUrl(string url)
        {
            Uri uri = new Uri(_client.BaseAddress + url);
            Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(uri.Query);
            List<KeyValuePair<string, string>> items = query
              .SelectMany(
                x => x.Value,
                (col, value) => new KeyValuePair<string, string>(col.Key, value)
              ).ToList();
            var qb = new QueryBuilder(items);
            qb.Add("username", _server.Username);
            qb.Add("password", _server.Password);

            string baseUri = uri.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);
            return baseUri + qb.ToQueryString();
        }

        public async Task Authenticate()
        {
            await _client.GetAsync(GetAuthenticatedUrl("auth"));
        }

        protected async Task<string> Upload(string url, List<string> filenames, List<Stream> streams)
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                string name = "file";
                if (streams.Count > 1) name = "files";
                for (int i = 0; i < streams.Count; i++)
                {
                    content.Add(new StreamContent(streams[i]), name, filenames[i]);
                }

                using (HttpResponseMessage message =
                  await _client.PostAsync(url, content)
                )
                {
                    string response = await message.Content.ReadAsStringAsync();
                    if (!message.IsSuccessStatusCode)
                        throw new Exception(response);
                    return response;
                }
            }
        }

        public async Task<string> Geocode(Stream stops, Stream vehicles, Stream stopsSettings, Stream vehiclesSettings)
        {
            return await Upload(
              GetAuthenticatedUrl("geocode"),
              new List<string> { $"stops.csv", $"vehicles.csv", $"stops_settings_{_server.Username}.txt", $"vehicles_settings_{_server.Username}.txt" },
              new List<Stream> { stops, vehicles, stopsSettings, vehiclesSettings }
            );
        }

        public async Task<string> StartComputation(int iterations)
        {
            HttpResponseMessage response = await _client.PostAsync(GetAuthenticatedUrl("startComputation?numberIterations=" + iterations.ToString()), null);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(await response.Content.ReadAsStringAsync());
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<MemoryStream> DownloadResults()
        {
            HttpResponseMessage response = await _client.GetAsync(GetAuthenticatedUrl("downloadResults"));
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(await response.Content.ReadAsStringAsync());
            MemoryStream stream = new MemoryStream();
            await response.Content.CopyToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
