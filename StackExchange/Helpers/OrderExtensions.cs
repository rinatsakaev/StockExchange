using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Models;

namespace StackExchange.Controllers
{
    public static class OrderExtensions
    {
        public static bool Execute(this Order current, IEnumerable<OrderQueue> othersQueues, ApplicationDbContext _context)
        {
            foreach (var queue in othersQueues)
                while (queue.TotalCount > 0)
                {
                    var otherOrder = queue.Queue.First();
                    _context.Logs.Add(new LogModel(current, otherOrder));

                    if (current.Quantity > otherOrder.Quantity)
                    {
                        current.ApplicationUser.Balance += otherOrder.Quantity * current.Price;
                        current.Quantity -= otherOrder.Quantity;
                        otherOrder.ApplicationUser.ItemCount += otherOrder.Quantity;

                        _context.Orders.Remove(otherOrder);
                        queue.Queue.Remove(otherOrder);
                        queue.TotalCount -= otherOrder.Quantity;
                    }
                    else
                    {
                        current.ApplicationUser.Balance += current.Quantity * current.Price;
                        otherOrder.Quantity -= current.Quantity;
                        otherOrder.ApplicationUser.ItemCount += current.Quantity;
                        queue.TotalCount -= current.Quantity;
                        current.Quantity = 0;
                    }

                    if (queue.TotalCount == 0)
                        _context.OrderQueues.Remove(queue);

                    if (current.Quantity != 0)
                        continue;

                    return true;
                } 
            return false;
        }
    }
}