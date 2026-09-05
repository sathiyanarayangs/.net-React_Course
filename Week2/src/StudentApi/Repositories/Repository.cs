namespace StudentApi.Repositories;

/// <summary>
/// Task 2.3 - the same IRepository&lt;T&gt; seam from Week 1's Repository/UoW
/// lab, now doing real work behind a real API instead of a console demo.
/// Swapping this for an EF Core-backed repository later means touching only
/// this file, never the service or controller layers.
/// </summary>
public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    T Add(T entity);
    bool Update(int id, T entity);
    bool Delete(int id);
}

/// <summary>
/// In-memory repository, registered as a Singleton in Program.cs so the
/// "database" survives across requests for the lifetime of the process.
/// Thread-safe via a lock, since AddSingleton means one shared instance is
/// hit concurrently by different requests.
/// </summary>
public abstract class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly Dictionary<int, T> _store = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    protected abstract int GetId(T entity);
    protected abstract void SetId(T entity, int id);

    public IEnumerable<T> GetAll()
    {
        lock (_lock) return _store.Values.ToList();
    }

    public T? GetById(int id)
    {
        lock (_lock) return _store.TryGetValue(id, out var entity) ? entity : null;
    }

    public T Add(T entity)
    {
        lock (_lock)
        {
            SetId(entity, _nextId);
            _store[_nextId] = entity;
            _nextId++;
            return entity;
        }
    }

    public bool Update(int id, T entity)
    {
        lock (_lock)
        {
            if (!_store.ContainsKey(id)) return false;
            SetId(entity, id);
            _store[id] = entity;
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock) return _store.Remove(id);
    }
}
