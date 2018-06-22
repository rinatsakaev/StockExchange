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

                    WriteLog(current, otherOrder, _context);

                    if (current.Type == OrderType.Bid)
                        ExecuteBid(current, otherOrder, queue, _context);
                    else
                        ExecuteAsk(current, otherOrder, queue, _context);

                    if (queue.TotalCount == 0)
                        _context.OrderQueues.Remove(queue);

                    if (current.Quantity != 0)
                        continue;

                    return true;
                } 
            return false;
        }

        private static void ExecuteAsk(Order current, Order otherOrder, OrderQueue queue, ApplicationDbContext _context)
        {
            if (current.Quantity > otherOrder.Quantity)
            {
                current.ApplicationUser.Balance += otherOrder.Quantity * current.Price;
                current.Quantity -= otherOrder.Quantity;
                otherOrder.ApplicationUser.ItemCount += otherOrder.Quantity;

                _context.Orders.Remove(otherOrder);
                queue.Queue.Remove(otherOrder);
            }
            else
            {          
                current.ApplicationUser.Balance += current.Quantity * current.Price;
                otherOrder.Quantity -= current.Quantity;
                otherOrder.ApplicationUser.ItemCount += current.Quantity;
                current.Quantity = 0;
            }
        }

        private static void ExecuteBid(Order current, Order otherOrder, OrderQueue queue, ApplicationDbContext _context)
        {
            if (current.Quantity > otherOrder.Quantity)
            {
                current.ApplicationUser.ItemCount += otherOrder.Quantity;
                current.Quantity -= otherOrder.Quantity;
                otherOrder.ApplicationUser.Balance += current.Quantity * current.Price;

                _context.Orders.Remove(otherOrder);
                queue.Queue.Remove(otherOrder);
            }
            else
            {
                current.ApplicationUser.ItemCount += current.Quantity;
                otherOrder.ApplicationUser.Balance += current.Quantity * current.Price;
                otherOrder.Quantity -= current.Quantity;
                current.Quantity = 0;
            }

        }

        private static void WriteLog(Order current, Order other, ApplicationDbContext _context)
        {
            var askOrder = current.Type == OrderType.Ask ? current : other;
            var bidOrder = current.Type == OrderType.Bid ? current : other;
            var log = new LogModel
            {
                AskDate = askOrder.DateAdded,
                BidDate = bidOrder.DateAdded,
                AskEmail = askOrder.ApplicationUser.Email,
                BidEmail = bidOrder.ApplicationUser.Email,
                Count = askOrder.Quantity > bidOrder.Quantity ? bidOrder.Quantity : askOrder.Quantity,
                Price = bidOrder.Price,
                ExecutedDate = DateTime.Now,
            };
            _context.Logs.Add(log);
        }
    }
}