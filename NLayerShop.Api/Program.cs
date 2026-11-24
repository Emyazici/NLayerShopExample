using Microsoft.EntityFrameworkCore;
using FluentValidation;
using NLayerShop.Data.Context;
using NLayerShop.Data.UnitOfWork;
using NLayerShop.Business.Interfaces;
using NLayerShop.Business.Services;
using NLayerShop.Business.Mapping;
using NLayerShop.Business.Validation;
using NLayerShop.Api.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services (DI) --------------------
builder.Services.AddControllers();

// RFC 9457 ProblemDetails + global exception handler
builder.Services.AddProblemDetails();
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

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -------------------- Middleware Pipeline --------------------
// Exception'ları ProblemDetails cevabına çevirir (GlobalExceptionHandler)
app.UseExceptionHandler();

// (İster development, ister prod) Swagger açık kalsın:
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
