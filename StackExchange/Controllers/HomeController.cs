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

            order.ApplicationUser = _context.Users.First(t => t.Id == userId);
            order.DateAdded = DateTime.Now;
            order.Price = Math.Round(order.Price, 2);

            var viewModel = new TradingViewModel
            {
                OrderQueues = _context.OrderQueues.Include(x => x.Queue).Select(x => x),
                Logs = _context.Logs.Select(x => x),
                User = _context.Users.First(t => t.Id == userId)
            };
            if (!ValidateOrder(order))
                return View("Index", viewModel);

            if (order.Type == OrderType.Ask)
                order.ApplicationUser.ItemCount -= order.Quantity;
            else
                order.ApplicationUser.Balance -= order.Quantity * order.Price;

            ModelState.Clear();
            if (ExecuteOrder(order))
                return View("Index", viewModel);

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
                    Queue = new List<Order>(),
                };
                orderQueue.Queue.Add(order);
                _context.OrderQueues.Add(orderQueue);
                _context.Orders.Add(order);
            }
            else
                orderInDb.Queue.Add(order);

            _context.SaveChanges();
        }

        private bool ExecuteOrder(Order order)
        {
            var orderQueue = _context.OrderQueues
                .Include(o => o.Queue).OrderBy(q => q.Price)
                .Where(o => o.Type != order.Type);

            orderQueue = order.Type == OrderType.Ask
                ? orderQueue.Where(o => o.Price >= order.Price)
                : orderQueue.Where(o => o.Price <= order.Price);

            var isCompleted = order.Execute(orderQueue.ToList(), _context);
            _context.SaveChanges();

            return isCompleted;
        }

        private bool ValidateOrder(Order order)
        {
            if (order.Price < 0)
            {
                ModelState.AddModelError("Error", "Price must be positive");
                return false;
            }

            if (order.Type == OrderType.Ask && order.ApplicationUser.ItemCount < order.Quantity)
            {
                ModelState.AddModelError("Error", "Items in stock < quantity");
                return false;
            }

            if (order.Type == OrderType.Bid && order.ApplicationUser.Balance < order.Quantity * order.Price)
            {
                ModelState.AddModelError("Error", "Not enough funds");
                return false;
            }

            return true;
        }
    }
}