using Book_Shop.Models;
using PagedList;
using System.Collections.Generic;
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
            return View();
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