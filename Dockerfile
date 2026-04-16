# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["elyra.csproj", "./"]
RUN dotnet restore "elyra.csproj"
COPY . .
RUN dotnet publish "elyra.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
# Render sets PORT at runtime; shell form expands $PORT when the container starts
CMD dotnet elyra.dll --urls "http://0.0.0.0:${PORT:-8080}"
