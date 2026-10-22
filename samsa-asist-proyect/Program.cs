using Microsoft.EntityFrameworkCore;
using samsa_asist_proyect.Dt.AppBdContext;
using samsa_asist_proyect.Ng.Asistencia;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// 1. Agregar controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// 2. Configurar MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 3. Configurar CORS (para que Angular pueda hacer peticiones)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // El puerto de tu Angular
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Registrar el servicio de negocio
builder.Services.AddScoped<AttendanceService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
