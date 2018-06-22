using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace StackExchange.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        [Range(0, Double.PositiveInfinity)]
        public double Price { get; set; }

        [Range(0, Int32.MaxValue)]
        public int Quantity { get; set; }

        public DateTime DateAdded { get; set; }
        public OrderType Type { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }

        public int OrderQueueId { get; set; }
        public virtual OrderQueue OrderQueue { get; set; }

    }

    public enum OrderType
    {
        Bid = 0,
        Ask = 1
    }
}