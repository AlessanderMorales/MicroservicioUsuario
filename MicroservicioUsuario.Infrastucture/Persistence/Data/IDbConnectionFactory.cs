using System.Data;

namespace ServiceCommon.Infrastructure.Persistence.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}