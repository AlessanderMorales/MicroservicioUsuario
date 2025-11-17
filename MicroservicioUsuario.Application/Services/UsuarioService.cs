using MicroservicioUsuario.Application.Security;
using MicroservicioUsuario.Domain.Entities;
using MicroservicioUsuario.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace MicroservicioUsuario.Application.Services
{
    public class UsuarioService
    {
        private readonly IRepository<Usuario> _repo;

        public UsuarioService(IRepository<Usuario> repo)
        {
            _repo = repo;
        }

        // ==========================
        // CRUD
        // ==========================

        public IEnumerable<Usuario> Listar()
        {
            return _repo.Listar().Where(u => u.Estado == 1);
        }

        public Usuario? Obtener(int id) => _repo.Obtener(id);

        public void Crear(Usuario u)
        {
            u.Contraseña = PasswordHasher.HashPassword(u.Contraseña);
            _repo.Crear(u);
        }

        public void Editar(Usuario u)
        {
            u.Contraseña = PasswordHasher.HashPassword(u.Contraseña);
            _repo.Editar(u);
        }

        public void Eliminar(int id) => _repo.Eliminar(id);

        // ==========================
        // LOGIN (Con PBKDF2)
        // ==========================

        public Usuario? Login(string emailOrUser, string password)
        {
            var usuarios = _repo.Listar().ToList();

            var usuario = usuarios.FirstOrDefault(u =>
                (!string.IsNullOrEmpty(u.Email) &&
                 u.Email.Equals(emailOrUser, StringComparison.OrdinalIgnoreCase))
                ||
                (!string.IsNullOrEmpty(u.NombreUsuario) &&
                 u.NombreUsuario.Equals(emailOrUser, StringComparison.OrdinalIgnoreCase))
            );

            if (usuario == null)
                return null;

            // 🔥 VALIDACIÓN SIN HASH (TEXTO PLANO)
            if (usuario.Contraseña != password)
                return null;

            return usuario;
        }
    }
}
