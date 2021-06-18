using Book_Shop.Models;
using PagedList;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Book_Shop.Controllers
{
    [AuthorizeAdminController]
    public class PromoCodesController : Controller
    {
        private Book_StoreEntities2 db = new Book_StoreEntities2();

       
        public ActionResult Index(int? page)
        {
            

            
            if (page == null) page = 1;

            
            var links = (from l in db.PromoCodes
                         select l).OrderBy(x => x.id);

           
            int pageSize = 3;

            
            int pageNumber = (page ?? 1);

            
            return View(links.ToPagedList(pageNumber, pageSize));
        }

        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PromoCode promoCode = db.PromoCodes.Find(id);
            if (promoCode == null)
            {
                return HttpNotFound();
            }
            return View(promoCode);
        }

       
        public ActionResult Create()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,value,code")] PromoCode promoCode)
        {
            if (ModelState.IsValid)
            {
                db.PromoCodes.Add(promoCode);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(promoCode);
        }

        
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PromoCode promoCode = db.PromoCodes.Find(id);
            if (promoCode == null)
            {
                return HttpNotFound();
            }
            return View(promoCode);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,value,code")] PromoCode promoCode)
        {
            if (ModelState.IsValid)
            {
                db.Entry(promoCode).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(promoCode);
        }

        
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PromoCode promoCode = db.PromoCodes.Find(id);
            if (promoCode == null)
            {
                return HttpNotFound();
            }
            return View(promoCode);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PromoCode promoCode = db.PromoCodes.Find(id);
            db.PromoCodes.Remove(promoCode);
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
