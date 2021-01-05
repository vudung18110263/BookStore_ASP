using Book_Shop.Models;
using Newtonsoft.Json.Linq;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
namespace Book_Shop.Controllers
{
    [AuthorizeShipperController]
    public class ShipperController : Controller
    {
        // GET: Shipper
        private Book_StoreEntities2 db = new Book_StoreEntities2();
        public ActionResult Index()
        {
            var a = db.Orders.ToList();
            if (Session["userId"] == null)//kiem tra dang nhập ?
                return RedirectToAction("Index", "Store");

            try
            {
                //khởi tạo các đối tượng
                List<Order> listorder = new List<Order>();//danh sách các order
                int priceALL;
                listorder = db.Orders.Where(x => x.status == "SHIPPING").ToList();
                List<Order_Detail> result = new List<Order_Detail>();//orderDetail khởi tạo trong folder model

                foreach (var item in listorder)
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
                    ViewBag.Count = result.Count();
                }
                result.Sort();
                return View(result);
            }
            catch
            {
                return View();
            }
        }
        public ActionResult Deliveried (int? idOrder)
        {
            var order = db.Orders.Where(x => x.id == idOrder).FirstOrDefault();
            order.status = "DONE";
            db.SaveChanges();
            return RedirectToAction("Index", "Shipper");
        }
    }
}