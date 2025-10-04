# Usa la imagen oficial del SDK de .NET 8 para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copia primero el archivo .csproj y restaura las dependencias
# El "./" significa "desde el directorio actual"
COPY ["PortalAcademico.csproj", "."]
RUN dotnet restore "./PortalAcademico.csproj"

# Copia el resto de los archivos del proyecto
COPY . .
WORKDIR "/source/."
# Publica la aplicación en modo Release
RUN dotnet publish "PortalAcademico.csproj" -c Release -o /app/publish --no-restore

# Usa la imagen más ligera del runtime de ASP.NET para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Define el punto de entrada para iniciar la aplicación
ENTRYPOINT ["dotnet", "PortalAcademico.dll"]