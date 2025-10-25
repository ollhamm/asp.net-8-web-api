using Microsoft.EntityFrameworkCore;
using aspnetcoreapi.Data;
using aspnetcoreapi.Models;
using aspnetcoreapi.Repositories.Interfaces;

namespace aspnetcoreapi.Repositories.Implementations;
public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _db;
    public RoleRepository(AppDbContext db) => _db = db;

    // GET ALL DATA
    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Roles.AsNoTracking().ToListAsync(ct);

    // GET BY ID
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Roles.FindAsync(new object?[] { id }, ct);

    // GET BY NAME
    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == name, ct);

    // GET BY IDS FOR ROLE ASSIGNMENT
    public async Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
            await _db.Roles.Where(r => ids.Contains(r.Id)).ToListAsync(ct);

    // ADD DATA
    public async Task<Role> AddAsync(Role role, CancellationToken ct = default)
    {
        var e = (await _db.Roles.AddAsync(role, ct)).Entity;
        await _db.SaveChangesAsync(ct);
        return e;
    }

    // UPDATE DATA
    public async Task UpdateAsync(Role role, CancellationToken ct = default)
    {
        _db.Roles.Update(role);
        await _db.SaveChangesAsync(ct);
    }

    // DELETE DATA
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var u = await GetByIdAsync(id, ct);
        if (u is null) return;
        _db.Roles.Remove(u);
        await _db.SaveChangesAsync(ct);
    }
}