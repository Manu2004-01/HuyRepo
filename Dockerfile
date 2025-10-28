# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and csproj files
COPY EatITAPI.sln .
COPY EatIT.Core/EatIT.Core.csproj EatIT.Core/
COPY EatIT.Infrastructure/EatIT.Infrastructure.csproj EatIT.Infrastructure/
COPY EatIT.WebAPI/EatIT.WebAPI.csproj EatIT.WebAPI/

# Restore dependencies
RUN dotnet restore

# Copy source
COPY EatIT.Core/ EatIT.Core/
COPY EatIT.Infrastructure/ EatIT.Infrastructure/
COPY EatIT.WebAPI/ EatIT.WebAPI/

# Build
WORKDIR /src/EatIT.WebAPI
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "EatIT.WebAPI.dll"]


