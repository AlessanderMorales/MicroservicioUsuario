using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroservicioUsuario.Domain.DTO
{
    public class LoginDTO
    {
        public string EmailOrUser { get; set; }
        public string Password { get; set; }
    }
}

