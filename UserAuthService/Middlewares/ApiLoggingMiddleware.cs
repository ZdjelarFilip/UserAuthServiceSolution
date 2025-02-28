using System.Net;
using System.Text;

public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiLoggingMiddleware> _logger;

    public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;
        var hostName = Dns.GetHostName();
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var clientName = request.Headers["User-Agent"].ToString();
        var queryParams = request.QueryString.HasValue ? request.QueryString.Value : "N/A";
        var requestBody = "";

        // Log request body only for POST or PUT
        if (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put)
        {
            request.EnableBuffering();
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }
        }

        _logger.LogInformation("INFO | {Time} | Client IP: {ClientIp} | Client: {ClientName} |" +
            " Host: {HostName} | API Method: {Method} {Path} | Request Params: {QueryParams} |" +
            " Message: Request received",
            DateTime.UtcNow, clientIp, clientName, hostName, request.Method, request.Path, queryParams);

        try
        {
            await _next(context);  // Call next middleware
        }
        catch (Exception ex)
        {
            _logger.LogError("ERROR | {Time} | Client IP: {ClientIp} | Client: {ClientName} |" +
                " Host: {HostName} | API Method: {Method} {Path} | Request Params: {QueryParams} |" +
                " Message: {ErrorMessage}",
                DateTime.UtcNow, clientIp, clientName, hostName, request.Method, request.Path, queryParams, ex.Message);
            throw;
        }
    }
}