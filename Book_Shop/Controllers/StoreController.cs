using Book_Shop.Models;
using Newtonsoft.Json.Linq;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            var links = db.Products.Where(x => x.isable == 1 && x.User.isActive == 1).OrderByDescending(x => x.rate);

            // 4. Tạo kích thước trang (pageSize) hay là số Link hiển thị trên 1 trang
            int pageSize = 8;

            // 4.1 Toán tử ?? trong C# mô tả nếu page khác null thì lấy giá trị page, còn
            // nếu page = null thì lấy giá trị 1 cho biến pageNumber.
            int pageNumber = (page ?? 1);

            // 5. Trả về các Link được phân trang theo kích thước và số trang.
            var kq = links.ToPagedList(pageNumber, pageSize);
            return View(kq);
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
        private string CheckProdcutQuantity(List<itemInCart> cart)
        {
            string notification = "";
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
        //khỏi tạo list order trước khi thanh toán
        public List<Order> checkoutListOrderNeed(string ShipAddress, string typeShipping)
        {
            List<itemInCart> cart = Session["cart"] as List<itemInCart>;
            List<Order> orders = new List<Order>();
            List<int?> listAuthorld = new List<int?>();
            int Userid = Convert.ToInt32(Session["userId"]);
            DateTime myDateTime = DateTime.Now;
            try
            {
                foreach (var item in cart)
                {
                    if (!listAuthorld.Contains(item.product.authorId))
                    {
                        listAuthorld.Add(item.product.authorId);
                        Order order = new Order()
                        {
                            userid = Userid,
                            status = "dang xu ly", //don hàng chưa được xử lý
                            date = myDateTime.Date,
                            shippingAddess = ShipAddress,
                            shippingType = typeShipping
                        };
                        orders.Add(order);
                    }
                }
            }
            catch
            { }
            return orders;
        }
        //thanh toan
        [AuthorizeUserController]
        public ActionResult CheckoutProdcut(FormCollection form)
        {
            List<itemInCart> cart = Session["cart"] as List<itemInCart>;
            string Notification = CheckProdcutQuantity(cart);
            if (Notification != "")
                return RedirectToAction("Checkout2", "Store", new { notification = Notification });


            string idPromo = null;

            int Userid = Convert.ToInt32(Session["userId"]);
            string payOption = form["payOption"];
            string promoCode = form["Promode"].ToString();
            string ShipAddress = form["Address"];
            string typeShipping = form["shippgingOption"];

            DateTime myDateTime = DateTime.Now;
            string Date = myDateTime.Date.ToString("yyyy-MM-dd");
            List<Order> orders = checkoutListOrderNeed(ShipAddress, typeShipping);


            /* ======================================= chua tru ma giam gia */
            if (promoCode != null)
            {
                var promode = db.PromoCodes.Where(x => x.code == promoCode).FirstOrDefault();
                if (promode != null)
                    idPromo = (promode.id).ToString();
                if (idPromo != null)
                    orders[0].promoid = Convert.ToInt32(idPromo);
            }
            //tao orderprodcut
            TempData["orders"] = orders;
            if (payOption == "2")
            {
                return RedirectToAction("payMomo", "Store");
            }
            return RedirectToAction("Pay", "Store");
        }
        public ActionResult Pay()
        {
            try
            {
                List<itemInCart> cart = Session["cart"] as List<itemInCart>;
                List<int?> listauthorld = new List<int?>();
                List<Order> orders = TempData["orders"] as List<Order>;
                foreach (var order in orders)
                {
                    order.payment = "cash";
                    order.status = "PENDING";
                }
                foreach (var item in orders)
                    db.Orders.Add(item);
                db.SaveChanges();
                foreach (var item in cart)
                {
                    if (!listauthorld.Contains(item.product.authorId))
                    {
                        listauthorld.Add(item.product.authorId);
                    }
                    int index = listauthorld.IndexOf(item.product.authorId);
                    Order_Product order_Product = new Order_Product
                    {
                        orderId = orders[index].id,
                        productId = item.product.id,
                        price = (int)item.product.price,
                        quantity = item.quantity,
                    };
                    item.product.stock -= item.quantity;
                    db.Entry(item.product).State = EntityState.Modified;
                    db.Order_Product.Add(order_Product);
                    db.SaveChanges();
                }
                Session["cart"] = null;
            }
            catch { }
            return RedirectToAction("Purchase", "Store", new { Status = "PENDING" });
        }
        [AuthorizeUserController]
        public ActionResult payMomo()
        {
            List<itemInCart> cart = Session["cart"] as List<itemInCart>;
            List<Order> orders = TempData["orders"] as List<Order>;
            string endpoint = ConfigurationManager.AppSettings["endpoint"].ToString();
            string partnerCode = ConfigurationManager.AppSettings["partnerCode"].ToString();
            string accessKey = ConfigurationManager.AppSettings["accessKey"].ToString();
            string serectkey = ConfigurationManager.AppSettings["serectkey"].ToString();
            string orderInfo = ConfigurationManager.AppSettings["orderInfo"].ToString();
            string returnUrl = ConfigurationManager.AppSettings["returnUrl"].ToString();
            string notifyurl = ConfigurationManager.AppSettings["notifyurl"].ToString();

            //tim gia ma khuyen mai
            int promoValue;
            int tempPromoid = 0;
            if (orders[0].promoid != null)
                tempPromoid = (int)orders[0].promoid;
            var promo = db.PromoCodes.Where(x => x.id == tempPromoid).FirstOrDefault();
            if (promo != null)
            {
                promoValue = promo.value ?? default(int);
            }
            else
                promoValue = 0;

            int amountTemp = Convert.ToInt32(cart.Sum(n => (n.product.price) * (n.quantity)) + Convert.ToInt32(orders[0].shippingType) * 15000 * orders.Count - promoValue);
            string amount;
            if (amountTemp >= 0)
                amount = amountTemp.ToString();
            else
                amount = "0";
            string orderid = Guid.NewGuid().ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;

            //log.Debug("rawHash = " + rawHash);

            MoMoSecurity crypto = new MoMoSecurity();
            string signature = crypto.signSHA256(rawHash, serectkey);
            //log.Debug("Signature = " + signature);

            ////build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };
            //log.Debug("Json request to MoMo: " + message.ToString());
            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);
            return Redirect(jmessage.GetValue("payUrl").ToString());

        }
        [AuthorizeUserController]
        public ActionResult ReturnUrl()
        {
            string param = Request.QueryString.ToString().Substring(0, Request.QueryString.ToString().IndexOf("signature") - 1);
            param = Server.UrlDecode(param);
            MoMoSecurity crypto = new MoMoSecurity();
            string serectkey = ConfigurationManager.AppSettings["serectkey"].ToString();
            string signature = crypto.signSHA256(param, serectkey);
            ViewBag.message = "";
            //int a = Convert.ToInt32(Request["orderId"]);
            //var order = db.Orders.Where(x => x.id == a).FirstOrDefault();
            List<Order> orders = TempData["orders"] as List<Order>;
            foreach (var item in orders)
            {
                item.payment = "momo";
            }
            foreach (var item in orders)
                db.Orders.Add(item);
            db.SaveChanges();

            if (signature != Request["signature"].ToString())
            {
                ViewBag.message = "thông tin không hợp lệ";
                foreach (var item in orders)
                {
                    item.status = "thanh Toan that bai";
                }
                return View();
            }
            if (!Request.QueryString["errorCode"].Equals("0"))
            {
                foreach (var item in orders)
                {
                    item.status = "thanh Toan that bai";
                }
                ViewBag.message = "thanh toán thất bại";
                return View();
            }
            else
            {
                List<itemInCart> cart = Session["cart"] as List<itemInCart>;
                List<int?> listauthorld = new List<int?>();
                foreach (var order in orders)
                {
                    order.status = "PENDING";
                }
                foreach (var item in cart)
                {
                    if (!listauthorld.Contains(item.product.authorId))
                    {
                        listauthorld.Add(item.product.authorId);
                    }
                    int index = listauthorld.IndexOf(item.product.authorId);
                    Order_Product order_Product = new Order_Product
                    {
                        orderId = orders[index].id,
                        productId = item.product.id,
                        price = (int)item.product.price,
                        quantity = item.quantity,
                    };
                    item.product.stock -= item.quantity;
                    db.Entry(item.product).State = EntityState.Modified;
                    db.Order_Product.Add(order_Product);
                    db.SaveChanges();
                }
                Session["cart"] = null;
                ViewBag.message = "thanh toán thành công";
            }
            return RedirectToAction("Purchase", "Store", new { Status = "PENDING" });
        }
        [HttpPost]
        [AuthorizeUserController]
        public ActionResult NotifyUrl()
        {
            string param = "";
            param = "partner_code=" + Request["partner_code"] +
                "&access_key=" + Request["access_key"] +
                "&amount=" + Request["amount"] +
                "&order_id=" + Request["order_id"] +
                "&order_info=" + Request["order_info"] +
                "&order_type=" + Request["order_type"] +
                "&transaction_id" + Request["transaction_id"] +
                "&message=" + Request["message"] +
                "&response_time=" + Request["response_time"] +
                "&status_code=" + Request["status_code"];

            param = Server.UrlDecode(param);
            MoMoSecurity crypto = new MoMoSecurity();
            string serectkey = ConfigurationManager.AppSettings["serectkey"].ToString();
            string signature = crypto.signSHA256(param, serectkey);

            var order = db.Orders.Where(x => x.id == Convert.ToInt32(Request["order_id"])).FirstOrDefault();

            if (signature != Request["signature"].ToString())
            {

            }
            string status_code = Request["status_code"].ToString();
            if (status_code != "0")
            {
                order.payment = "momo";
                order.status = "thanh Toan that bai";//chờ xác nhận
            }
            else
            {
                order.payment = "momo";
                order.status = "PENDING";//chờ xác nhận
                List<itemInCart> cart = Session["cart"] as List<itemInCart>;
                foreach (var item in cart)
                {
                    Order_Product order_Product = new Order_Product
                    {
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
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        [AuthorizeUserController]
        public ActionResult DetailUser(int? idOrder)
        {
            var order = db.Orders.Where(x => x.id == idOrder).FirstOrDefault();
            if (order == null)
                return RedirectToAction("Purchase");

            //lấy ra danh sách product trong database
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
        [AuthorizeUserController]
        public ActionResult Purchase(string Status)
        {
            var a = db.Orders.ToList();
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
                        priceALL = priceALL + itemOrderPro.price * itemOrderPro.quantity;
                    }
                    priceALL += Convert.ToInt32(item.shippingType) * 15000;
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
        public ActionResult Products(int? page)
        {
            if (page == null) page = 1;
            var links = db.Products.Where(x => x.isable == 1).OrderBy(x => x.rate);
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(links.ToPagedList(pageNumber, pageSize));
        }
        [AuthorizeUserController]
        public ActionResult Single(int? id)
        {
            var product = db.Products.Where(x => x.id == id && x.isable == 1).FirstOrDefault();
            return View(product);
        }
        public ActionResult Mail()
        {
            return View();
        }
        [AuthorizeUserController]
        public ActionResult Info()
        {
            var temp = Session["userId"].ToString();
            int id = int.Parse(temp);
            var user = db.Users.Where(x => x.id == id).FirstOrDefault();
            return View(user);
        }
        [AuthorizeUserController]
        [HttpPost]
        public ActionResult CancelOrder(int? idOrder)
        {
            var order = db.Orders.Where(x => x.id == idOrder).FirstOrDefault();
            var listOrderProduct = db.Order_Product.Where(x => x.id == idOrder).ToList();
            foreach (var item in listOrderProduct)
            {
                var product = db.Products.Where(x => x.id == item.productId).FirstOrDefault();
                product.stock += item.quantity;
                db.SaveChanges();
            }
            order.status = "CANCELED";
            db.SaveChanges();
            return RedirectToAction("Purchase", "Store", new { Status = "CANCELED" });
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
        public ActionResult MinusFromCart(JsonResult result)
        {
            int productId = int.Parse(result.id);
            List<itemInCart> cart = Session["cart"] as List<itemInCart>;

            // Lặp qua từng phần tử trong giỏ hàng
            foreach (var cartItem in cart)
            {
                // Kiểm tra nếu sản phẩm đã ở trong giỏ hàng rồi thì trừ quantity
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
        public ActionResult Checkout2(string notification)
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
            return View("Checkout", user);
        }
        public static string ConvertToUnSign(string s)
        {
            s = s ?? "";
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').ToLower();
        }
        public ActionResult Search(int? page)
        {
            string query = ConvertToUnSign(Request.QueryString["q"]);

            string category = Request.QueryString["category"] ?? null;
            var links = db.Products.AsEnumerable();
            if (category != null)
            {
                links = links.Where(x => x.category == category && x.isable == 1).OrderByDescending(x => x.rate);
            }
            else
            {
                links = links.Where(x => (ConvertToUnSign(x.category).Contains(query) || ConvertToUnSign(x.name).Contains(query) || ConvertToUnSign(x.User.account).Contains(query)) && x.isable == 1).OrderByDescending(x => x.rate);
            }
            if (page == null) page = 1;
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            return View("Index", links.ToPagedList(pageNumber, pageSize));
        }
        public class JsonPromo
        {
            public string code { get; set; }
        }
        [AuthorizeUserController]
        [HttpPost]
        public ActionResult AddPromoCode(JsonPromo promo)
        {
            var promocode = db.PromoCodes.Where(p => p.code == promo.code).FirstOrDefault();
            if (promocode == null)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            return Json(promocode.value, JsonRequestBehavior.AllowGet);
        }
    }
}