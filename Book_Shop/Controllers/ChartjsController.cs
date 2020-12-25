using Book_Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Book_Shop.Controllers
{
    public class ChartjsController : Controller
    {
        // GET: Chartjs
        private Book_StoreEntities2 db = new Book_StoreEntities2();
        // GET: Chart
        public ActionResult Index(int? month)
        {
            var ordersForBar = db.Order_Product.Where(o => o.Order.status == "DONE").ToList();
            var categoriesForBar = new List<string>();
            var dataForBar = new List<float>();
            foreach (var order in ordersForBar)
            {
                if (!categoriesForBar.Contains(order.Product.category))
                {
                    categoriesForBar.Add(order.Product.category);
                    dataForBar.Add(order.price * order.quantity);
                }
                else
                {
                    int index = categoriesForBar.IndexOf(order.Product.category);
                    dataForBar[index] += order.price * order.quantity;
                }

            }
            ViewBag.barLabels = categoriesForBar.ToArray();
            ViewBag.barData = dataForBar.ToArray();

            var now = DateTime.Now;
            var sevenDaysFromNow = now.AddDays(-7);
            var ordersForLine = db.Order_Product.Where(o => o.Order.status == "DONE" && o.Order.date <= now && o.Order.date >= sevenDaysFromNow).ToList();
            var dataForLine = new List<float>(new float[7] { 0, 0, 0, 0, 0, 0, 0 });
            var labelsForLine = new List<DateTime>(new DateTime[7] { now.Date.AddDays(-6), now.Date.AddDays(-5), now.Date.AddDays(-4), now.Date.AddDays(-3), now.Date.AddDays(-2), now.Date.AddDays(-1), now.Date });
            foreach (var order in ordersForLine)
            {
                if (labelsForLine.Contains(order.Order.date))
                {
                    int index = labelsForLine.IndexOf(order.Order.date);
                    dataForLine[index] += order.price * order.quantity;
                }
            }
            var stringLabelsForLine = new List<string>();
            foreach (var date in labelsForLine)
            {
                stringLabelsForLine.Add(date.ToString("dd-MM"));
            }
            ViewBag.lineLabels = stringLabelsForLine.ToArray();
            ViewBag.lineData = dataForLine.ToArray();

            return View();
        }
    }
}