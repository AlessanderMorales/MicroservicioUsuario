using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroservicioUsuario.Domain.Requests
{
    public class CambiarPasswordRequest
    {
        public string ContraseñaActual { get; set; }
        public string NuevaContraseña { get; set; }
    }
}

