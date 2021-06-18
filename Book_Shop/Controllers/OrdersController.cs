using Book_Shop.Models;
using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Book_Shop.Controllers
{
    [AuthorizeAdminController]
    public class OrdersController : Controller
    {
        private Book_StoreEntities2 db = new Book_StoreEntities2();

       
        public ActionResult Index(int? page)
        {
           
            if (page == null) page = 1;

           
            var links = db.Orders.Include(o =>
            o.PromoCode).Include(o => o.User).OrderByDescending(x => x.date);

            
            int pageSize = 9;

          
            int pageNumber = (page ?? 1);

           
            return View(links.ToPagedList(pageNumber, pageSize));
        }
        [HttpPost]
        public ActionResult ViewOrderByMonth(FormCollection form)
        {
            var now = DateTime.Now;
            int monthPie, yearPie;
            string monthYear = form["month"];
            var orders = db.Orders.Include(o => o.PromoCode)
            .Include(o => o.User).OrderByDescending(x => x.date);
            if (monthYear != "")
            {
                monthPie = Convert.ToInt32(monthYear.Substring(5, 2));
                yearPie = Convert.ToInt32(monthYear.Substring(0, 4));
                orders = db.Orders.Where(x => x.date.Month == monthPie
                     && x.date.Year == yearPie).Include(o => o.PromoCode)
                     .Include(o => o.User).OrderByDescending(x => x.date);
                TempData["monthYear"] = monthYear;
            }
            return View(orders.ToList());
        }
        private Dictionary<string, string> GetMineTypes()
        {
            return new Dictionary<string, string>
            {
                {".xls","application/vnd.ms-excel" },
                {".xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
            };
        }
        public ActionResult Export()
        {
            string monthYear = "";
            var now = DateTime.Now;
            if (TempData["monthYear"] != null)
                monthYear = TempData["monthYear"] as string;
            string nameExcel = "myexport" + monthYear
                + now.Hour.ToString() + "_"
                + now.Minute.ToString() + "_"
                + now.Second.ToString();
            var result = ExportData(nameExcel, monthYear);
            if (result)
                ViewBag.message = "Export successfully";
            else
                ViewBag.message = "Export Fail";

            return Json(nameExcel, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Download(string nameExcel)
        {
            string filename = Server.MapPath("/") + "export\\" + nameExcel + ".xlsx";
            var memory = new MemoryStream();
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;
            var ext = Path.GetExtension(filename).ToLowerInvariant();
            var a = File(memory, GetMineTypes()[ext], Path.GetFileName(filename));
            return a;
        }

        private bool ExportData(string nameExcel, string monthYear)
        {
            bool result = false;
            try
            {
                string filename = Server.MapPath("/") + "\\export\\" + nameExcel + ".xlsx";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage pck = new ExcelPackage(new System.IO.FileInfo(filename));
               
                var ws = pck.Workbook.Worksheets.Add(nameExcel);
                int StartRow = 2;
                int priceALL;
                int monthPie;
                int yearPie;
              
                var orders = db.Orders.Where(x => x.status == "DONE").Include(o => o.PromoCode)
                .Include(o => o.User).OrderByDescending(x => x.date).ToList();
                if (monthYear != "")
                {
                    monthPie = Convert.ToInt32(monthYear.Substring(5, 2));
                    yearPie = Convert.ToInt32(monthYear.Substring(0, 4));
                    orders = db.Orders.Where(x => x.date.Month == monthPie
                         && x.date.Year == yearPie && x.status == "DONE").Include(o => o.PromoCode)
                         .Include(o => o.User).OrderByDescending(x => x.date).ToList();
                }

                List<Order_Detail> result2 = new List<Order_Detail>();

                foreach (var item in orders)
                {
                   
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
                        priceALL = priceALL + itemOrderPro.price * itemOrderPro.quantity + Convert.ToInt32(item.shippingType) * 15000;
                    }
                    Order_Detail order_Detail = new Order_Detail(item, listorderProJoinProducts, priceALL);
                    result2.Add(order_Detail);
                }
                result2.Sort();

                ws.Cells["A1"].Value = "id order";
                ws.Cells["B1"].Value = "user name";
                ws.Cells["C1"].Value = "Address";
                ws.Cells["D1"].Value = "promocode";
                ws.Cells["E1"].Value = "total bill";
                ws.Cells["F1"].Value = "status";
                ws.Cells["G1"].Value = "date";

                foreach (var item in result2)
                {

                    ws.Cells[string.Format("A{0}", StartRow)].Value = item.id;
                    ws.Cells[string.Format("B{0}", StartRow)].Value = item.UserFullName;
                    ws.Cells[string.Format("C{0}", StartRow)].Value = item.shippingAddess;
                    ws.Cells[string.Format("D{0}", StartRow)].Value = item.promoValue;
                    ws.Cells[string.Format("E{0}", StartRow)].Value = item.PriceALl;
                    ws.Cells[string.Format("F{0}", StartRow)].Value = item.status;
                    ws.Cells[string.Format("G{0}", StartRow)].Value = item.date.Day.ToString() + "/"
                        + item.date.Month.ToString() + "/" + item.date.Year.ToString();
                    StartRow++;
                }
                pck.Save();
                result = true;
            }
            catch { }
            return result;
        }

        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        public ActionResult Create()
        {
            ViewBag.promoid = new SelectList(db.PromoCodes, "id", "name");
            ViewBag.userid = new SelectList(db.Users, "id", "account");
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,userid,promoid,status,date,shippingAddess,payment")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.promoid = new SelectList(db.PromoCodes, "id", "name", order.promoid);
            ViewBag.userid = new SelectList(db.Users, "id", "account", order.userid);
            return View(order);
        }

        
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.promoid = new SelectList(db.PromoCodes, "id", "name", order.promoid);
            ViewBag.userid = new SelectList(db.Users, "id", "account", order.userid);
            return View(order);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,userid,promoid,status,date,shippingAddess,payment")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.promoid = new SelectList(db.PromoCodes, "id", "name", order.promoid);
            ViewBag.userid = new SelectList(db.Users, "id", "account", order.userid);
            return View(order);
        }

       
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
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
