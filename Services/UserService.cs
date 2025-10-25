using aspnetcoreapi.Repositories.Interfaces;
using aspnetcoreapi.Models;
using aspnetcoreapi.DTOs;

namespace aspnetcoreapi.Services;
public class UserService
{
    private readonly IUserRepository _repo;
    public UserService(IUserRepository repo) => _repo = repo;

    public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken ct = default) =>
        (await _repo.GetAllAsync(ct)).Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email });

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var u = await _repo.GetByIdAsync(id, ct);
        return u is null ? null : new UserDto { Id = u.Id, Name = u.Name, Email = u.Email };
    }

    public async Task<UserDto> CreateAsync(UserDto dto, CancellationToken ct = default)
    {
        var u = new User { Name = dto.Name, Email = dto.Email };
        var created = await _repo.AddAsync(u, ct);
        return new UserDto { Id = created.Id, Name = created.Name, Email = created.Email };
    }

    public async Task<bool> UpdateAsync(Guid id, UserDto dto, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;
        existing.Name = dto.Name;
        existing.Email = dto.Email;
        await _repo.UpdateAsync(existing, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;
        await _repo.DeleteAsync(id, ct);
        return true;
    }
}