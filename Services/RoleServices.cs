using aspnetcoreapi.Repositories.Interfaces;
using aspnetcoreapi.Models;
using aspnetcoreapi.DTOs;

namespace aspnetcoreapi.Services;
public class RoleService
{
    private readonly IRoleRepository _repo;
    public RoleService(IRoleRepository repo) => _repo = repo;

    public async Task<IEnumerable<RoleDto>> GetAllAsync(CancellationToken ct = default) =>
        (await _repo.GetAllAsync(ct)).Select(r => new RoleDto { Id = r.Id, Name = r.Name });

    public async Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.GetByIdAsync(id, ct);
        return r is null ? null : new RoleDto { Id = r.Id, Name = r.Name };
    }

   public async Task<RoleDto> AddAsync(RoleCreateRequest req, CancellationToken ct = default)
    {
        var r = new Role { Name = req.Name };
        var created = await _repo.AddAsync(r, ct);
        return new RoleDto { Id = created.Id, Name = created.Name };
    }
            

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;
        await _repo.DeleteAsync(id, ct);
        return true;
    }
}