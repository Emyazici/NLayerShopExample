using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using NLayerShop.Data.Context;
using NLayerShop.Data.UnitOfWork;
using NLayerShop.Business.Interfaces;
using NLayerShop.Business.Services;
using NLayerShop.Business.Mapping;
using NLayerShop.Business.Validation;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services (DI) --------------------
builder.Services.AddControllers();

// Validatorlarý bu assembly'den tara
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
// (Ýster development, ister prod) Swagger açýk kalsýn:
app.UseSwagger();
app.UseSwaggerUI();

// (Ýleride Global Exception Middleware ekleyeceðiz)
// app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
