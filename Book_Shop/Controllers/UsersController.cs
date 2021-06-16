using Book_Shop.Models;
using Common;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Book_Shop.Controllers
{
    public class UsersController : Controller
    {
        private static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");

            }
            return byte2String;
        }
        private Book_StoreEntities2 db = new Book_StoreEntities2();
        // GET: Users
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(FormCollection form)
        {
            if (form["username"] == null ||
                form["password"] == null ||
                form["confirmPassword"] == null ||
                form["email"] == null ||
                form["phone"] == null ||
                form["address"] == null ||
                form["fullname"] == null)
            {
                return RedirectToAction("Index", "Store");
            }
            try {
                string username = form["username"].ToString();
                string password = form["password"].ToString();
                string confirmPassword = form["confirmPassword"].ToString();
                string email = form["email"].ToString();
                string phone = form["phone"].ToString();
                string address = form["address"].ToString();
                string fullname = form["fullname"].ToString();
                //int payment = form["payment"];
                string avatarPath = "";
                //if (form["payment"].ToString() == null)
                //{
                //    payment = null;
                //}
                var avatar = Request.Files["Avatar"];

                var user = db.Users.SingleOrDefault(n => n.account == username);
                var user2 = db.Users.SingleOrDefault(n => n.mail == email);
                if (user != null || user2 != null)
                {
                    ViewBag.Result = "User or Email existed ";
                    return View();
                }
                if (password != confirmPassword)
                {
                    ViewBag.Result = "Password not matched";
                    return View();
                }
                //if (avatar.FileName != "" && avatar.ContentLength > 0)
                if (avatar != null)
                {
                    var path = Server.MapPath("~/UploadFiles/" + username + ".PNG");
                    avatar.SaveAs(path);
                    avatarPath = "/UploadFiles/" + username + ".PNG";
                }
                User newUser = new User() { account = username, pass_word = GetMD5(password), lever = 2, mail = email, phone = phone, avatar = avatarPath, paymentId = null, address = address, fullname = fullname, isActive = 1 };
                TempData["user"] = newUser;
                string confirmMail = RandomString(8);
                TempData["confirmMail"] = confirmMail;
                new MailHelper().SendMail(newUser.mail, "Confirm Gmail", "Ma xac nhan Gmail " + confirmMail, "");
                //db.Users.Add(newUser);
                //db.SaveChanges();
                return RedirectToAction("ConfirmGmail");
            } catch {
                return RedirectToAction("Index", "Store");
            }
            
        }
        public ActionResult ConfirmGmail()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ConfirmGmail(string confirm)
        {
            User newuser = TempData["user"] as User;
            if (confirm == TempData["confirmMail"] as string)
            {
                db.Users.Add(newuser);
                db.SaveChanges();
                return Redirect("/");
            }
            else
            {
                ViewBag.error = "Mã xác nhận không đúng";
            }
            return View();
        }
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(FormCollection form)
        {
            string username = form["username"].ToString();
            string password =form["password"].ToString();
            string newPassword = form["newPassword"].ToString();
            string confirmPassword = form["confirmPassword"].ToString();
            string pass = GetMD5(password);
            var user = db.Users.SingleOrDefault(n => n.account == username && n.pass_word == pass);

            if (user == null)
            {
                ViewBag.Error = "User not existed or wrong password";
                return View();
            }

            if (username == null || password == null || newPassword == null || confirmPassword == null)
            {
                ViewBag.Error = "Fields are required";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Password not matched";
                return View();
            }

            user.pass_word = GetMD5(newPassword) ;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", "Store");
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection form)
        {
            string username = form["username"].ToString();
            string password = form["password"].ToString();
            string passwordMD5 = GetMD5(password);
            var user = db.Users.SingleOrDefault(n => n.account == username && n.pass_word == passwordMD5);
            if (user == null)
            {
                return Content("false");
            }
            Session["userId"] = user.id;
            Session["avatar"] = user.avatar;
            Session["fullname"] = user.fullname;
            Session["lever"] = user.lever;
            if (user.lever == 3)
                return Content("isShipper");
            return Content("");

        }
        public ActionResult logout()
        {
            Session.Clear();//remove session
            return Redirect("/");
        }
        public ActionResult logoutAdmin()
        {
            Session.Clear();//remove session
            return Redirect("/Home/Index");
        }
        public ActionResult UplaodImage(string image)
        {
            ViewBag.image = image;
            return View();
        }
        public ActionResult ForgotPassword(User user)
        {
            if (user.account == null)
                return View();
            if (db.Users.Any(x => x.account == user.account) == false)
            {
                ViewBag.Error = "There is no such account ";
                return View();
            }

            string randomString = RandomString(8);
            var user2 = db.Users.Where(x => x.account == user.account).FirstOrDefault();
            if (SendView(user2.mail, randomString) == false)
            {
                ViewBag.Error = "Problem while sending email.";
                return View();
            }

            user2.pass_word = GetMD5(randomString);
            db.Entry(user2).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Store");
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
        private bool SendView(string EMAIL, string newPassWord)
        {
            try
            {

                new MailHelper().SendMail(EMAIL, "Change Your password", "New you password " + newPassWord, "");
                return true;
            }
            catch (Exception)
            {
                return false;// "Problem while sending email, Please check details.";
            }
        }
        private bool SendViewConfirGmail(string EMAIL, string a)
        {
            ViewBag.Kq = EMAIL;
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
                WebMail.UserName = "tiensony1505@gmail.com";
                WebMail.Password = "scdldht1999";

                //Sender email address.  
                //WebMail.From = "tranhanam1999@gmail.com";

                //Send email  
                WebMail.Send(to: EMAIL, subject: "Xác nhận gmail", body: "Mã xác nhận: " + a, cc: null, bcc: null, isBodyHtml: true);
                return true; //"Email Sent Successfully.";
            }
            catch (Exception)
            {
                return false;// "Problem while sending email, Please check details.";
            }
        }

    }
}