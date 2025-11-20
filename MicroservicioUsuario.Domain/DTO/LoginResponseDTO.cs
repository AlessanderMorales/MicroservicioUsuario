namespace MicroservicioUsuario.Domain.DTO
{
    public class LoginResponseDTO
    {
        public bool Error { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int Id_Usuario { get; set; }
      public string Nombres { get; set; } = string.Empty;
   public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
   public string Rol { get; set; } = string.Empty;
        public bool RequiereCambioContraseña { get; set; }
    }
}
