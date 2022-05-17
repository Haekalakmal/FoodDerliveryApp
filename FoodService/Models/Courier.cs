using System;
using System.Collections.Generic;

namespace FoodService.Models
{
    public partial class Courier
    {
        public Courier()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string CourierName { get; set; } = null!;
        public int Phone { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
