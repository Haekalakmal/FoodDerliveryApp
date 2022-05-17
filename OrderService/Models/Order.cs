using System;
using System.Collections.Generic;

namespace OrderService.Models
{
    public partial class Order
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public int UserId { get; set; }
        public int CourierId { get; set; }

        public virtual Courier Courier { get; set; } = null!;
    }
}
