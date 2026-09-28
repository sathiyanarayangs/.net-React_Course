using DataAccess.Core.Models;
using DataAccess.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EfCodeFirst;

/// <summary>
/// Task 4.7 - IRepository&lt;Student&gt; implemented over EF Core Code
/// First instead of raw ADO.NET. The interface and every method signature
/// are identical to AdoNetStudentRepository — that identity is the whole
/// point of the seam, and is what lets Task 4.11's config switch swap this
/// in without the API layer changing a single line.
/// </summary>
public class EfStudentRepository : IRepository<Student>
{
    private readonly AppDbContext _context;

    public EfStudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Student>> GetAllAsync() =>
        await _context.Students.AsNoTracking().ToListAsync();

    public async Task<Student?> GetByIdAsync(int id) =>
        await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Student> AddAsync(Student entity)
    {
        _context.Students.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> UpdateAsync(int id, Student entity)
    {
        var existing = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
        if (existing is null) return false;

        existing.Name = entity.Name;
        existing.Age = entity.Age;
        existing.Email = entity.Email;
        existing.EnrolledOn = entity.EnrolledOn;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
        if (existing is null) return false;

        _context.Students.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
