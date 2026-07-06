using Azure.Identity;
using MasivMovies.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Azure Key Vault — carga secretos en configuración
var keyVaultUrl = builder.Configuration["AzureKeyVault:VaultUrl"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// Infraestructura (Redis, RabbitMQ, repositorios, servicios)
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MasivMovies API",
        Version = "v1",
        Description = "API para gestión de salas de cine, películas y boletos"
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Needed for integration tests with WebApplicationFactory
public partial class Program { }
