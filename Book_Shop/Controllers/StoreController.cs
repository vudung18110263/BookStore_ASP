using Book_Shop.Models;
using PagedList;
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
        public ActionResult Products()
        {
            return View();
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
    }
}