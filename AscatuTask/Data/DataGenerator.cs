using Microsoft.EntityFrameworkCore;

namespace AscatuTask.Data
{
    public class DataGenerator
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new OrderDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<OrderDbContext>>()))
            {
                context.Database.Migrate();
                context.Database.EnsureCreated();

                if (context.Orders.Any())
                {
                    return;
                }

                context.Orders.AddRange(
                    new Order
                    {
                        Id = Guid.NewGuid().ToString(),
                        PersonId = "1",
                        Product = "Product 1",
                        Quantity = 1,
                        Date = DateTime.Now
                    },
                    new Order
                    {
                        Id = Guid.NewGuid().ToString(),
                        PersonId = "2",
                        Product = "Product 2",
                        Quantity = 2,
                        Date = DateTime.Now
                    },
                    new Order
                    {
                        Id = Guid.NewGuid().ToString(),
                        PersonId = "3",
                        Product = "Product 3",
                        Quantity = 3,
                        Date = DateTime.Now
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
