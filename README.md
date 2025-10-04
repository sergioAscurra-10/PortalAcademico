# Portal Académico — Gestión de Cursos y Matrículas

Proyecto de examen parcial para la gestión de cursos y matrículas, construido con ASP.NET Core 8/9.

**URL de la aplicación desplegada:** [PEGA AQUÍ LA URL DE RENDER CUANDO LA TENGAS]

---

## Configuración Local

1.  Clonar el repositorio.
2.  Asegurarse de tener el SDK de .NET 8 (o 9) instalado.
3.  Para usar Redis localmente, configurar la cadena de conexión en `appsettings.Development.json`.
4.  Ejecutar la aplicación con `dotnet run`. La base de datos (`app.db`) se creará y se llenará con datos iniciales en el primer arranque.

**Credenciales de prueba:**
-   **Rol:** Coordinador
-   **Usuario:** `coordinador@test.com`
-   **Contraseña:** `Password123!`

---

## Despliegue en Render.com

La aplicación está configurada para desplegarse como un "Web Service" en Render.

### Configuración del Servicio

-   **Environment:** `.NET`
-   **Build Command:** `dotnet publish -c Release -o out`
-   **Start Command:** `dotnet PortalAcademico/PortalAcademico.dll`

### Variables de Entorno Requeridas

-   `ASPNETCORE_ENVIRONMENT`: `Production`
-   `ASPNETCORE_URLS`: `http://0.0.0.0:${PORT}`
-   `ConnectionStrings__DefaultConnection`: `DataSource=/data/app.db;Cache=Shared` (Requiere un Disco Persistente montado en `/data`)
-   `Redis__ConnectionString`: redis-17395.c84.us-east-1-2.ec2.redns.redis-cloud.com:17395,password=3HXHigIFCaQPJcPWU4DiY7BsjFcQklJ6,ssl=True,abortConnect=False