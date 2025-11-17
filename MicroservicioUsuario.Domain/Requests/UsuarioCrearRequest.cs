using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroservicioUsuario.Domain.Requests
{
    public class UsuarioCrearRequest
    {
        public string Nombres { get; set; }
        public string PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string Email { get; set; }
        public string NombreUsuario { get; set; }
        public string Rol { get; set; }
        public string Contraseña { get; set; }
    }
}
