using MicroservicioUsuario.Domain.Entities;
using MicroservicioUsuario.Domain.Interfaces;
using System.Collections.Generic;

namespace MicroservicioUsuario.Application.Services
{
    public class UsuarioService
    {
        private readonly IRepository<Usuario> _repo;

        public UsuarioService(IRepository<Usuario> repo)
        {
            _repo = repo;
        }

        public IEnumerable<Usuario> Listar() => _repo.GetAll();
        public Usuario Obtener(int id) => _repo.GetById(id);
        public void Crear(Usuario u) => _repo.Add(u);
        public void Editar(Usuario u) => _repo.Update(u);
        public void Eliminar(int id) => _repo.Delete(id);
    }
}
