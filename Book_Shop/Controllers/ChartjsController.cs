using Book_Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Book_Shop.Controllers
{
    [AuthorizeAdminController]
    public class ChartjsController : Controller
    {
        // GET: Chartjs
        private Book_StoreEntities2 db = new Book_StoreEntities2();
        // GET: Chart
        public ActionResult Index(int? month, FormCollection form)
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

            //biểu đồ cột theo tháng
            string yearBar;
            yearBar = form["year"];
            if (yearBar == null)
            {
                yearBar = now.Year.ToString();
            }
            var ordersForBar1 = db.Orders.Where(x => x.date.Year.ToString() == yearBar).ToList();
            var datamonthForBar = new List<int>();
            var monthForBar = new List<string>();
            for (int i = 1; i < 13; i++)
            {
                monthForBar.Add(i.ToString());
                datamonthForBar.Add(0);
            }
            int priceALL;

            List<Order_Detail> result = new List<Order_Detail>();//orderDetail khởi tạo trong folder model
            foreach (var item in ordersForBar1)
            {
                //khởi tạo các temp
                List<Order_Product> listOrderPro = new List<Order_Product>();
                List<OrderProJoinProduct> listorderProJoinProducts = new List<OrderProJoinProduct>();
                OrderProJoinProduct orderProJoinProduct = new OrderProJoinProduct();
                Product product = new Product();
                listOrderPro = db.Order_Product.Where(x => x.orderId == item.id).ToList();
                priceALL = 0;

                foreach (var itemOrderPro in listOrderPro)
                {
                    product = db.Products.Where(x => x.id == itemOrderPro.productId).FirstOrDefault();
                    orderProJoinProduct = new OrderProJoinProduct(itemOrderPro, product);
                    listorderProJoinProducts.Add(orderProJoinProduct);
                    priceALL = priceALL + itemOrderPro.price * itemOrderPro.quantity + Convert.ToInt32(item.shippingType) * 15000;
                }
                Order_Detail order_Detail = new Order_Detail(item, listorderProJoinProducts, priceALL);
                result.Add(order_Detail);
            }
            foreach (var order in result)
            {
                int index = monthForBar.IndexOf(order.date.Month.ToString());
                datamonthForBar[index] += order.PriceALl;
            }
            ViewBag.monthBarLabels = monthForBar.ToArray();
            ViewBag.monthBarData = datamonthForBar.ToArray();
            ViewBag.year = yearBar;
            return View();
        }
    }
}