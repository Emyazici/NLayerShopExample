using Microsoft.EntityFrameworkCore;
using FluentValidation;
using NLayerShop.Data.Context;
using NLayerShop.Data.UnitOfWork;
using NLayerShop.Business.Interfaces;
using NLayerShop.Business.Services;
using NLayerShop.Business.Mapping;
using NLayerShop.Business.Validation;
using NLayerShop.Api.ExceptionHandling;
using NLayerShop.Data.Seed;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog: structured console logging, configured from the "Serilog" section of appsettings.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// -------------------- Services (DI) --------------------
builder.Services.AddControllers();

// RFC 9457 ProblemDetails + global exception handler
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions.TryAdd("traceId",
            System.Diagnostics.Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Validatorları bu assembly'den tara
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();

// AutoMapper profilleri
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// PostgreSQL için: opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Business Servisleri
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Development only: apply migrations and insert demo data when the database is empty.
// Other environments (Production, Testing, ...) manage their schema and data themselves.
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.MigrateAndSeedAsync(db);
}

// -------------------- Middleware Pipeline --------------------
// One concise log line per request (method, path, status code, elapsed time).
// Registered before the exception handler so it logs the final status code (e.g. 404), not 500.
app.UseSerilogRequestLogging();

// Exception'ları ProblemDetails cevabına çevirir (GlobalExceptionHandler)
app.UseExceptionHandler();

// (İster development, ister prod) Swagger açık kalsın:
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
