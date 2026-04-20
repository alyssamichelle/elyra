# syntax=docker/dockerfile:1
# Two different Telerik secrets (do not mix them up):
#
# 1) NuGet API key — download packages from https://nuget.telerik.com/v3/index.json
#    Render secret file name: telerik_nuget_api_key
#    Contents: NuGet API key only. Username for the feed is always "api-key".
#    https://www.telerik.com/blazor-ui/documentation/installation/nuget
#
# 2) Product license key — activates Telerik.Licensing during dotnet publish
#    Render secret file name: telerik_license_key
#    Contents: entire contents of telerik-license.txt from your account (License Keys → Download).
#    https://www.telerik.com/blazor-ui/documentation/deployment/ci-cd-license-key
#
# Local BuildKit:
#   DOCKER_BUILDKIT=1 docker build \
#     --secret id=telerik_nuget_api_key,src=./telerik_nuget_api_key.txt \
#     --secret id=telerik_license_key,src=./telerik-license.txt \
#     .

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
# License must be present for publish so the built app is activated (no runtime watermark/banner).
RUN --mount=type=secret,id=telerik_license_key,env=TELERIK_LICENSE \
    dotnet publish "elyra.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
# Render sets PORT at runtime; shell form expands $PORT when the container starts
CMD dotnet elyra.dll --urls "http://0.0.0.0:${PORT:-8080}"
