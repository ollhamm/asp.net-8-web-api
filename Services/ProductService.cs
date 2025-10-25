using aspnetcoreapi.Repositories.Interfaces;
using aspnetcoreapi.Models;
using aspnetcoreapi.DTOs;

namespace aspnetcoreapi.Services;
public class ProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken ct = default) =>
        (await _repo.GetAllAsync(ct)).Select(p => new ProductDto { Id = p.Id, Name = p.Name, Price = p.Price });

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? null : new ProductDto { Id = p.Id, Name = p.Name, Price = p.Price };
    }

    public async Task<ProductDto> AddAsync(ProductCreateRequest dto, CancellationToken ct = default)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price
        };
        var created = await _repo.AddAsync(product, ct);
        return new ProductDto { Id = created.Id, Name = created.Name, Price = created.Price };
    }

    public async Task<bool> UpdateAsync(Guid id, ProductDto dto, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;
        existing.Name = dto.Name;
        existing.Price = dto.Price;
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