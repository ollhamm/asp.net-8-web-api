namespace aspnetcoreapi.Repositories.Interfaces;
using aspnetcoreapi.Models;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken ct = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<Role> AddAsync(Role role, CancellationToken ct = default);
    Task UpdateAsync(Role role, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}