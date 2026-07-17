using System.Net;
using System.Text.Json;
using Microsoft.ApplicationInsights;

namespace MasivMovies.API.Middleware;

/// <summary>
/// Middleware global que captura todas las excepciones no manejadas,
/// las registra en Application Insights y retorna respuestas HTTP consistentes.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly TelemetryClient _telemetryClient;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        TelemetryClient telemetryClient)
    {
        _next = next;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            InvalidOperationException ex => (HttpStatusCode.Conflict, ex.Message),
            ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message),
            KeyNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No tiene permisos para realizar esta acción."),
            _ => (HttpStatusCode.InternalServerError, "Ha ocurrido un error interno. Intente más tarde.")
        };

        // Log interno con detalle completo
        _logger.LogError(exception,
            "Error no manejado | StatusCode: {StatusCode} | Path: {Path} | Message: {Message}",
            (int)statusCode, context.Request.Path, exception.Message);

        // Registrar en Application Insights con propiedades personalizadas
        var properties = new Dictionary<string, string>
        {
            { "StatusCode", ((int)statusCode).ToString() },
            { "Path", context.Request.Path.Value ?? "" },
            { "Method", context.Request.Method },
            { "ExceptionType", exception.GetType().Name }
        };

        _telemetryClient.TrackException(exception, properties);

        // Si es error 500, registrar como evento crítico
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _telemetryClient.TrackEvent("CriticalError", properties);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            detail = message,
            path = context.Request.Path.Value
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
