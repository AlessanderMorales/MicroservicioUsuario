namespace MicroservicioUsuario.Domain.Requests
{
    public class UsuarioActualizarRequest
    {
        public string Nombres { get; set; }
        public string PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
    }
}
