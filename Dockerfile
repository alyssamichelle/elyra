# syntax=docker/dockerfile:1
# Telerik packages require https://nuget.telerik.com/v3/index.json (not on nuget.org).
# On Render: Environment → Secret Files → Filename must be telerik_nuget_api_key
# (contents = your Telerik NuGet API key; username for the feed is always "api-key").
# See https://www.telerik.com/blazor-ui/documentation/installation/nuget
#
# Local BuildKit build:
#   DOCKER_BUILDKIT=1 docker build --secret id=telerik_nuget_api_key,src=./telerik_key.txt .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["elyra.csproj", "./"]
RUN --mount=type=secret,id=telerik_nuget_api_key \
    dotnet nuget add source "https://nuget.telerik.com/v3/index.json" \
      --name "TelerikOnlineFeed" \
      --username "api-key" \
      --password "$(tr -d '\n\r' </run/secrets/telerik_nuget_api_key)" \
      --store-password-in-clear-text \
    && dotnet restore "elyra.csproj"
COPY . .
RUN dotnet publish "elyra.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
# Render sets PORT at runtime; shell form expands $PORT when the container starts
CMD dotnet elyra.dll --urls "http://0.0.0.0:${PORT:-8080}"
