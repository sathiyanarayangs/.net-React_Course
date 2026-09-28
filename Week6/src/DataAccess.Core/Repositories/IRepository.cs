namespace DataAccess.Core.Repositories;

/// <summary>
/// The seam. Task 4.2, 4.7, and 4.10 each implement this exact interface
/// once — over raw ADO.NET, over an EF Core Code First DbContext, and over
/// a Scaffold-DbContext-generated DB First context — and Task 4.11 proves
/// the service/API layer above never has to know or care which one is
/// currently wired up.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task<bool> UpdateAsync(int id, T entity);
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// Task 4.5 - the seam for the Week 3 login user, moved from an in-memory
/// Dictionary to a real ADO.NET-backed table without the login endpoint
/// itself changing at all.
/// </summary>
public interface IUserStore
{
    Task<Core.Models.User?> FindByUsernameAsync(string username);
    Task<Core.Models.User> AddAsync(Core.Models.User user);
}
