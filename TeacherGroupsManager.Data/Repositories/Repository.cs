using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Common;
using TeacherGroupsManager.Data.Context;

namespace TeacherGroupsManager.Data.Repositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> Query();
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}

public class GenericRepository<T>(TeacherGroupsDbContext context) : IGenericRepository<T> where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();
    public IQueryable<T> Query() => _set.AsQueryable();
    public Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default) => _set.ToListAsync(cancellationToken);
    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => _set.FindAsync([id], cancellationToken).AsTask();
    public Task AddAsync(T entity, CancellationToken cancellationToken = default) => _set.AddAsync(entity, cancellationToken).AsTask();
    public void Update(T entity) => _set.Update(entity);
    public void Delete(T entity) => _set.Remove(entity);
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => _set.AnyAsync(predicate, cancellationToken);
}

public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork(TeacherGroupsDbContext context) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = [];

    public IGenericRepository<T> Repository<T>() where T : class
    {
        if (_repositories.TryGetValue(typeof(T), out var repository))
        {
            return (IGenericRepository<T>)repository;
        }

        var created = new GenericRepository<T>(context);
        _repositories[typeof(T)] = created;
        return created;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return context.SaveChangesAsync(cancellationToken);
    }
}
