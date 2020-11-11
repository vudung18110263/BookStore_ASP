using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Book_Shop.Models;
using System.Data.Entity.Validation;
using System.IO;
using System.Text;
using System.Web.Helpers;
using System.Data.Entity;
using System.Web.Http.Results;

namespace Book_Shop.Controllers
{
    public class UsersController : Controller
    {
        private Book_StoreEntities2 db = new Book_StoreEntities2();
        // GET: Users
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult registration()
        {
            return View();
        }
        [HttpPost]
        public ActionResult registration(User user)
        {
            if (ModelState.IsValid)
            {
                var f = Request.Files["Avatar"];
                //if (user.account == null || user.pass_word == null || user.mail == null)
                //    ViewBag.KQ = "Please complete all information";
                //else if (db.Users.Any(x => x.account == user.account))
                //    ViewBag.KQ = "Account already exists";
                //else if (db.Users.Any(x => x.mail == user.mail))
                //    ViewBag.KQ = "email is already in use";
                //else 
                if (f != null && f.ContentLength > 0)
                {
                    try
                    {
                        var path = Server.MapPath("~/UploadFiles/" + user.account + ".PNG");
                        //ViewBag.Dung = f;
                        f.SaveAs(path);
                        user.avatar = "/UploadFiles/" + user.account + ".PNG";
                        db.Users.Add(user);
                        db.SaveChanges();
                        ViewBag.Kq = "Registration Successfully";
                    }
                    catch (DbEntityValidationException ex)
                    {
                        foreach (var errors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in errors.ValidationErrors)
                            {
                                // get the error message 
                                string errorMessage = validationError.ErrorMessage;
                                ViewBag.ErrorLog = errorMessage;
                            }
                        }
                    }
                }
                else
                {
                    user.lever = 1;
                    db.Users.Add(user);
                    db.SaveChanges();
                    ViewBag.Kq = " Registration Successfully";
                }
            }
            return View(user);
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            string taikhoan = f["username"].ToString();
            string matkhau = f["password"].ToString();
            var us = db.Users.SingleOrDefault(n => n.account == taikhoan && n.pass_word == matkhau);
            //nếu user nhập đúng mật khẩu
            if (us != null)
            {
                    Session["user"] = us;
                    return Content("/Home/Index");
            }
            return Content("false");
        }
        public ActionResult UplaodImage(string image)
        {
            ViewBag.image = image;
            return View();
        }
        public ActionResult SendMailGetPassWord(User user)
        {
            if (user.account == null)
                return View();
            if (db.Users.Any(x => x.account == user.account))
            {
                string a = RandomString(8);
                var user2 = db.Users.Where(x => x.account == user.account).FirstOrDefault();
                if (SendView(user2.mail, a))
                {
                    user2.pass_word = a;
                    db.Entry(user2).State = EntityState.Modified;
                    db.SaveChanges();
                    ViewBag.Kq = "Email Sent Successfully.";
                }
                else
                    ViewBag.Kq = "Problem while sending email.";
            }
            else
                ViewBag.Kq = "There is no such account ";
            return View();
        }
        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }
        private bool SendView(string EMAIL,string a)
        {
            try
            {
                //Configuring webMail class to send emails  
                //gmail smtp server  
                WebMail.SmtpServer = "smtp.gmail.com";
                //gmail port to send emails  
                WebMail.SmtpPort = 587;
                WebMail.SmtpUseDefaultCredentials = true;
                //sending emails with secure protocol  
                WebMail.EnableSsl = true;
                //EmailId used to send emails from application  
                WebMail.UserName = "tranhanam1999hn@gmail.com";
                WebMail.Password = "0898546564";

                //Sender email address.  
                WebMail.From = "tranhanam1999hn@gmail.com";

                //Send email  
                WebMail.Send(to: EMAIL, subject: "Change Your password", body: "New you password " + a, cc:null, bcc:null, isBodyHtml: true);
                return true; //"Email Sent Successfully.";
            }
            catch (Exception)
            {
                return false;// "Problem while sending email, Please check details.";
            }
        }
    }
}