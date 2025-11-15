using aspnetcoreapi.Repositories.Interfaces;
using aspnetcoreapi.Models;
using aspnetcoreapi.DTOs;

namespace aspnetcoreapi.Services;
public class ProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken ct = default) =>
        (await _repo.GetAllAsync(ct)).Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Images = p.Images.Select(i => i.ImageUrl).ToList()
        });

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? null : new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Images = p.Images.Select(i => i.ImageUrl).ToList()
        };
    }

    public async Task<ProductDto> AddAsync(ProductCreateRequest dto, List<string> imageUrls, CancellationToken ct = default)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Images = imageUrls.Select(url => new ProductImage { ImageUrl = url }).ToList()
        };
        var created = await _repo.AddAsync(product, ct);
        return new ProductDto
        {
            Id = created.Id,
            Name = created.Name,
            Price = created.Price,
            Images = created.Images.Select(i => i.ImageUrl).ToList()
        };
    }

    public async Task<bool> UpdateAsync(Guid id, ProductDto dto, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;

        // Buat list images baru dari DTO
        var newImages = dto.Images
            .Select(url => new ProductImage { ImageUrl = url, ProductId = existing.Id })
            .ToList();

        // Kirim data baru ke repository
        var updatedProduct = new Product
        {
            Id = existing.Id,
            Name = dto.Name,
            Price = dto.Price,
            Images = newImages
        };

        await _repo.UpdateAsync(updatedProduct, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;
        // Remove images associated with the product
        existing.Images.Clear();
        await _repo.DeleteAsync(id, ct);
        return true;
    }
}