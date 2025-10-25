namespace aspnetcoreapi.Repositories.Interfaces;
using aspnetcoreapi.Models;

public interface IUserRepository
{
    // user register 
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User> AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default);

    // jwt
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);
}