// Program.cs
using ServiceCommon.Domain.Port.Repositories; // Asegúrate de tener este using si IRepository es en ServiceCommon
using ServiceCommon.Infrastructure.Persistence.Data;
using ServiceUsuario.Application.Service;
using ServiceUsuario.Domain.Entities;
using ServiceUsuario.Infrastructure.Persistence.Factories; // Para UsuarioRepositoryCreator
using ServiceUsuario.Infrastructure.Persistence.Repositories; // Para IDB<Usuario>

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- REGISTRO DE DEPENDENCIAS ESPECÍFICO PARA EL MICROSERVICIO DE USUARIOS ---

// 1. Registro de la fábrica de conexiones como Singleton
// Esto asegura que haya una ÚNICA instancia de MySqlConnectionSingleton
// compartida por toda la aplicación, tal como el patrón Singleton lo dicta para la conexión a DB.
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionSingleton>();

// 2. Registro de la fábrica de repositorios de Usuario
// Se puede registrar como Singleton si la fábrica en sí no tiene estado por solicitud,
// o como Scoped si necesita resolver dependencias específicas del alcance de la solicitud.
// Para este caso, Singleton es razonable ya que solo encapsula la fábrica de conexiones.
builder.Services.AddSingleton<MySqlRepositoryFactory<Usuario>, UsuarioRepositoryCreator>();

// 3. Registro del servicio de aplicación UsuarioService
// Generalmente, los servicios de aplicación se registran como Scoped,
// lo que significa que se crea una nueva instancia para cada solicitud HTTP.
builder.Services.AddScoped<UsuarioService>();

// --- FIN DEL REGISTRO DE DEPENDENCIAS ---


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();