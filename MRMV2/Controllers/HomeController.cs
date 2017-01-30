using Microsoft.AspNet.Identity;
using MRMV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                lp.getNewRecipes= getNewRecipesFromDB;
                Session["messageExists"] = false;
                Session["message"] = "";
            }
            catch (Exception) { }
            return View(lp);



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