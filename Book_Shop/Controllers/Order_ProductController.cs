using Book_Shop.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Book_Shop.Controllers
{
    [AuthorizeAdminController]
    public class Order_ProductController : Controller
    {
        private Book_StoreEntities2 db = new Book_StoreEntities2();

      
        public ActionResult Index()
        {
            var order_Product = db.Order_Product.Include(o => o.Order).Include(o => o.Product);
            return View(order_Product.ToList());
        }

       
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order_Product order_Product = db.Order_Product.Find(id);
            if (order_Product == null)
            {
                return HttpNotFound();
            }
            return View(order_Product);
        }

      
        public ActionResult Create()
        {
            ViewBag.orderId = new SelectList(db.Orders, "id", "status");
            ViewBag.productId = new SelectList(db.Products, "id", "name");
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,orderId,productId,quantity,price")] Order_Product order_Product)
        {
            if (ModelState.IsValid)
            {
                db.Order_Product.Add(order_Product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.orderId = new SelectList(db.Orders, "id", "status", order_Product.orderId);
            ViewBag.productId = new SelectList(db.Products, "id", "name", order_Product.productId);
            return View(order_Product);
        }

       
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order_Product order_Product = db.Order_Product.Find(id);
            if (order_Product == null)
            {
                return HttpNotFound();
            }
            ViewBag.orderId = new SelectList(db.Orders, "id", "status", order_Product.orderId);
            ViewBag.productId = new SelectList(db.Products, "id", "name", order_Product.productId);
            return View(order_Product);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,orderId,productId,quantity,price")] Order_Product order_Product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order_Product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.orderId = new SelectList(db.Orders, "id", "status", order_Product.orderId);
            ViewBag.productId = new SelectList(db.Products, "id", "name", order_Product.productId);
            return View(order_Product);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order_Product order_Product = db.Order_Product.Find(id);
            if (order_Product == null)
            {
                return HttpNotFound();
            }
            return View(order_Product);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order_Product order_Product = db.Order_Product.Find(id);
            db.Order_Product.Remove(order_Product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
