using MicroservicioUsuario.Domain.Entities;
using MicroservicioUsuario.Domain.Interfaces;
using System.Collections.Generic;
using Dapper;

namespace MicroservicioUsuario.Infrastructure.Repository
{
    public class UsuarioRepository : IRepository<Usuario>
    {
        private readonly MySqlConnectionSingleton _connection;

        public UsuarioRepository(MySqlConnectionSingleton connection)
        {
            _connection = connection;
        }

        public IEnumerable<Usuario> GetAll()
        {
            using var conn = _connection.CreateConnection();

            return conn.Query<Usuario>(
                @"SELECT 
                    id_usuario AS Id,
                    nombres AS Nombres,
                    primer_apellido AS PrimerApellido,
                    segundo_apellido AS SegundoApellido,
                    nombre_usuario AS NombreUsuario,
                    contraseña AS Contraseña,
                    email AS Email,
                    rol AS Rol,
                    estado AS Estado,
                    requiere_cambio_contraseña AS RequiereCambioContraseña,
                    fechaRegistro,
                    ultimaModificacion
                FROM Usuario
                WHERE estado = 1
                ORDER BY id_usuario DESC"
            );
        }

        public Usuario GetById(int id)
        {
            using var conn = _connection.CreateConnection();

            return conn.QueryFirstOrDefault<Usuario>(
                @"SELECT 
                    id_usuario AS Id,
                    nombres AS Nombres,
                    primer_apellido AS PrimerApellido,
                    segundo_apellido AS SegundoApellido,
                    nombre_usuario AS NombreUsuario,
                    contraseña AS Contraseña,
                    email AS Email,
                    rol AS Rol,
                    estado AS Estado,
                    requiere_cambio_contraseña AS RequiereCambioContraseña,
                    fechaRegistro,
                    ultimaModificacion
                FROM Usuario
                WHERE id_usuario = @Id",
                new { Id = id }
            );
        }

        public void Add(Usuario entity)
        {
            using var conn = _connection.CreateConnection();

            conn.Execute(
                @"INSERT INTO Usuario 
                    (nombres, primer_apellido, segundo_apellido, nombre_usuario,
                     contraseña, email, rol, estado, requiere_cambio_contraseña)
                 VALUES
                    (@Nombres, @PrimerApellido, @SegundoApellido, @NombreUsuario,
                     @Contraseña, @Email, @Rol, @Estado, @RequiereCambioContraseña)",
                entity
            );
        }

        public void Update(Usuario entity)
        {
            using var conn = _connection.CreateConnection();

            conn.Execute(
                @"UPDATE Usuario SET
                    nombres = @Nombres,
                    primer_apellido = @PrimerApellido,
                    segundo_apellido = @SegundoApellido,
                    nombre_usuario = @NombreUsuario,
                    contraseña = @Contraseña,
                    email = @Email,
                    rol = @Rol,
                    requiere_cambio_contraseña = @RequiereCambioContraseña
                 WHERE id_usuario = @Id",
                entity
            );
        }

        public void Delete(int id)
        {
            using var conn = _connection.CreateConnection();

            conn.Execute(
                "UPDATE Usuario SET estado = 0 WHERE id_usuario = @Id",
                new { Id = id }
            );
        }
    }
}
