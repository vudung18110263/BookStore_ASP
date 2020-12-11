using Book_Shop.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Book_Shop.Controllers
{
    public class StoreController : Controller
    {
        // GET: Store
        private Book_StoreEntities2 db = new Book_StoreEntities2();

        // GET: Products
        public ActionResult Index(int? page)
        {

            // 1. Tham số int? dùng để thể hiện null và kiểu int
            // page có thể có giá trị là null và kiểu int.

            // 2. Nếu page = null thì đặt lại là 1.
            if (page == null) page = 1;

            // 3. Tạo truy vấn, lưu ý phải sắp xếp theo trường nào đó, ví dụ OrderBy
            // theo LinkID mới có thể phân trang.
            var links = (from l in db.Products
                         select l).OrderBy(x => x.id);

            // 4. Tạo kích thước trang (pageSize) hay là số Link hiển thị trên 1 trang
            int pageSize = 4;

            // 4.1 Toán tử ?? trong C# mô tả nếu page khác null thì lấy giá trị page, còn
            // nếu page = null thì lấy giá trị 1 cho biến pageNumber.
            int pageNumber = (page ?? 1);

            // 5. Trả về các Link được phân trang theo kích thước và số trang.
            return View(links.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult About()
        {
            return View();
        }
        //public ActionResult CheckProdcutQuantity(string view)
        //{
        //    List<string> notification = new List<string>();
        //    List<itemInCart> cart = Session["cart"] as List<itemInCart>;
        //    foreach (var item in cart)
        //    {
        //        var product = db.Products.Where(x => x.id == item.product.id).FirstOrDefault();
        //        if (product.stock < item.quantity)
        //        {
        //            notification.Add("san pham :" + product.name + " chi con lai :" + product.stock);
        //        }
        //    }
        //    if (notification.Count > 0)
        //    {
        //        ViewBag.KQ = notification;
        //        return View(view);
        //    }
        //    else
        //        return View("Purchase");
        //} 
        public string CheckProdcutQuantity(List<itemInCart> cart)
        {
            string notification ="";
            foreach (var item in cart)
            {
                var product = db.Products.Where(x => x.id == item.product.id).FirstOrDefault();
                if (product.stock < item.quantity)
                {
                    return notification = notification + "san pham :" + product.name + " chi con lai :" + product.stock + "\n";
                }
            }
            return notification;
        }
        public ActionResult CheckoutProdcut(FormCollection form)
        {
            List<itemInCart> cart = Session["cart"] as List<itemInCart>;
            string Notification = CheckProdcutQuantity(cart);
            if (Notification != "")
                return RedirectToAction("Checkout2", "Store", new { notification = Notification });

            string idPromo="";
            var temp = Session["userId"].ToString();
            int Userid = int.Parse(temp);
            string promoCode = form["Promode"].ToString();
            string ShipAddress = form["Address"];
            var promode = db.PromoCodes.Where(x => x.code == promoCode).FirstOrDefault();
            if (promode != null)
                idPromo = (promode.id).ToString();
            return RedirectToAction("Pay", "Store", new { userID = Userid, promoID = idPromo, ShippingAddress= ShipAddress });
        }
        public ActionResult Pay(int userID,string promoID,string ShippingAddress )
        {
            string Payment = "cash";
            DateTime myDateTime = DateTime.Now;
            string Date = myDateTime.Date.ToString("yyyy-MM-dd");
            //tao order
            
            Order order = new Order()
            {
                userid = userID,
                status = "PENDING",
                date = myDateTime.Date,
                shippingAddess = ShippingAddress,
                payment = Payment
            };
            db.Orders.Add(order);
            db.SaveChanges();
            if (promoID != null)
                order.promoid = Convert.ToInt32(promoID);
            //tao orderprodcut
            List<itemInCart> cart = Session["cart"] as List<itemInCart>;
            foreach(var item in cart)
            {
                Order_Product order_Product = new Order_Product { 
                    orderId = order.id, 
                    productId = item.product.id, 
                    price = (int)item.product.price, 
                    quantity = item.quantity,
                };
                item.product.stock -= item.quantity;
                db.Entry(item.product).State = EntityState.Modified;
                db.Order_Product.Add(order_Product);
                db.SaveChanges();
            }
            return RedirectToAction("Index","Store");
        }
        public ActionResult Detail()
        {
            return View();
        }
        public ActionResult Products(int? page)
        {
            if (page == null) page = 1;
            var links = (from l in db.Products
                         select l).OrderBy(x => x.id);
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(links.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Single(int? id)
        {
            var product = db.Products.Where(x => x.id == id).FirstOrDefault();
            return View(product);
        }
        public ActionResult Mail()
        {
            return View();
        }
        public ActionResult Info()
        {
            var temp = Session["userId"].ToString();
            int id = int.Parse(temp);
            var user = db.Users.Where(x => x.id == id).FirstOrDefault();
            return View(user);
        }
        public partial class itemInCart
        {
            public Product product { get; set; }
            public int quantity { get; set; }
        }
        public class JsonResult
        {
            public string id { get; set; }
        }
        [HttpPost]
        public ActionResult AddToCart(JsonResult result)
        {
            if (Session["cart"] == null)
            {
                Session["cart"] = new List<itemInCart>();
            }

            int productId = int.Parse(result.id);
            List<itemInCart> cart = Session["cart"] as List<itemInCart>;

            // Lặp qua từng phần tử trong giỏ hàng
            foreach (var cartItem in cart)
            {
                // Kiểm tra nếu sản phẩm đã ở trong giỏ hàng rồi thì tăng quantity
                if (cartItem.product.id == productId)
                {
                    cartItem.quantity += 1;
                    return Json(cart, JsonRequestBehavior.AllowGet);
                }
            }
            // Nếu chưa có thì thêm vào giỏ hàng
            Product product = db.Products.SingleOrDefault(n => n.id == productId);
            product = new Product(product);
            itemInCart item = new itemInCart() { product = product, quantity = 1 };
            cart.Add(item);
            return Json(cart, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult minusFromCart(JsonResult result)
        {
            int productId = int.Parse(result.id);
            List<itemInCart> cart = Session["cart"] as List<itemInCart>;

            // Lặp qua từng phần tử trong giỏ hàng
            foreach (var cartItem in cart)
            {
                // Kiểm tra nếu sản phẩm đã ở trong giỏ hàng rồi thì tăng quantity
                if (cartItem.product.id == productId)
                {
                    //-----------------------------------------
                    cartItem.quantity = --cartItem.quantity;
                    if (cartItem.quantity == 0)
                    {
                        cart.Remove(cartItem);
                        return Json(cart, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(cart, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCart(JsonResult result)
        {
            return Json(Session["cart"], JsonRequestBehavior.AllowGet);
        }
        public ActionResult Checkout()
        {
            ViewBag.Userid = true;
            if (Session["userId"] == null)
            {
                ViewBag.Userid = false;
                return View();
            }
            var temp = Session["userId"].ToString();
            int id = int.Parse(temp);
            var user = db.Users.Where(x => x.id == id).FirstOrDefault();
            return View(user);
        }
        public ActionResult Checkout2(string notification )
        {
            ViewBag.KQ = MvcHtmlString.Create(notification.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Aggregate((a, b) => a + "<br />" + b));
            ViewBag.Userid = true;
            if (Session["userId"] == null)
            {
                ViewBag.Userid = false;
                return View("Checkout");
            }
            var temp = Session["userId"].ToString();
            int id = int.Parse(temp);
            var user = db.Users.Where(x => x.id == id).FirstOrDefault();
            return View("Checkout",user);
        }
        public ActionResult Purchase(string Status)
        {
            if (Session["userId"] == null)//kiem tra dang nhập ?
                return View();
            //lấy userid trên session
            var temp = Session["userId"].ToString();
            int idUser = int.Parse(temp);

            if (Status == null)
                Status = "ALL";
            try
            {
                //khởi tạo các đối tượng
                List<Order> listorder = new List<Order>();//danh sách các order
                int priceALL;
                if (Status == "ALL")
                {
                    listorder = db.Orders.Where(x => x.userid == idUser).ToList();
                }
                else
                {
                    listorder = db.Orders.Where(x => x.status == Status && x.userid == idUser).ToList();
                }
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
                        priceALL = priceALL + itemOrderPro.price;
                    }
                    Order_Detail order_Detail = new Order_Detail(item, listorderProJoinProducts, priceALL.ToString());
                    result.Add(order_Detail);
                    ViewBag.Count = result.Count();
                }
                return View(result);
            }
            catch
            {
                return View();
            }
        }
    }
}