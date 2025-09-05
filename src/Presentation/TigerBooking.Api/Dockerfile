# syntax=docker/dockerfile:1

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore
COPY TigerBooking.sln ./
COPY src/Core/TigerBooking.Domain/TigerBooking.Domain.csproj src/Core/TigerBooking.Domain/
COPY src/Core/TigerBooking.Application/TigerBooking.Application.csproj src/Core/TigerBooking.Application/
COPY src/Infrastructure/TigerBooking.Infrastructure/TigerBooking.Infrastructure.csproj src/Infrastructure/TigerBooking.Infrastructure/
COPY src/Presentation/TigerBooking.Api/TigerBooking.Api.csproj src/Presentation/TigerBooking.Api/

RUN dotnet restore src/Presentation/TigerBooking.Api/TigerBooking.Api.csproj

# Copy the rest of the source
COPY . .

# Publish
RUN dotnet publish src/Presentation/TigerBooking.Api/TigerBooking.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Set environment for containerized run
ENV ASPNETCORE_URLS=http://0.0.0.0:58081 \
    ASPNETCORE_ENVIRONMENT=loc-docker \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

EXPOSE 58081

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TigerBooking.Api.dll"]
