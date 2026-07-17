using System.Net;
using System.Text.Json;

namespace MasivMovies.API.Middleware;

/// <summary>
/// Middleware global que captura todas las excepciones no manejadas
/// y retorna respuestas HTTP consistentes sin exponer detalles internos.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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

        // Log interno con detalle completo — nunca se expone al cliente
        _logger.LogError(exception,
            "Error no manejado | StatusCode: {StatusCode} | Path: {Path} | Message: {Message}",
            (int)statusCode, context.Request.Path, exception.Message);

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
