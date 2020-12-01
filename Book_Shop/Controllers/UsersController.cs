using Book_Shop.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web.Helpers;
using System.Web;
using System.Web.Mvc;
using Common;

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
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(FormCollection form)
        {
            string username = form["username"].ToString();
            string password = form["password"].ToString();
            string confirmPassword = form["confirmPassword"].ToString();
            string email = form["email"].ToString();
            string phone = form["phone"].ToString();
            string address = form["address"].ToString();
            string payment = form["payment"].ToString();
            string avatarPath = "";
            if (form["payment"].ToString() == null)
            {
                payment = "";
            }
            var avatar = Request.Files["Avatar"];

            var user = db.Users.SingleOrDefault(n => n.account == username);
            if (user != null)
            {
                ViewBag.Result = "User existed";
                return View();
            }
            if (password == confirmPassword)
            {
                ViewBag.Result = "Password not matched";
                return View();
            }
            if (avatar.FileName != "" && avatar.ContentLength > 0)
            {
                var path = Server.MapPath("~/UploadFiles/" + user.id + ".PNG");
                avatar.SaveAs(path);
                avatarPath = "/UploadFiles/" + user.id + ".PNG";
            }
            User newUser = new User() { account = username, pass_word = password, lever = 2, mail = email, phone = phone, avatar = avatarPath, payment = payment, shippingAddress = address };
            db.Users.Add(newUser);
            db.SaveChanges();
            return Redirect("/");
        }
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(FormCollection form)
        {
            string username = form["username"].ToString();
            string password = form["password"].ToString();
            string newPassword = form["newPassword"].ToString();
            string confirmPassword = form["confirmPassword"].ToString();
            var user = db.Users.SingleOrDefault(n => n.account == username && n.pass_word == password);

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

            user.pass_word = newPassword;
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
            var user = db.Users.SingleOrDefault(n => n.account == username && n.pass_word == password);

            if (user == null)
            {

                return Content("false");
            }

            Session["user"] = user;
            return Content("");

        }
        [HttpPost]
        public ActionResult logout()
        {
            Session.Clear();//remove session
            return RedirectToAction("Login");

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

            user2.pass_word = randomString;
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
                ////Configuring webMail class to send emails  
                ////gmail smtp server  
                //WebMail.SmtpServer = "smtp.gmail.com";
                ////gmail port to send emails  
                //WebMail.SmtpPort = 587;
                //WebMail.SmtpUseDefaultCredentials = true;
                ////sending emails with secure protocol  
                //WebMail.EnableSsl = true;
                ////EmailId used to send emails from application  
                //WebMail.UserName = "tranhanam1999hn@gmail.com";
                //WebMail.Password = "0898546564";

                ////Sender email address.  
                //WebMail.From = "tranhanam1999hn@gmail.com";
                ////Send email  ()
                new MailHelper().SendMail(EMAIL, "Change Your password", "New you password " + newPassWord, "");
                //WebMail.Send(to: EMAIL, subject: "Change Your password", body: "New you password " + newPassWord, cc: null, bcc: null, isBodyHtml: true);
                return true; //"Email Sent Successfully.";
            }
            catch (Exception)
            {
                return false;// "Problem while sending email, Please check details.";
            }
        }
    }
}