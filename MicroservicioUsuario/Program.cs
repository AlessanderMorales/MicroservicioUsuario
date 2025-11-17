using MicroservicioUsuario.Application.Services;
using MicroservicioUsuario.Domain.Entities;
using MicroservicioUsuario.Domain.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Microservicio Usuario", Version = "v1" });
});

// DI
builder.Services.AddSingleton<MySqlConnectionSingleton>();

builder.Services.AddScoped<IRepository<Usuario>, UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapControllers();
app.Run();
