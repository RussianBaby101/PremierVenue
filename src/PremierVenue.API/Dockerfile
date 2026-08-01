FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PremierVenue.API/PremierVenue.API.csproj", "PremierVenue.API/"]
COPY ["PremierVenue.Core/PremierVenue.Core.csproj", "PremierVenue.Core/"]
COPY ["PremierVenue.Domain/PremierVenue.Domain.csproj", "PremierVenue.Domain/"]
COPY ["PremierVenue.Infrastructure/PremierVenue.Infrastructure.csproj", "PremierVenue.Infrastructure/"]
RUN dotnet restore "PremierVenue.API/PremierVenue.API.csproj"
COPY PremierVenue.API/. PremierVenue.API/
COPY PremierVenue.Core/. PremierVenue.Core/
COPY PremierVenue.Domain/. PremierVenue.Domain/
COPY PremierVenue.Infrastructure/. PremierVenue.Infrastructure/
RUN dotnet build "PremierVenue.API/PremierVenue.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PremierVenue.API/PremierVenue.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PremierVenue.API.dll"]