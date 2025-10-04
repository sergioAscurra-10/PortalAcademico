# Usa la imagen oficial del SDK de .NET 8 para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copia TODO el código fuente de una vez
COPY . .

# Restaura las dependencias y publica la aplicación en un solo paso
RUN dotnet publish "PortalAcademico.csproj" -c Release -o /app/publish

# Usa la imagen más ligera del runtime de ASP.NET para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Define el punto de entrada para iniciar la aplicación
ENTRYPOINT ["dotnet", "PortalAcademico.dll"]