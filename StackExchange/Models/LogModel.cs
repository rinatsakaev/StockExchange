using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackExchange.Models
{
    public class LogModel
    {
        public int Id { get; set; }
        public DateTime ExecutedDate { get; set; }
        public DateTime BidDate { get; set; }
        public DateTime AskDate { get; set; }
        public double Price { get; set; }
        public int Count { get; set; }
        public string BidEmail { get; set; }
        public string AskEmail { get; set; }

        public LogModel()
        {
            
        }

        public LogModel(Order current, Order other)
        {
            var ask = current.Type == OrderType.Ask ? current : other;
            var bid = current.Type == OrderType.Bid ? current : other;
            ExecutedDate = DateTime.Now;
            AskDate = ask.DateAdded;
            BidDate = bid.DateAdded;
            AskEmail = ask.ApplicationUser.Email;
            BidEmail = bid.ApplicationUser.Email;
            Price = current.Price;
            Count = current.Quantity > other.Quantity ? other.Quantity : current.Quantity;
        }
    }
}