using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public class HttpHelper
{
    public HttpHelper()
    {
    }

    public static JObject JsonGetRequestSync(String url)
    {
        var client = new HttpClient();

        var task = Task.Run(() => client.GetAsync(url));
        task.Wait();
        var response = task.Result;

        var task2 = Task.Run(() => response.Content.ReadAsStringAsync());
        task2.Wait();

        return JObject.Parse(task2.Result);
    }
    public class RequestResponse
    {
        public HttpResponseHeaders headers { get; set; }
        public String response { get; set; }
        public HttpResponseMessage responseRaw { get; set; }
    }

    public static async Task<RequestResponse> JsonGetRequest(String url, String? bearerToken = null)
    {
        return await HttpHelper.JsonRequest(url, HttpMethod.Get, null, bearerToken);
    }


    public static async Task<RequestResponse> JsonPostRequest(String url, JToken body, String? bearerToken = null)
    {
        return await HttpHelper.JsonRequest(url, HttpMethod.Post, body, bearerToken);
    }


    public static async Task<RequestResponse> JsonPutRequest(String url, JToken body, String? bearerToken = null)
    {
        return await HttpHelper.JsonRequest(url, HttpMethod.Put, body, bearerToken);
    }

    public static async Task<RequestResponse> JsonDeleteRequest(String url, JToken body, String? bearerToken = null)
    {
        return await HttpHelper.JsonRequest(url, HttpMethod.Delete, body, bearerToken);
    }

    private static async Task<RequestResponse> JsonRequest(String url, HttpMethod method, JToken? body, String? bearerToken = null)
    {
        var client = new HttpClient();
        if (bearerToken != null)
        {
            client.DefaultRequestHeaders.Add("Authorization", bearerToken);
        }

        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(url)
        };

        if (body != null)
        {
            request.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
        }

        var response = await client.SendAsync(request);

        var responseString = await response.Content.ReadAsStringAsync();
        HttpStatusCode[] successCodes = new HttpStatusCode[]{
            HttpStatusCode.Created,
            HttpStatusCode.OK,
            HttpStatusCode.NoContent
        };

        if (Array.IndexOf(successCodes, response.StatusCode) == -1)
        {
            throw new Exception(responseString);
        }

        return new RequestResponse()
        {
            headers = response.Headers,
            response = responseString,
            responseRaw = response
        };
    }
}
