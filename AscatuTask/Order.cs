using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AscatuTask
{
    public class Order
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PersonId { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }

    public class OrderViewModel
    {
        public string PersonId { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
    }
}
