FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/MasivMovies.API/MasivMovies.API.csproj", "src/MasivMovies.API/"]
COPY ["src/MasivMovies.Application/MasivMovies.Application.csproj", "src/MasivMovies.Application/"]
COPY ["src/MasivMovies.Domain/MasivMovies.Domain.csproj", "src/MasivMovies.Domain/"]
COPY ["src/MasivMovies.Infrastructure/MasivMovies.Infrastructure.csproj", "src/MasivMovies.Infrastructure/"]
RUN dotnet restore "src/MasivMovies.API/MasivMovies.API.csproj"
COPY . .
WORKDIR "/src/src/MasivMovies.API"
RUN dotnet build "MasivMovies.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MasivMovies.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MasivMovies.API.dll"]
