using Microsoft.EntityFrameworkCore;
using aspnetcoreapi.Data;
using aspnetcoreapi.Models;
using aspnetcoreapi.Repositories.Interfaces;

namespace aspnetcoreapi.Repositories.Implementations;
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    // GET ALL DATA
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Users.AsNoTracking().ToListAsync(ct);

    // GET BY ID
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Users.FindAsync(new object?[] { id }, ct);

    // GET BY EMAIL
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    // ASSIGN ROLE TO USER
    public async Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };
        await _db.UserRoles.AddAsync(userRole, ct);
        await _db.SaveChangesAsync(ct);
    }

    // ADD DATA
    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        var e = (await _db.Users.AddAsync(user, ct)).Entity;
        await _db.SaveChangesAsync(ct);
        return e;
    }

    // UPDATE DATA
    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }

    // DELETE DATA
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var u = await GetByIdAsync(id, ct);
        if (u is null) return;
        _db.Users.Remove(u);
        await _db.SaveChangesAsync(ct);
    }

    // GET BY ID WITH ROLES (jwt purpose)
    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default) =>
        await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
}