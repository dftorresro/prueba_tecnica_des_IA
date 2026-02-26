using InvoiceApi.Data;
using InvoiceApi.Services;
using InvoiceApi.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Invoice API", Version = "v1" });

    // API Key: X-API-KEY
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key needed to access the endpoints. Header: X-API-KEY",
        In = ParameterLocation.Header,
        Name = "X-API-KEY",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DI
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<InvoiceRepository>();
builder.Services.AddScoped<InvoiceService>();

// Validación automática con ProblemDetails
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest
        };
        return new BadRequestObjectResult(problem);
    };
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = feature?.Error;

        var status = StatusCodes.Status500InternalServerError;
        var title = "Unexpected error";

        // Si el SP hace RAISERROR por duplicado, suele venir como SqlException
        if (ex is Microsoft.Data.SqlClient.SqlException sqlEx &&
            sqlEx.Message.Contains("InvoiceNumber already exists", StringComparison.OrdinalIgnoreCase))
        {
            status = StatusCodes.Status409Conflict;
            title = "Conflict";
        }
        else if (ex is ArgumentException)
        {
            status = StatusCodes.Status400BadRequest;
            title = "Bad request";
        }

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = ex?.Message
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// API Key middleware (si lo tienes)
app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();
app.Run();