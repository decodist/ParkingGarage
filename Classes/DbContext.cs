using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DemoAppDotNet.Models;

namespace DemoAppDotNet.Classes
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        // Handle relationships and delete behaviors
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Floor -> Building
            modelBuilder.Entity<Floor>()
                .HasOne(f => f.Building)
                .WithMany(b => b.Floors)
                .HasForeignKey(f => f.BuildingId)
                .OnDelete(DeleteBehavior.NoAction);
        
            // Bay -> Floor
            modelBuilder.Entity<Bay>()
                .HasOne(b => b.Floor)
                .WithMany(f => f.Bays) 
                .HasForeignKey(b => b.FloorId)
                .OnDelete(DeleteBehavior.NoAction);
        
            // Spot -> Bay
            modelBuilder.Entity<Spot>()
                .HasOne(s => s.Bay)
                .WithMany(s => s.Spots)
                .HasForeignKey(s => s.BayId)
                .OnDelete(DeleteBehavior.NoAction);
        
            // Spot -> Floor
            modelBuilder.Entity<Spot>()
                .HasOne(s => s.Floor)
                .WithMany()
                .HasForeignKey(s => s.FloorId)
                .OnDelete(DeleteBehavior.NoAction);
        
            // Car -> Spot
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Spot)
                .WithMany(s => s.Cars)
                .HasForeignKey(c => c.SpotId)
                .OnDelete(DeleteBehavior.NoAction);
        
            base.OnModelCreating(modelBuilder);
        }
        
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Bay> Bays { get; set; }
        public DbSet<Spot> Spots { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Rate> Rates { get; set; }
        
        // Get Methods
        public async Task<TEntity?> GetByIdAsync<TEntity>(int id) where TEntity : class
        {
            return await Set<TEntity>().FindAsync(id);
        }

        public async Task<List<TEntity>> GetAllAsync<TEntity>() where TEntity : class
        {
            return await Set<TEntity>().ToListAsync();
        }
        
        public async Task<List<TEntity>> GetWhereAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return await Set<TEntity>().Where(predicate).ToListAsync();
        }

        // Insert Methods
        public async Task<TEntity> InsertAsync<TEntity>(TEntity entity) where TEntity : class
        {
            await Set<TEntity>().AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        public async Task<List<TEntity>> InsertRangeAsync<TEntity>(List<TEntity> entities) where TEntity : class
        {
            await Set<TEntity>().AddRangeAsync(entities);
            await SaveChangesAsync();
            return entities;
        }

        // Update Method
        public async Task<TEntity> UpdateAsync<TEntity>(TEntity entity) where TEntity : class
        {
            Set<TEntity>().Update(entity);
            await SaveChangesAsync();
            return entity;
        }

        // Delete Methods
        public async Task<bool> DeleteAsync<TEntity>(int id) where TEntity : class
        {
            var entity = await GetByIdAsync<TEntity>(id);
            if (entity == null) return false;

            Set<TEntity>().Remove(entity);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync<TEntity>(TEntity entity) where TEntity : class
        {
            Set<TEntity>().Remove(entity);
            await SaveChangesAsync();
            return true;
        }

        // Bulk operations
        public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            return await Database.ExecuteSqlRawAsync(sql, parameters);
        }

        // Check if entity exists
        public async Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return await Set<TEntity>().AnyAsync(predicate);
        }

        // Get count
        public async Task<int> CountAsync<TEntity>() where TEntity : class
        {
            return await Set<TEntity>().CountAsync();
        }

        public async Task<int> CountWhereAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return await Set<TEntity>().CountAsync(predicate);
        }
    }
}