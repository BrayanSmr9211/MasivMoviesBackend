# Changelog — main

## [No publicado]

### Agregado
- Se creó proyecto MasivMoviesBackend con arquitectura Clean Architecture (.NET 8)
- Se implementó capa Domain con entidades: Movie, Room, Showtime, Ticket y contratos de repositorios
- Se implementó capa Application con DTOs, validaciones y servicios: MovieService, RoomService, ShowtimeService, TicketService, ReportService
- Se implementó capa Infrastructure con repositorios Redis, RedisSeatLockService (bloqueo atómico con TTL 15 min) y RabbitMqPublisher
- Se implementó capa API con controllers REST: Movies, Rooms, Showtimes, Tickets, Reports
- Se configuró integración con Azure Key Vault para secretos (DefaultAzureCredential)
- Se configuró Docker Compose con servicios: API, Redis 7, RabbitMQ 3 (con management)
- Se agregaron 10 tests unitarios con xUnit + Moq para MovieService, ShowtimeService y TicketService
- Se agregó Swagger/OpenAPI y health checks
- Se agregó README con documentación de endpoints y arquitectura
