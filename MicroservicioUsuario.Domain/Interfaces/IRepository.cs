namespace MicroservicioUsuario.Domain.Interfaces
{
    public interface IRepository<T>
    {
        IEnumerable<T> Listar();
        T? Obtener(int id);
        void Crear(T entity);
        void Editar(T entity);
        void Eliminar(int id);
    }
}
