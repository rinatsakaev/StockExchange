using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace StackExchange.Models
{
    public class OrderQueue
    {
        public int OrderQueueId { get; set; }

        [Range(0, Double.PositiveInfinity)]
        public double Price { get; set; }

        public int TotalCount { get; set; }

        public OrderType Type { get; set; }

        public virtual ICollection<Order> Queue { get; set; }
    }
}