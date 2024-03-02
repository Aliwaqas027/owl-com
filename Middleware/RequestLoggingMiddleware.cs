using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate next;
    private readonly String[] redactedEndpoints = { "/authentication" };

    public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        Console.WriteLine($"[{context.Request.Method}] {context.Request.Path}");

        var requestBody = await this.ExtractRequestBody(context);

        Console.WriteLine(requestBody);

        await next(context);
    }

    private async Task<String> ExtractRequestBody(HttpContext context)
    {
        String requestBody = "";

        foreach (var redactedEndpoint in redactedEndpoints)
        {
            if (context.Request.Path.Value.Equals(redactedEndpoint))
            {
                return "<REDACTED>";
            }
        }

        try
        {
            context.Request.EnableBuffering();

            if (context.Request.Body.CanRead)
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 512, leaveOpen: true);

                requestBody = await reader.ReadToEndAsync();

                context.Request.Body.Position = 0;
            }
            else
            {
                requestBody = "<NOT EXTRACTABLE>";
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error reading request body: {e.Message}");
            requestBody = "<EXCEPTION READING>";
        }

        return requestBody;
    }
}