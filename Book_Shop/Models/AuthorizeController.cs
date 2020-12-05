using System.Web;
using System.Web.Mvc;

namespace Book_Shop.Models
{
    public class AuthorizeController : ActionFilterAttribute
    {
        Book_StoreEntities db = new Book_StoreEntities();
        //phương thức thực thi khi action được gọi
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            User tbus = HttpContext.Current.Session["user"] as User;
            if (tbus == null || tbus.lever != 1)
            {
                filterContext.Result = new RedirectResult("~/Home/Index");
                return;
            }
        }
    }
}