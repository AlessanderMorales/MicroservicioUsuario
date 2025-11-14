namespace ServiceCommon.Domain.Port.Repositories;

public interface IRepository<T> 
{
    IEnumerable<T> GetAllAsync();
    T GetByIdAsync(int id);
    void AddAsync(T entity);
    void UpdateAsync(T entity);
    void DeleteAsync(int id);
    void DeleteAsync(T entity);

    void DeactivateByProjectId(int idProyecto);
}