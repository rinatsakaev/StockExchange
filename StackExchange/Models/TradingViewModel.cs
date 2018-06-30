using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackExchange.Models
{
    public class TradingViewModel
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<LogModel> Logs { get; set; }
        public IEnumerable<OrderQueue> OrderQueues { get; set; }
        public Order Order { get; set; }
      
    }
}