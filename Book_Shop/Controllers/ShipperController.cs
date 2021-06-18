using Book_Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace Book_Shop.Controllers
{
    [AuthorizeShipperController]
    public class ShipperController : Controller
    {
        
        private Book_StoreEntities2 db = new Book_StoreEntities2();
        public ActionResult Index()
        {
            var a = db.Orders.ToList();
            if (Session["userId"] == null)
                return RedirectToAction("Index", "Store");

            try
            {
             
                List<Order> listorder = new List<Order>();
                int priceALL;
                listorder = db.Orders.Where(x => x.status == "SHIPPING").ToList();
                List<Order_Detail> result = new List<Order_Detail>();

                foreach (var item in listorder)
                {
                    
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
        public ActionResult Deliveried(int? idOrder)
        {
            var order = db.Orders.Where(x => x.id == idOrder).FirstOrDefault();
            order.status = "DONE";
            db.SaveChanges();
            return RedirectToAction("Index", "Shipper");
        }
        public ActionResult DetailShipper(int? idOrder)
        {
            var order = db.Orders.Where(x => x.id == idOrder).FirstOrDefault();
            if (order == null)
                return RedirectToAction("Index");

           
            List<Order_Product> listOrderPro = new List<Order_Product>();
            listOrderPro = db.Order_Product.Where(x => x.orderId == order.id).ToList();

            int priceALL = 0;
            List<OrderProJoinProduct> listorderProJoinProducts = new List<OrderProJoinProduct>();
            OrderProJoinProduct orderProJoinProduct = new OrderProJoinProduct();
            Product product = new Product();
            foreach (var itemOrderPro in listOrderPro)
            {
                product = db.Products.Where(x => x.id == itemOrderPro.productId).FirstOrDefault();
                orderProJoinProduct = new OrderProJoinProduct(itemOrderPro, product);
                listorderProJoinProducts.Add(orderProJoinProduct);
                priceALL = priceALL + itemOrderPro.price * itemOrderPro.quantity;
            }
            priceALL += Convert.ToInt32(order.shippingType) * 15000;
            Order_Detail order_Detail = new Order_Detail(order, listorderProJoinProducts, priceALL);

            return View(order_Detail);
        }
    }
}