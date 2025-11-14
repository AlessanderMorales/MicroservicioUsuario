using MicroservicioUsuario.Application.Services;
using MicroservicioUsuario.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/usuarios")]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _service;

    public UsuarioController(UsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_service.Listar());

    [HttpGet("{id}")]
    public IActionResult Get(int id) => Ok(_service.Obtener(id));

    [HttpPost]
    public IActionResult Create([FromBody] Usuario u)
    {
        _service.Crear(u);
        return Ok();
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Usuario u)
    {
        u.Id = id;
        _service.Editar(u);
        return Ok();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _service.Eliminar(id);
        return Ok();
    }
}
