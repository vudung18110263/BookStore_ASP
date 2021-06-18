using Book_Shop.Models;
using PagedList;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Book_Shop.Controllers
{
    [AuthorizeAdminController]
    public class ProductsController : Controller
    {
        private Book_StoreEntities2 db = new Book_StoreEntities2();

       
        public ActionResult Index(int? page)
        {
           

           
            if (page == null) page = 1;

           
            var links = db.Products.Where(x => x.isable == 1).Include(p => p.User).OrderBy(x => x.id);

            
            int pageSize = 9;

           
            int pageNumber = (page ?? 1);

           
            return View(links.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult IndexProductUnIsable(int? page)
        {
           
            if (page == null) page = 1;

           
            var links = db.Products.Where(x => x.isable == 0).Include(p => p.User).OrderBy(x => x.id);

           
            int pageSize = 9;

           
            int pageNumber = (page ?? 1);
            ViewBag.page = page;
           
            return View(links.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult SellAllow(int? id, int? page)
        {
            var product = db.Products.Where(x => x.id == id).FirstOrDefault();
            product.isable = 1;
            db.SaveChanges();
            return RedirectToAction("IndexProductUnIsable", new { page = page });
        }

       
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

      
        public ActionResult Create()
        {
            var categories = db.Categories.ToList();
            ViewBag.Categories = categories;
            ViewBag.authorId = new SelectList(db.Users, "id", "account");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,image,description,category,price,rate,stock,authorId")] Product product)
        {
            if (ModelState.IsValid == false)
            {
                return View(product);
            }
            db.Products.Add(product);
            db.SaveChanges();
            var image = Request.Files["image"];
            var path = Server.MapPath("~/imageProduct/" + product.id + ".PNG");
            image.SaveAs(path);
            product.image = "/imageProduct/" + product.id + ".PNG";
            product.isable = 1;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            ViewBag.authorId = new SelectList(db.Users, "id", "account", product.authorId);
            return RedirectToAction("Index");
        }

       
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.authorId = new SelectList(db.Users, "id", "account", product.authorId);
            return View(product);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,image,description,category,price,rate,stock,authorId")] Product product)
        {
            ViewBag.Image = product.image;
            if (ModelState.IsValid)
            {
                var image = Request.Files["image"];
                var path = Server.MapPath("~/imageProduct/" + product.id + ".PNG");
                image.SaveAs(path);
                product.image = "/imageProduct/" + product.id + ".PNG";
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.authorId = new SelectList(db.Users, "id", "account", product.authorId);
            return View(product);
        }

      
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            product.isable = 2;
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
