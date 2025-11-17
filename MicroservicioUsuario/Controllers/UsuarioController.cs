using MicroservicioUsuario.Application.Services;
using MicroservicioUsuario.Domain.Entities;
using MicroservicioUsuario.Domain.DTO;
using MicroservicioUsuario.Domain.Requests;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/usuario")]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _service;

    public UsuarioController(UsuarioService service)
    {
        _service = service;
    }

    // GET: api/usuarios
    [HttpGet]
    public IActionResult GetAll()
    {
        var lista = _service.Listar();
        return Ok(lista);
    }

    // GET: api/usuarios/5
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var usuario = _service.Obtener(id);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    // POST: api/usuarios
    [HttpPost]
    public IActionResult Create([FromBody] UsuarioCrearRequest req)
    {
        var u = new Usuario
        {
            Nombres = req.Nombres,
            PrimerApellido = req.PrimerApellido,
            SegundoApellido = req.SegundoApellido,

            Email = req.Email,
            NombreUsuario = req.NombreUsuario,
            Rol = req.Rol,

            Contraseña = req.Contraseña,
            Estado = 1,
            RequiereCambioContraseña = true,
            FechaRegistro = DateTime.Now,
            UltimaModificacion = DateTime.Now
        };

        _service.Crear(u);

        return CreatedAtAction(nameof(Get), new { id = u.Id_Usuario }, u);
    }

    // PUT: api/usuarios/5
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Usuario u)
    {
        u.Id_Usuario = id;

        _service.Editar(u);

        var actualizado = _service.Obtener(id);
        if (actualizado == null) return NotFound();

        return Ok(actualizado);
    }

    // DELETE: api/usuarios/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var usuario = _service.Obtener(id);
        if (usuario == null)
            return NotFound();

        _service.Eliminar(id);

        return Ok(new { mensaje = "Usuario desactivado correctamente" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDTO dto)
    {
        var usuario = _service.Login(dto.EmailOrUser, dto.Password);
        if (usuario == null)
            return Unauthorized(new { mensaje = "Credenciales inválidas" });

        return Ok(new
        {
            usuario.Id_Usuario,
            usuario.Nombres,
            usuario.PrimerApellido,
            usuario.SegundoApellido,
            usuario.NombreUsuario,
            usuario.Email,
            usuario.Rol,
            usuario.RequiereCambioContraseña
        });
    }

}
