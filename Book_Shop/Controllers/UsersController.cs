using Book_Shop.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web.Helpers;
using System.Web.Mvc;

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
        public ActionResult Register(User formValues)
        {
            var avatar = Request.Files["Avatar"];
            var user = db.Users.SingleOrDefault(n => n.account == formValues.account);
            if (user != null)
            {
                ViewBag.Result = "User existed";
                return View();
            }
            try
            {
                if (avatar.FileName != "" && avatar.ContentLength > 0)
                {
                    var path = Server.MapPath("~/UploadFiles/" + formValues.account + ".PNG");
                    avatar.SaveAs(path);
                    formValues.avatar = "/UploadFiles/" + formValues.account + ".PNG";
                }
                db.Users.Add(formValues);
                db.SaveChanges();
                return Redirect("/");
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var errors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in errors.ValidationErrors)
                    {
                        // get the error message 
                        string errorMessage = validationError.ErrorMessage;
                        ViewBag.Result = errorMessage;
                    }
                }
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
            ViewBag.Result = "Change password successfully";
            return View();
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
            var us = db.Users.SingleOrDefault(n => n.account == username && n.pass_word == password);

            if (us == null)
            {
                return Content("false");
            }

            Session["user"] = us;
            return Content("/Home/Index");

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
            ViewBag.Result = "Email Sent Successfully.";

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
        private bool SendView(string EMAIL, string a)
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
                WebMail.Send(to: EMAIL, subject: "Change Your password", body: "New you password " + a, cc: null, bcc: null, isBodyHtml: true);
                return true; //"Email Sent Successfully.";
            }
            catch (Exception)
            {
                return false;// "Problem while sending email, Please check details.";
            }
        }
    }
}