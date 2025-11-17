using Dapper;
using MicroservicioUsuario.Domain.Entities;
using MicroservicioUsuario.Domain.Interfaces;

public class UsuarioRepository : IRepository<Usuario>
{
    private readonly MySqlConnectionSingleton _connection;

    public UsuarioRepository(MySqlConnectionSingleton connection)
    {
        _connection = connection;
    }

    // ============================================================
    // LISTAR SOLO ACTIVOS
    // ============================================================
    public IEnumerable<Usuario> Listar()
    {
        using var conn = _connection.CreateConnection();
        return conn.Query<Usuario>(@"
            SELECT 
                id_usuario AS Id_Usuario,
                nombres AS Nombres,
                primer_apellido AS PrimerApellido,
                segundo_apellido AS SegundoApellido,
                nombre_usuario AS NombreUsuario,
                contraseña AS Contraseña,
                email AS Email,
                rol AS Rol,
                estado AS Estado,
                requiere_cambio_contraseña AS RequiereCambioContraseña,
                fechaRegistro AS FechaRegistro,
                ultimaModificacion AS UltimaModificacion
            FROM Usuario
            WHERE estado = 1;
        ");
    }

    // ============================================================
    // OBTENER POR ID
    // ============================================================
    public Usuario? Obtener(int id)
    {
        using var conn = _connection.CreateConnection();
        return conn.QueryFirstOrDefault<Usuario>(@"
            SELECT 
                id_usuario AS Id_Usuario,
                nombres AS Nombres,
                primer_apellido AS PrimerApellido,
                segundo_apellido AS SegundoApellido,
                nombre_usuario AS NombreUsuario,
                email AS Email,
                contraseña AS Contraseña,
                rol AS Rol,
                estado AS Estado,
                requiere_cambio_contraseña AS RequiereCambioContraseña,
                fechaRegistro AS FechaRegistro,
                ultimaModificacion AS UltimaModificacion
            FROM Usuario
            WHERE id_usuario = @id;
        ", new { id });
    }

    // ============================================================
    // CREAR
    // ============================================================
    public void Crear(Usuario u)
    {
        using var conn = _connection.CreateConnection();
        conn.Execute(@"
            INSERT INTO Usuario 
                (nombres, primer_apellido, segundo_apellido, nombre_usuario, 
                 email, contraseña, rol, estado, requiere_cambio_contraseña,
                 fechaRegistro, ultimaModificacion)
            VALUES 
                (@Nombres, @PrimerApellido, @SegundoApellido, @NombreUsuario, 
                 @Email, @Contraseña, @Rol, @Estado, @RequiereCambioContraseña,
                 NOW(), NOW());
        ", u);
    }

    // ============================================================
    // EDITAR
    // ============================================================
    public void Editar(Usuario u)
    {
        using var conn = _connection.CreateConnection();
        conn.Execute(@"
            UPDATE Usuario SET
                nombres = @Nombres,
                primer_apellido = @PrimerApellido,
                segundo_apellido = @SegundoApellido,
                nombre_usuario = @NombreUsuario,
                email = @Email,
                contraseña = @Contraseña,
                rol = @Rol,
                estado = @Estado,
                requiere_cambio_contraseña = @RequiereCambioContraseña,
                ultimaModificacion = NOW()
            WHERE id_usuario = @Id_Usuario;
        ", u);
    }

    // ============================================================
    // ELIMINAR LÓGICO (estado = 0)
    // ============================================================
    public void Eliminar(int id)
    {
        using var conn = _connection.CreateConnection();
        conn.Execute(@"
            UPDATE Usuario
            SET 
                estado = 0,
                ultimaModificacion = NOW()
            WHERE id_usuario = @id;
        ", new { id });
    }
}
