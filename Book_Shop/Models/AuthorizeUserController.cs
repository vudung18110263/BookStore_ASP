using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Book_Shop.Models
{
    public class AuthorizeUserController : ActionFilterAttribute
    {
        Book_StoreEntities2 db = new Book_StoreEntities2();
        // GET: AuthorizeUser
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["userid"] == null)
            {
                filterContext.Result = new RedirectResult("~/Store/Index");
                return;
            }
            var temp = HttpContext.Current.Session["userid"].ToString();
            int id = int.Parse(temp);
            var user = db.Users.Where(x => x.id == id).FirstOrDefault();
            if (user.lever != 1 && user.lever != 2)
            {
                filterContext.Result = new RedirectResult("~/Store/Index");
                return;
            }
        }
    }
}