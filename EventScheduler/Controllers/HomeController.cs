using EventScheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace EventScheduler.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Index(UsersModel model)
        {
            if (ModelState.IsValid)
            {
                using (MyDatabaseEntities db = new MyDatabaseEntities())
                {
                    var u = db.Users.Where(x => x.Email == model.Email && x.Password == model.Password).FirstOrDefault();

                    if (u != null)
                    {
                        System.Web.HttpContext.Current.Session["userID"] = u.userID;
                        System.Web.HttpContext.Current.Session["EmailAddress"] = u.Email;
                        //As user logged in this function is called
                        SendEmail(model);
                        return RedirectToAction("Calendar", "Home");
                    }
                    else
                    {
                        TempData["error"] = "Invalid email or password. Please try again!";
                    }
                }
            }
            return View(model);
        }

        public ActionResult Calendar()
        {
            int currentUser = (int)System.Web.HttpContext.Current.Session["userID"];
            ViewBag.user = currentUser;

            return View();
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            Session.Clear();

            return RedirectToAction("Index", "Home");
        }
        public JsonResult SignUp(string firstname, string lastname, string password, string emailaddress)
        {
            
            try
            {
                
                using (MyDatabaseEntities db = new MyDatabaseEntities())
                {
                    
                   // if (db.Users.Any(x => x.Email==emailaddress))
                   //{
                   //     TempData["EmailExist"] = "This email is already registered. Please try another one!";
                   //     ViewBag.Reg = "This email is already exists";
                       
                   // }
                    //else
                    //{
                        User user = new User();

                        user.FirstName = firstname;
                        user.LastName = lastname;
                        user.Password = password;
                        user.Email = emailaddress;
                        db.Users.Add(user);
                        db.SaveChanges();

                        //TempData["Registered"] = "You have been successfully registered. Please login now";

                    //}
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.InnerException);
            }
            return Json(JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetEvents()
        {
            int currentUser = int.Parse(System.Web.HttpContext.Current.Session["userID"].ToString());
           
            using (MyDatabaseEntities db = new MyDatabaseEntities())
            {
                var allevents = db.Events.ToList();
                // ViewBag.events = allevents;
                //var user = db.Events.SqlQuery("select Subject,Description,Start,End,ThemeColor,IsFullDay from Events").ToList();
                 var userEvents = db.Events.Where(x => x.userID == currentUser).ToList();
                //return Json(userEvents);
                return new JsonResult { Data = userEvents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
        [HttpPost]
        public JsonResult SaveEvent(Event e)
        {
            int currentUser = int.Parse(System.Web.HttpContext.Current.Session["userID"].ToString());
            //int currentUser = int.Parse(System.Web.HttpContext.Current.Session["userID"].ToString());
            var status = false;
            using (MyDatabaseEntities db = new MyDatabaseEntities())

            {

                if (e.EventID > 0)
                {

                    var v = db.Events.Where(x => x.EventID == e.EventID).FirstOrDefault();
                    //Update the Event
                    if (v != null)
                    {

                        v.Subject = e.Subject;
                        v.Start = e.Start;
                        v.End = e.End;
                        v.Description = e.Description;
                        v.IsFullDay = e.IsFullDay;
                        v.ThemeColor = e.ThemeColor;
                    }

                }
                else
                {
                    Event ev = new Event();
                    /////////////
                    e.EmailSent = 0;
                    e.userID = currentUser;
                    db.Events.Add(e);

                }
                db.SaveChanges();
                status = true;
            }

            return new JsonResult { Data = new { status = status } };
        }
        [HttpPost]
        public JsonResult DeleteEvent(int eventID)
        {
            var status = false;

            using (MyDatabaseEntities db = new MyDatabaseEntities())
            {
                var v = db.Events.Where(x => x.EventID == eventID).FirstOrDefault();

                if (v != null)
                {
                    db.Events.Remove(v);
                    db.SaveChanges();
                    status = true;
                }
            }

            return new JsonResult { Data = new { status = status } };
        }
        public void SendEmail(UsersModel obj)
        {

            //it hchecks for next day event and send email about all these events to logged in user
            DateTime tomorow = DateTime.Now.AddDays(1);
            DateTime today = DateTime.Now;
            using(MyDatabaseEntities db = new MyDatabaseEntities())
            {
                WebMail.SmtpServer = "smtp.gmail.com";
                //gmail port to send emails  
                WebMail.SmtpPort = 587;
                WebMail.SmtpUseDefaultCredentials = true;
                //sending emails with secure protocol  
                WebMail.EnableSsl = true;
                WebMail.UserName = "eventsscheduler1@gmail.com";
                WebMail.Password = "myevents1";
                WebMail.From = "eventsscheduler1@gmail.com";

                string emailaddress = System.Web.HttpContext.Current.Session["EmailAddress"].ToString();
                int currentUser = int.Parse(System.Web.HttpContext.Current.Session["userID"].ToString());
                var query = db.Users.Where(x => x.Email == emailaddress).FirstOrDefault();
                //////////////////////////////////////
                var events = db.Events.Where(x => x.userID==currentUser && x.Start> today && x.Start <tomorow && x.EmailSent==0).ToList();
                var counts = events.Count();
                if (query != null && events !=null)
                {
                    for (int i = 0; i < counts; i++)
                    {
                        obj.EMailBody = "Dear " + query.FirstName + " You tasks for tomorrow: \n " + events[i].Subject + " Starting Time: " + events[i].Start + "\n" + "Tasks Details: " + events[i].Description+"Please visit your calendar for further details. \r\n Thank You";
                        obj.EmailSubject = "Scheduled Tasks";
                        WebMail.Send(to: emailaddress, subject: obj.EmailSubject, body: obj.EMailBody, cc: obj.EmailCC, bcc: obj.EmailBCC, isBodyHtml: true);
                        events[i].EmailSent = 1;
                        db.SaveChanges();
                    }                 
                    TempData["Sent"] = "Email has been successfully sent!";
                }
            }
        }
        
    }

}