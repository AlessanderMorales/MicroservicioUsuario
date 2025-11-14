using ServiceCommon.Infrastructure.Persistence.Data;
using ServiceCommon.Domain.Port.Repositories;
using ServiceUsuario.Domain.Entities;
using ServiceUsuario.Infrastructure.Persistence.Repositories;

namespace ServiceUsuario.Infrastructure.Persistence.Factories
{
    public class UsuarioRepositoryCreator : MySqlRepositoryFactory<Usuario>
    {
        public UsuarioRepositoryCreator(MySqlConnectionSingleton connectionFactory) : base(connectionFactory) { }

        public override IDB<Usuario> CreateRepository()
        {

            return new UsuarioRepository(this._connectionSingleton);
        }
    }
}