using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StackExchange.Models;
using System.Data.Entity;
using System.Runtime.InteropServices;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace StackExchange.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;

        public HomeController()
        {
            _context = new ApplicationDbContext();
        }

        [Authorize]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var orderQueues = _context.OrderQueues.Include(x => x.Queue);

            return View(new TradingViewModel
            {
                OrderQueues = orderQueues,
                Logs = _context.Logs.Select(x => x),
                User = _context.Users.FirstOrDefault(x => x.Id == userId)
            });
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddOrder(Order order)
        {
            var userId = User.Identity.GetUserId();
            var viewModel = new TradingViewModel
            {
                OrderQueues = _context.OrderQueues.Include(x => x.Queue).Select(x => x),
                Logs = _context.Logs.Select(x => x),
                User = _context.Users.First(t => t.Id == userId)
            };

            if (!ModelState.IsValid)
                return View("Index", viewModel);

            
            order.ApplicationUser = _context.Users.First(t => t.Id == userId);
            order.DateAdded = DateTime.Now;
            order.Price = Math.Round(order.Price, 2);

           

            if (order.Type == OrderType.Ask)
                order.ApplicationUser.ItemCount -= order.Quantity;
            else
                order.ApplicationUser.Balance -= order.Quantity * order.Price;

            ModelState.Clear();
            if (!ExecuteOrder(order))
                SaveOrder(order);

            return View("Index", viewModel);

        }

        private void SaveOrder(Order order)
        {
            var orderInDb = _context.OrderQueues
                .Include(o => o.Queue)
                .FirstOrDefault(o => o.Price == order.Price && o.Type == order.Type);

            if (orderInDb == null)
            {
                var orderQueue = new OrderQueue
                {
                    Price = order.Price,
                    Type = order.Type,
                    TotalCount = order.Quantity,
                    Queue = new List<Order>()
                };
                orderQueue.Queue.Add(order);
                _context.OrderQueues.Add(orderQueue);
                _context.Orders.Add(order);
            }
            else
            {
                orderInDb.Queue.Add(order);
                orderInDb.TotalCount+=order.Quantity;
            }
                 
            _context.SaveChanges();
        }

        private bool ExecuteOrder(Order order)
        {
            var orderQueue = _context.OrderQueues
                .Include(o => o.Queue)
                .Where(o => o.Type != order.Type);

            orderQueue = order.Type == OrderType.Ask
                ? orderQueue.OrderByDescending(o=>o.Price).Where(o => o.Price >= order.Price)
                : orderQueue.OrderBy(o=>o.Price).Where(o => o.Price <= order.Price);

            var isCompleted = order.Execute(orderQueue.ToList(), _context);
            _context.SaveChanges();

            return isCompleted;
        }

    }
}

