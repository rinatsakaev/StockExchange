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
    }
}