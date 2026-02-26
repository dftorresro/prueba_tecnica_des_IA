README

Hola!

Esta entrega corresponde a la prueba tecnica descrita en el documento de la prueba. 
Incluye:

1. Parte 1 Microservicio API REST en ASP.NET Core para registrar y consultar facturas en SQL Server usando Stored Procedures.
2. Parte 2 Optimizacion de prompt en un archivo separado.
3. Parte 3 Estrategia de pruebas volumetricas en un archivo separado.

Estructura del proyecto
Carpeta raiz

* InvoiceApi
  Proyecto ASP.NET Core (API)
* Database.sql
  Script para crear la base de datos, la tabla y los stored procedures
* PROMPTS - Parte 2.md
  Documento con la optimizacion del prompt y comparativo
* Pruebas volumetricas - Parte 3.md
  Documento con la estrategia propuesta

Parte 1 API de facturas (InvoiceApi)

Requisitos

* .NET SDK 
* SQL Server (local o en contenedor)
* Una base de datos llamada InvoiceDb (el script la crea)

Base de datos
Ejecutar Database.sql en SQL Server. El script:

* Crea la base InvoiceDb
* Crea la tabla dbo.Invoices con indice unico por InvoiceNumber
* Crea los stored procedures:

  * dbo.sp_Invoice_Create
  * dbo.sp_Invoice_GetById
  * dbo.sp_Invoice_SearchByClient

Configuracion
En InvoiceApi/appsettings.json:

* ConnectionStrings:SqlServer contiene el string de conexion
* Security:ApiKey define una API key para proteger endpoints via header X-API-KEY

Sobre seguridad:
La API valida una API key en el header X-API-KEY cuando existe una clave configurada en appsettings.

Ejecucion local

1. Verificar que SQL Server este activo y que Database.sql ya fue ejecutado
2. Ajustar el Connection String si tu SQL Server no esta en localhost:1433 o si cambian credenciales
3. Levantar la API desde la carpeta InvoiceApi con dotnet run (o ejecutar desde Visual Studio)

Swagger
En ambiente Development, la API expone Swagger UI al iniciar el servicio (segun launchSettings y el entorno).
Desde Swagger se pueden probar endpoints agregando el header X-API-KEY con el valor configurado.

Endpoints implementados

1. POST /invoice
   Crea una factura. Valida campos requeridos y rangos basicos.
   Retorna 201 Created y el objeto creado.

2. GET /invoice/{id}
   Busca por id (Guid).
   Si no existe retorna 404 con un ProblemDetails indicando Invoice not found.

3. GET /invoice/search?client={clientName}
   Busca por nombre de cliente (contiene). Requiere al menos 2 caracteres.
   Retorna lista (hasta 200 registros segun el stored procedure), ordenados por CreatedAt desc.

Manejo de errores y validaciones completado.


Parte 2 Prompt (PROMPTS - Parte 2.md)
Documento con:

* Mejoras aplicadas al prompt (control de longitud, estructura fija, reglas anti irrelevancia)
* Explicacion de como se evita contenido fuera de foco
* Comparativo entre el prompt original y el optimizado, mas un ejemplo de salida esperada

Parte 3 Pruebas volumetricas (Pruebas volumetricas - Parte 3.md)
Documento con:

* Definicion de prueba volumetrica y diferencias con carga y estres
* Escenario propuesto para esta API
* Volumenes S, M, L y mezcla de operaciones
* Metricas y KPIs (p50 p95 p99, throughput, error rate, recursos, metricas SQL)
* Estrategia de ejecucion por fases y posibles cuellos de botella con soluciones

Comentario de pruebas de ejecucion:
Todo fue testado en un computador Windows sin fallos.
Al intentar reproducir en Mac tuve algunos problemas de entorno (principalmente dependencias y configuracion del motor SQL / toolchain), por lo que puede requerir ajustes adicionales segun la instalacion local.


Quedo atento a cualquier duda sobre ejecucion, configuracion o decisiones de implementacion.

Daniel Torres
