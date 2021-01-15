using Book_Shop.Models;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Book_Shop.Controllers
{
    public class ManageUserController : Controller
    {
        // GET: ManageUser
        private Book_StoreEntities2 db = new Book_StoreEntities2();
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }
        public ActionResult Disable(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Disable")]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            user.isActive = 0;
            db.SaveChanges();
            return RedirectToAction("Index", "ManageUser");
        }
        public ActionResult Enable(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Enable")]
        public ActionResult Enablefirmed(int id)
        {
            User user = db.Users.Find(id);
            user.isActive = 1;
            db.SaveChanges();
            return RedirectToAction("Index", "ManageUser");
        }
    }
}