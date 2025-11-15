using Microsoft.EntityFrameworkCore;
using aspnetcoreapi.Data;
using aspnetcoreapi.Models;
using aspnetcoreapi.Repositories.Interfaces;

namespace aspnetcoreapi.Repositories.Implementations;
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db) => _db = db;

    // GET ALL DATA
    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Products.AsNoTracking().ToListAsync(ct);

    // GET BY ID
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    // ADD DATA
    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        var e = (await _db.Products.AddAsync(product, ct)).Entity;
        await _db.SaveChangesAsync(ct);
        return e;
    }

    // UPDATE DATA
    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        var existing = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == product.Id, ct);

        if (existing == null) return;

        // Update property utama
        existing.Name = product.Name;
        existing.Price = product.Price;

        // Hapus semua images lama
        _db.ProductImages.RemoveRange(existing.Images);
        await _db.SaveChangesAsync(ct); // commit penghapusan dulu

        // Tambahkan images baru
        existing.Images = product.Images
            .Select(img => new ProductImage { ImageUrl = img.ImageUrl, ProductId = existing.Id })
            .ToList();

        await _db.SaveChangesAsync(ct); // commit penambahan
    }

    // DELETE DATA
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var p = await GetByIdAsync(id, ct);
        if (p is null) return;
        _db.Products.Remove(p);
        await _db.SaveChangesAsync(ct);
    }
}