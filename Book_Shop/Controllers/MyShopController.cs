﻿using Book_Shop.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Book_Shop.Controllers
{
    [AuthorizeUserController]
    public class MyShopController : Controller
    {
        private Book_StoreEntities2 db = new Book_StoreEntities2();
        // GET: MyShop
        public ActionResult Index()
        {
            int id = Convert.ToInt32(Session["userId"]);
            List<Product> listproduct = new List<Product>();
            foreach (var item in db.Products)
            {
                if (item.authorId == id && item.isable == 1)
                {
                    listproduct.Add(item);
                }
            }
            return View(listproduct);
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
            product.authorId = Convert.ToInt32(Session["userId"]);
            db.Products.Add(product);
            db.SaveChanges();
            var image = Request.Files["image"];
            var path = Server.MapPath("~/imageProduct/" + product.id + ".PNG");
            image.SaveAs(path);
            product.image = "/imageProduct/" + product.id + ".PNG";
            product.isable = 0;
            product.rate = 3;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            ViewBag.authorId = new SelectList(db.Users, "id", "account", product.authorId);
            return RedirectToAction("Index");
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
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            product.isable = 2;
            db.SaveChanges();
            return RedirectToAction("Index", "MyShop");
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
            return View(product);
        }
        [HttpPost]
        public ActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                var image = Request.Files["image"];
                if (image.ContentLength != 0)
                {
                    var path = Server.MapPath("~/imageProduct/" + product.name + ".png");
                    image.SaveAs(path);
                }
                product.image = "/imageProduct/" + product.name + ".png";
                product.authorId = Convert.ToInt32(Session["userId"]);
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "MyShop");
            }
            return View(product);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.SingleOrDefault(p => p.id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
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
            return RedirectToAction("OrderManagement", "MyShop", new { Status = "CANCELED" });
        }
        public ActionResult OrderManagement(string Status)
        {
            var a = db.Orders.ToList();
            if (Session["userId"] == null)
                return RedirectToAction("Index", "Store");

           
            var temp = Session["userId"].ToString();
            int idUser = int.Parse(temp);
            ViewBag.done = "false";
            if (Status == "DONE" || Status == null)
                ViewBag.Done = "true";

            if (Status == null)
                Status = "DONE";
            try
            {
                
                List<Order> listorder = new List<Order>();
                int priceALL;
                bool temp2;
                listorder = db.Orders.Where(x => x.status == Status).ToList();
                List<Order_Detail> result = new List<Order_Detail>();

                foreach (var item in listorder)
                {
                    
                    List<Order_Product> listOrderPro = new List<Order_Product>();
                    List<OrderProJoinProduct> listorderProJoinProducts = new List<OrderProJoinProduct>();
                    OrderProJoinProduct orderProJoinProduct = new OrderProJoinProduct();
                    Product product = new Product();
                    listOrderPro = db.Order_Product.Where(x => x.orderId == item.id).ToList();
                    priceALL = 0;
                    temp2 = false;
                    foreach (var itemOrderPro in listOrderPro)
                    {
                        product = db.Products.Where(x => x.id == itemOrderPro.productId).FirstOrDefault();
                        orderProJoinProduct = new OrderProJoinProduct(itemOrderPro, product);
                        if (orderProJoinProduct.authorid == idUser)
                        {
                            temp2 = true;
                            listorderProJoinProducts.Add(orderProJoinProduct);
                            priceALL = priceALL + itemOrderPro.price * itemOrderPro.quantity + Convert.ToInt32(item.shippingType) * 15000;
                            temp2 = true;
                        }
                    }
                    if (temp2 == true)
                    {
                        Order_Detail order_Detail = new Order_Detail(item, listorderProJoinProducts, priceALL);
                        result.Add(order_Detail);
                        ViewBag.Count = result.Count();
                    }
                }
                result.Sort();
                return View(result);
            }
            catch
            {
                return View();
            }
        }
        public ActionResult OrderManagementDone(FormCollection form)
        {
            int monthPie, yearPie;
            string monthYear = form["month"];
            var temp = Session["userId"].ToString();
            int idUser = int.Parse(temp);
            var listorder = db.Orders.Where(x => x.status == "DONE").Include(o => o.PromoCode)
            .Include(o => o.User).OrderByDescending(x => x.date).ToList();
            if (monthYear != "")
            {
                monthPie = Convert.ToInt32(monthYear.Substring(5, 2));
                yearPie = Convert.ToInt32(monthYear.Substring(0, 4));
                listorder = db.Orders.Where(x => x.date.Month == monthPie
                     && x.date.Year == yearPie && x.status == "DONE").Include(o => o.PromoCode)
                     .Include(o => o.User).OrderByDescending(x => x.date).ToList();
                TempData["monthYear"] = monthYear;
            }
            ViewBag.Done = "true";
            try
            {
               
                int priceALL;
                bool temp2;
                List<Order_Detail> result = new List<Order_Detail>();

                foreach (var item in listorder)
                {
                  
                    List<Order_Product> listOrderPro = new List<Order_Product>();
                    List<OrderProJoinProduct> listorderProJoinProducts = new List<OrderProJoinProduct>();
                    OrderProJoinProduct orderProJoinProduct = new OrderProJoinProduct();
                    Product product = new Product();
                    listOrderPro = db.Order_Product.Where(x => x.orderId == item.id).ToList();
                    priceALL = 0;
                    temp2 = false;
                    foreach (var itemOrderPro in listOrderPro)
                    {
                        product = db.Products.Where(x => x.id == itemOrderPro.productId).FirstOrDefault();
                        orderProJoinProduct = new OrderProJoinProduct(itemOrderPro, product);
                        if (orderProJoinProduct.authorid == idUser)
                        {
                            temp2 = true;
                            listorderProJoinProducts.Add(orderProJoinProduct);
                            priceALL = priceALL + itemOrderPro.price * itemOrderPro.quantity + Convert.ToInt32(item.shippingType) * 15000;
                            temp2 = true;
                        }
                    }
                    if (temp2 == true)
                    {
                        Order_Detail order_Detail = new Order_Detail(item, listorderProJoinProducts, priceALL);
                        result.Add(order_Detail);
                        ViewBag.Count = result.Count();
                    }
                }
                return View("OrderManagement", result);
            }
            catch
            {
                return View("OrderManagement");
            }
        }
        public ActionResult Export()
        {
            string monthYear = "";
            var now = DateTime.Now;
            if (TempData["monthYear"] != null)
                monthYear = TempData["monthYear"] as string;
            string nameExcel = "sellerExport" + monthYear
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
        private Dictionary<string, string> GetMineTypes()
        {
            return new Dictionary<string, string>
            {
                {".xls","application/vnd.ms-excel" },
                {".xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
            };
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
            return File(memory, GetMineTypes()[ext], Path.GetFileName(filename));
        }
        private bool ExportData(string nameExcel, string monthYear)
        {
            bool result = false;
            try
            {
                string filename = Server.MapPath("/") + "\\export\\" + nameExcel + ".xlsx";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage pck = new ExcelPackage(new System.IO.FileInfo(filename));
               
                var temp = Session["userId"].ToString();
                int idUser = int.Parse(temp);
                var ws = pck.Workbook.Worksheets.Add(nameExcel);
                int StartRow = 2;
                bool temp2;
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
                    TempData["monthYear"] = monthYear;
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
                    temp2 = false;
                    foreach (var itemOrderPro in listOrderPro)
                    {
                        product = db.Products.Where(x => x.id == itemOrderPro.productId).FirstOrDefault();
                        orderProJoinProduct = new OrderProJoinProduct(itemOrderPro, product);
                        if (orderProJoinProduct.authorid == idUser)
                        {
                            temp2 = true;
                            listorderProJoinProducts.Add(orderProJoinProduct);
                            priceALL = priceALL + itemOrderPro.price * itemOrderPro.quantity + Convert.ToInt32(item.shippingType) * 15000;
                            temp2 = true;
                        }
                    }
                    if (temp2 == true)
                    {
                        Order_Detail order_Detail = new Order_Detail(item, listorderProJoinProducts, priceALL);
                        result2.Add(order_Detail);
                        ViewBag.Count = result2.Count();
                    }
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
        public ActionResult ConfirmOrder(int? idOrder)
        {
            var order = db.Orders.Where(x => x.id == idOrder).FirstOrDefault();
            order.status = "GETTING";
            db.SaveChanges();
            return RedirectToAction("OrderManagement", "MyShop", new { Status = "PENDING" });
        }
        public ActionResult ComfirmGetedOrder(int? idOrder)
        {
            var order = db.Orders.Where(x => x.id == idOrder).FirstOrDefault();
            order.status = "SHIPPING";
            db.SaveChanges();
            return RedirectToAction("OrderManagement", "MyShop", new { Status = "GETTING" });
        }
    }
}