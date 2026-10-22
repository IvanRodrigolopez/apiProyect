using Microsoft.EntityFrameworkCore;
using samsa_asist_proyect.Dt.AppBdContext;
using samsa_asist_proyect.Ng.Asistencia;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // 👈 AQUÍ ESTABA EL ERROR (quitar las //)

// 2. Configurar MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(connectionString));

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
builder.Services.AddScoped<SucursalService>();

var app = builder.Build();

// 4. Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // 👈 Habilita el generador de Swagger
    app.UseSwaggerUI();    // 👈 Habilita la interfaz gráfica de Swagger
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular"); // 👈 IMPORTANTE: Agregué esta línea para que Angular no tenga errores de CORS

app.UseAuthorization();

app.MapControllers();

app.Run();