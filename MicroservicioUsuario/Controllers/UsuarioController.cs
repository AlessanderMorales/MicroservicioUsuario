using MicroservicioUsuario.Application.Services;
using MicroservicioUsuario.Domain.Entities;
using MicroservicioUsuario.Domain.DTO;
using MicroservicioUsuario.Domain.Requests;
using MicroservicioUsuario.Domain.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/usuario")]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _service;
    private readonly JwtService _jwtService;
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(UsuarioService service, JwtService jwtService, ILogger<UsuarioController> logger)
    {
        _service = service;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public IActionResult GetAll()
    {
        var lista = _service.Listar();
        return Ok(lista);
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult Get(int id)
    {
        if (id <= 0) return BadRequest(new { error = true, message = "El ID debe ser mayor a 0." });

        var usuario = _service.Obtener(id);
        if (usuario == null) return NotFound(new { error = true, message = "Usuario no encontrado." });

        return Ok(usuario);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Create([FromBody] UsuarioCrearRequest req)
    {
        try
        {
            if (req == null) return BadRequest(new { error = true, message = "Los datos del usuario son requeridos." });

            req.Nombres = InputValidator.SanitizeName(req.Nombres);
            req.PrimerApellido = InputValidator.SanitizeName(req.PrimerApellido);
            if (!string.IsNullOrEmpty(req.SegundoApellido)) req.SegundoApellido = InputValidator.SanitizeName(req.SegundoApellido);
            req.Email = InputValidator.SanitizeEmail(req.Email);
            req.NombreUsuario = InputValidator.SanitizeUsername(req.NombreUsuario);
            req.Rol = InputValidator.ValidateRole(req.Rol);
            InputValidator.ValidatePassword(req.Contraseña);

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
            _logger.LogInformation($"Usuario creado: {u.NombreUsuario}");

            return CreatedAtAction(nameof(Get), new { id = u.Id_Usuario }, u);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validación fallida al crear usuario: {ex.Message}");
            return BadRequest(new { error = true, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] UsuarioActualizarRequest req)
    {
        try
        {
            if (id <= 0) return BadRequest(new { error = true, message = "El ID debe ser mayor a 0." });
            if (req == null) return BadRequest(new { error = true, message = "Los datos del usuario son requeridos." });

            req.Nombres = InputValidator.SanitizeName(req.Nombres);
            req.PrimerApellido = InputValidator.SanitizeName(req.PrimerApellido);
            if (!string.IsNullOrEmpty(req.SegundoApellido)) req.SegundoApellido = InputValidator.SanitizeName(req.SegundoApellido);
            req.Email = InputValidator.SanitizeEmail(req.Email);
            req.Rol = InputValidator.ValidateRole(req.Rol);

            var usuarioExistente = _service.Obtener(id);
            if (usuarioExistente == null) return NotFound(new { error = true, message = "Usuario no encontrado." });

            usuarioExistente.Nombres = req.Nombres;
            usuarioExistente.PrimerApellido = req.PrimerApellido;
            usuarioExistente.SegundoApellido = req.SegundoApellido;
            usuarioExistente.Email = req.Email;
            usuarioExistente.Rol = req.Rol;
            usuarioExistente.UltimaModificacion = DateTime.Now;

            _service.Editar(usuarioExistente);
            _logger.LogInformation($"Usuario actualizado: {usuarioExistente.NombreUsuario}");

            var actualizado = _service.Obtener(id);
            return Ok(actualizado);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validación fallida al actualizar usuario: {ex.Message}");
            return BadRequest(new { error = true, message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Delete(int id)
    {
        if (id <= 0) return BadRequest(new { error = true, message = "El ID debe ser mayor a 0." });

        var usuario = _service.Obtener(id);
        if (usuario == null) return NotFound(new { error = true, message = "Usuario no encontrado." });

        _service.Eliminar(id);
        _logger.LogInformation($"Usuario desactivado: {id}");

        return Ok(new { error = false, message = "Usuario desactivado correctamente" });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginDTO dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.EmailOrUser) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { error = true, message = "Email/Usuario y contraseña son requeridos." });

            dto.EmailOrUser = InputValidator.SanitizeString(dto.EmailOrUser);

            var usuario = _service.Login(dto.EmailOrUser, dto.Password);
            if (usuario == null)
            {
                _logger.LogWarning($"Intento de login fallido para: {dto.EmailOrUser}");
                return Unauthorized(new { error = true, message = "Credenciales inválidas" });
            }

            var token = _jwtService.GenerateToken(
                usuario.Id_Usuario,
                usuario.NombreUsuario,
                usuario.Email,
                usuario.Rol,
                usuario.RequiereCambioContraseña
            );

            _logger.LogInformation($"Login exitoso: {usuario.NombreUsuario}");

            return Ok(new LoginResponseDTO
            {
                Error = false,
                Message = "Login exitoso",
                Token = token,
                Id_Usuario = usuario.Id_Usuario,
                Nombres = usuario.Nombres,
                PrimerApellido = usuario.PrimerApellido,
                SegundoApellido = usuario.SegundoApellido,
                NombreUsuario = usuario.NombreUsuario,
                Email = usuario.Email,
                Rol = usuario.Rol,
                RequiereCambioContraseña = usuario.RequiereCambioContraseña
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = true, message = ex.Message });
        }
    }

    [HttpPut("cambiar-contraseña/{id}")]
    [Authorize]
    public IActionResult CambiarContraseña(int id, [FromBody] CambiarPasswordRequest req)
    {
        try
        {
            if (id <= 0) return BadRequest(new { error = true, message = "El ID debe ser mayor a 0." });
            if (req == null || string.IsNullOrWhiteSpace(req.ContraseñaActual) || string.IsNullOrWhiteSpace(req.NuevaContraseña))
                return BadRequest(new { error = true, message = "Datos incompletos." });

            InputValidator.ValidatePassword(req.NuevaContraseña);

            bool ok = _service.CambiarContraseña(id, req.ContraseñaActual, req.NuevaContraseña);
            if (!ok)
            {
                _logger.LogWarning($"Intento fallido de cambio de contraseña para usuario {id}");
                return BadRequest(new { error = true, message = "La contraseña actual es incorrecta." });
            }

            _logger.LogInformation($"Contraseña cambiada para usuario {id}");
            return Ok(new { error = false, message = "Contraseña cambiada correctamente" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = true, message = ex.Message });
        }
    }
}
