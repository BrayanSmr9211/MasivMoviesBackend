# MasivMovies Backend

API REST para la gestión de salas de cine, películas, funciones y boletos.

## Stack Tecnológico

- **.NET 8** (C#)
- **Redis** — Base de datos no relacional + manejo de concurrencia de asientos (locks con TTL 15 min)
- **RabbitMQ** — Cola de mensajes para notificaciones de compras
- **Docker Compose** — Orquestación de contenedores (API + Redis + RabbitMQ)
- **Azure Key Vault** — Gestión segura de secretos
- **xUnit + Moq** — Testing unitario

## Arquitectura

Clean Architecture con separación de capas:

```
src/
├── MasivMovies.Domain/          → Entidades y contratos (interfaces)
├── MasivMovies.Application/     → DTOs, servicios de aplicación, interfaces de infraestructura
├── MasivMovies.Infrastructure/  → Implementaciones (Redis repos, RabbitMQ, Seat locks)
└── MasivMovies.API/             → Controllers, configuración, Dockerfile
tests/
├── MasivMovies.Application.Tests/  → Tests unitarios de servicios
└── MasivMovies.API.Tests/          → Tests de integración
```

## Requisitos Funcionales

1. **Creación de Películas** — CRUD con validaciones
2. **Registro de Salas** — Gestión de salas por sede
3. **Registro de Funciones** — Programación y cancelación de horarios
4. **Compra y Verificación de Boletos** — Concurrencia con Redis (SET NX + TTL 15 min)
5. **Reportes Mensuales** — Funciones más exitosas por boletos vendidos

## Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/v1/movies` | Crear película |
| GET | `/api/v1/movies` | Listar películas |
| GET | `/api/v1/movies/{id}` | Obtener película por Id |
| POST | `/api/v1/rooms` | Registrar sala |
| GET | `/api/v1/rooms` | Listar salas |
| POST | `/api/v1/showtimes` | Registrar función |
| DELETE | `/api/v1/showtimes/{id}` | Cancelar función |
| GET | `/api/v1/showtimes` | Listar funciones |
| POST | `/api/v1/tickets/purchase` | Comprar boleto |
| POST | `/api/v1/tickets/{id}/confirm` | Confirmar compra |
| GET | `/api/v1/tickets/seats/{showtimeId}` | Estado de asientos |
| GET | `/api/v1/reports/monthly?year=2024&month=6` | Reporte mensual |

## Ejecución Local

### Con Docker Compose

```bash
docker-compose up --build
```

La API estará en `http://localhost:8080`. Swagger UI en `http://localhost:8080/swagger`.

### Sin Docker (requiere Redis y RabbitMQ locales)

```bash
dotnet run --project src/MasivMovies.API
```

## Tests

```bash
dotnet test
```

## Azure Key Vault

Para habilitar secretos desde Azure Key Vault, configura la variable `AzureKeyVault:VaultUrl` con la URL de tu Key Vault. La autenticación usa `DefaultAzureCredential` (Managed Identity en Azure, o Azure CLI en desarrollo local).

## Autores

- Brayan Muñoz
