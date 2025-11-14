using ServiceCommon.Domain.Port.Repositories;


namespace ServiceCommon.Infrastructure.Persistence.Data
{
    public abstract class MySqlRepositoryFactory<T>
    {
        public readonly MySqlConnectionSingleton _connectionSingleton;

        public MySqlRepositoryFactory(MySqlConnectionSingleton connectionFactory)
        {
            _connectionSingleton = connectionFactory;
        }

        public abstract IDB<T> CreateRepository();


    }
}