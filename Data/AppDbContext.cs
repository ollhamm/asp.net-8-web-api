using Microsoft.EntityFrameworkCore;
using aspnetcoreapi.Models; 

namespace aspnetcoreapi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } 
        public DbSet<Product> Products { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Users config
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Roles config
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<Role>()
                .Property(r => r.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // UserRole config
            modelBuilder.Entity<UserRole>()
                .Property(ur => ur.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Composite unique index: 1 user is not allowed to have the same role more than once
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();

            // Relation User -> UserRole
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relation Role -> UserRole
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override int SaveChanges()
        {
            SetTimestamps();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SetTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SetTimestamps()
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries()
                         .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                if (entry.Metadata.FindProperty("UpdatedAt") is not null)
                    entry.CurrentValues["UpdatedAt"] = utcNow;

                if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedAt") is not null)
                {
                    var current = entry.CurrentValues["CreatedAt"];
                    if (current is null || (current is DateTime dt && dt == default))
                        entry.CurrentValues["CreatedAt"] = utcNow;
                }
            }
        }
    }
}