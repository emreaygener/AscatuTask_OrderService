using Microsoft.EntityFrameworkCore;

namespace AscatuTask.Data
{
    public interface IOrderDbContext
    {
        public DbSet<Order> Orders { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
