using Microsoft.AspNet.Identity;
using MRMV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace MRMV2.Controllers
{
    public class HomeController : Controller
    {


        private rdbEntities db = new rdbEntities();

        /// <summary>
        /// Index Method of the home controller calls the isAdmin method to check if a user is an admin or not.
        /// If a user is an admin they are redirected to the index of the Admin page
        /// If a user is not an admin they are directed to the landing page and shown a gallery of images
        /// </summary>
        /// <returns>Index View</returns>
        public ActionResult Index()
        {

            if (isAdmin())
                return RedirectToAction("Index", "Admin");

            LandingPageModel lp = new LandingPageModel();

            try
            {
                //Set the viewbag message to an empty string to ensure that any messages to be displayed on the landing page only display once
                ViewBag.Message = "";

                if (Session["messageExists"] != null)
                {
                    //If there is a message to display when redirected to the landing page
                    object messageExists = Session["messageExists"] ?? false;
                    //The message to display
                    string message = Session["message"] as string ?? "";



                    if ((Boolean)messageExists)
                    {
                        ViewBag.Message = message;
                    }
                }
                Session["section"] = "";


                //Get the top 7 viewed recipes on the website to display on the front page
                var getHotRecipesFromDB = (from rec in db.Recipes where rec.Recipe_Visibility == 1 orderby rec.Number_Of_Views descending select rec).Take(7).ToList();
                lp.getHotRecipes = getHotRecipesFromDB;

                //Get new Recipes
                var getNewRecipesFromDB = (from rec in db.Recipes where rec.Recipe_Visibility == 1 orderby rec.Date_Uploaded descending select rec).Take(10).ToList();
                lp.getNewRecipes = getNewRecipesFromDB;




                Session["messageExists"] = false;
                Session["message"] = "";


            }
            catch (Exception) { }
            return View(lp);



        }

        public ActionResult getNews()
        {
            //Get News
            var getNewsFromDB = (from news in db.News join user in db.AspNetUsers on news.UserID equals user.Id orderby news.UploadDate ascending select new { News = news, UserInfo = user }).Take(10).ToList();
            List<NewsModel> getNewsFromDBList = new List<NewsModel>();
            foreach (var item in getNewsFromDB)
            {
                getNewsFromDBList.Add(new NewsModel() { News1 = item.News.News1, UploadDate = item.News.UploadDate, UserID = item.News.UserID, UserName = item.UserInfo.UserName, UserPicturePath = "../../ProfilePicture/"+item.UserInfo.User_Picture_Path, Id = item.News.Id });
                }

            return Json(getNewsFromDBList, JsonRequestBehavior.AllowGet);
        }



        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Comments()
        {

            //Get News
            var getNewsFromDB = (from news in db.News join user in db.AspNetUsers on news.UserID equals user.Id orderby news.UploadDate ascending select new { News = news, UserInfo = user }).Take(10).ToList();
            List<NewsModel> getNewsFromDBList = new List<NewsModel>();
            foreach (var item in getNewsFromDB)
            {
                getNewsFromDBList.Add(new NewsModel() { News1 = item.News.News1, UploadDate = item.News.UploadDate, UserID = item.News.UserID, UserName = item.UserInfo.UserName, UserPicturePath = item.UserInfo.User_Picture_Path });
            }

            
            return Json(getNewsFromDBList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Check if the current user is an admin
        /// </summary>
        /// <returns>True if the current user is admin or false if the current user is not an admin</returns>
        [AllowAnonymous]
        public bool isAdmin()
        {

            string role = "";
            //Get the current users user id 
            string uid = User.Identity.GetUserId();
            //If the current user is not signed in the user id will be null
            if (uid != null)
            {
                //Get roll of currently logged in user
                var getRole = (from usr in db.AspNetUsers where usr.Id == uid select usr).First();
                role = getRole.Roles;

                if (role == "Admin")
                    return true;
            }
            return false;

        }



        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}