using MRMV2.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;

namespace MRMV2.Controllers
{
    public class UserController : Controller, IRequiresSessionState
    {


        private rdbEntities db = new rdbEntities();
        AccountController ac = new AccountController();

        /// <summary>
        /// Loads user profile
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns></returns>
        // GET: User
        public ActionResult Profile(String id = "")
        {
            Users u = new Users();
            string strID = "";
            if (id.Equals(""))
                strID = User.Identity.GetUserId() ?? "";
            else
                strID = id.ToString();



            var uid = User.Identity.GetUserId() ?? "";
            var getUsers = (from usr in db.AspNetUsers where usr.Id == strID select usr);
            var getLikedRecipes = (from usr in db.Liked_Recipes where usr.User_ID == strID select usr);
            IEnumerable<Recipe> getUploads = null;

            if (strID.Equals(uid) && uid != "")
                getUploads = (from upl in db.Recipes where upl.User_ID == strID select upl).ToList();
            else
                getUploads = (from upl in db.Recipes where upl.User_ID == strID && upl.Recipe_Visibility == 1 select upl).ToList();
            List<String> like = new List<String>();
            List<Recipe> upload = new List<Recipe>();
            foreach (var recInfo in getLikedRecipes)
                like.Add(recInfo.Recipe_ID);


            for (int o = 0; o < like.Count; o++)
            {
                string code = like[o];
                var likedRec = (from usr in db.Recipes where usr.Recipe_ID == code && usr.Recipe_Visibility == 1 select usr);
                foreach (var s in likedRec)
                {
                    Recipe r = new Recipe()
                    {
                        Recipe_ID = like[o],
                        Recipe_Short_Description = s.Recipe_Short_Description,
                        Display_Picture_Location = s.Display_Picture_Location,
                        Date_Uploaded = s.Date_Uploaded,
                        Recipe_Name = s.Recipe_Name,
                        Recipe_Description = s.Recipe_Description,
                        Cooking_Time = s.Cooking_Time,
                        Recipe_Visibility = s.Recipe_Visibility,
                        Number_Of_Views = s.Number_Of_Views,
                        Instructions = s.Instructions,
                        User_ID = s.User_ID

                    };
                    u.LikedRecipes.Add(r);

                }

            }

            //for (int o = 0; o < getUploads.Count; o++)
            //{
            //string code = upload[o];
            //var uploadedRec = (from usr in db.Recipes where usr.Recipe_ID == code && usr.Recipe_Visibility == 1 select usr);
            foreach (var s in getUploads)
            {
                Recipe r = new Recipe()
                {
                    Recipe_ID = s.Recipe_ID,
                    Recipe_Short_Description = s.Recipe_Short_Description,
                    Display_Picture_Location = s.Display_Picture_Location,
                    Date_Uploaded = s.Date_Uploaded,
                    Recipe_Name = s.Recipe_Name,
                    Recipe_Description = s.Recipe_Description,
                    Cooking_Time = s.Cooking_Time,
                    Recipe_Visibility = s.Recipe_Visibility,
                    Number_Of_Views = s.Number_Of_Views,
                    Instructions = s.Instructions,
                    User_ID = s.User_ID

                };
                u.UploadedRecipes.Add(r);

            }



            foreach (var usrInfo in getUsers)
            {

                u.userProfilePicture = usrInfo.User_Picture_Path;
                u.user_Description = usrInfo.User_Description;
                u.userName = usrInfo.UserName;
                u.userID = usrInfo.Id;

            }
            return View("Index", u);


        }



        /// <summary>
        /// Check if email provided on password reset is valid. If it is not fake questions will be givin to the user without there knowledge
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>Security questions real or fake</returns>
        public ActionResult isValidEmail(string email)
        {

            ForgotPasswordViewModel fp = new ForgotPasswordViewModel();
            Random rng = new Random();
            int randomNum;
            var user = (from u in db.AspNetUsers where u.Email == email select u).ToList();
            if (user != null && user.Count != 0)
            {
                List<string> questions = new List<string>();
                var sec = (from s in db.Security_Questions where s.AspNetUser.Email == email select s).ToList();

                randomNum = rng.Next(sec.Count);

                questions.Add(sec[randomNum].Security_Question);

                fp.Questions = questions;
                return Json(fp, JsonRequestBehavior.AllowGet);
            }
            List<string> fakeQuestions = new List<string>() { "First Pets Name","Mother's Maiden Name", "Father's Middle Name", "Mother's Middle Name", "First School","Favourite Park","Favorite Passtime","First Vehicle","Name of Highschool you attended","Grandmother's Middle Name",
                "Grandfather's Middle Name","Favourite Subject","Favourite Food","Favourite Color","Name of Childhood best friend","Favourite Animal", "City You Grew Up In","Favourite Teacher" };
            randomNum = rng.Next(fakeQuestions.Count);
            fp.Questions = new List<string> { fakeQuestions[randomNum] };
            return Json(fp, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Updates the users profile picture 
        /// </summary>
        /// <returns>Nothing</returns>
        //[Authorize(Roles = "User")]
        public JsonResult changeProfilePic()
        {
            try
            {
                var userID = User.Identity.GetUserId();
                var file = Request.Files[0];
                var updateUserProfilePicture = db.AspNetUsers.Where(s => s.Id == userID).First();
                db.AspNetUsers.Attach(updateUserProfilePicture);



                var entry = db.Entry(updateUserProfilePicture);
                if (file != null && file.FileName.Length >= 5)
                {
                    var ext = file.FileName.Substring(file.FileName.Length - 5);
                    if (ext.Contains(".bmp"))
                        ext = ".bmp";
                    else if (ext.Contains(".jpg"))
                        ext = ".jpg";
                    else if (ext.Contains(".jpeg"))
                        ext = ".jpeg";
                    else if (ext.Contains(".png"))
                        ext = ".png";

                    var path = "";
                    var loc = "";
                    var path0 = Path.Combine(Server.MapPath("../ProfilePicture/"), userID + ext);
                    var path1 = Path.Combine(Server.MapPath("../ProfilePicture/"), "1" + userID + ext);
                    if (System.IO.File.Exists(path0))
                    {
                        System.IO.File.Delete(path0);
                        path = Path.Combine(Server.MapPath("../ProfilePicture/"), "1" + userID + ext);
                        updateUserProfilePicture.User_Picture_Path = "1" + userID + ext;
                        loc = "../../../ProfilePicture/";

                    }
                    else if (System.IO.File.Exists(path1))
                    {
                        System.IO.File.Delete(path1);
                        path = Path.Combine(Server.MapPath("../ProfilePicture/"), userID + ext);
                        updateUserProfilePicture.User_Picture_Path = userID + ext;
                        loc = "../../../ProfilePicture/";
                    }
                    else
                    {
                        path = Path.Combine(Server.MapPath("../ProfilePicture/"), userID + ext);
                        updateUserProfilePicture.User_Picture_Path = userID + ext;
                        loc = "../../../ProfilePicture/";
                    }
                    file.SaveAs(path);

                    entry.Property(e => e.User_Picture_Path).IsModified = true;
                    // other changed properties
                    db.SaveChanges();
                    return Json(loc + updateUserProfilePicture.User_Picture_Path, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {

            }
            return Json("", JsonRequestBehavior.AllowGet);
        }






        ///// <summary>
        ///// Sets up the users options page
        ///// </summary>
        ///// <returns></returns>
        ////[Authorize(Roles = "User")]
        //public ActionResult Options()
        //{

        //    try
        //    {
        //        var uid = User.Identity.GetUserId();

        //        Session["mrmLogo"] = "";
        //        Session["infoLogo"] = "";
        //        Session["twitterLogo"] = "";
        //        Session["redditLogo"] = "";
        //        Session["facebookLogo"] = "";
        //        Session["gitLogo"] = "";
        //        Session["saveLogo"] = "";
        //        Session["garbageLogo"] = "";
        //        Session["arrowLogoLeft"] = "";
        //        Session["arrowLogoRight"] = "";
        //        Session["arrowLogoUp"] = "";
        //        Session["arrowLogoDown"] = "";
        //        Session["Logos"] = null;
        //        Session["themeID"] = "";
        //        Session["backgroundLocation"] = "";

        //        var userOptions = (from usr in db.UserOptions where usr.User_Id == uid select usr).ToList();
        //        if (userOptions.Count != 0)
        //        {


        //            //Get all user options and pass it to the view.
        //            //Get all availible themes and pass them to the view as well.
        //            var currentTheme = (from bck in db.ThemeGalleries where bck.User_Id == uid && bck.Permenant == 0 && bck.User_Custom_Theme == 0 select bck).ToList();
        //            var backTableCustom = (from bck in db.ThemeGalleries where bck.User_Id == uid && bck.Permenant == 0 && bck.User_Custom_Theme == 1 select bck).ToList();
        //            var backTable = (from bck in db.ThemeGalleries where bck.User_Id == null select bck);
        //            List<ThemeGallery> li = new List<ThemeGallery>();
        //            ThemeGallery currentThemeInfo;



        //            currentThemeInfo = (new ThemeGallery()
        //            {

        //                Background_Box_Color = currentTheme[0].Background_Box_Color,
        //                Background_Box_Opacity = currentTheme[0].Background_Box_Opacity,
        //                Background_Color_Opacity = currentTheme[0].Background_Color_Opacity,
        //                Background_Color = currentTheme[0].Background_Color,
        //                Background_Image = currentTheme[0].Background_Image,
        //                Background_Image_Color_Or_Theme = currentTheme[0].Background_Image_Color_Or_Theme,
        //                Input_Color = currentTheme[0].Input_Color,
        //                Input_Opacity = currentTheme[0].Input_Opacity,
        //                Logo_BitBucket = currentTheme[0].Logo_BitBucket,
        //                Logo_Facebook = currentTheme[0].Logo_Facebook,
        //                Logo_Info = currentTheme[0].Logo_Info,
        //                Logo_Reddit = currentTheme[0].Logo_Reddit,
        //                Logo_Twitter = currentTheme[0].Logo_Twitter,
        //                Logo_Main = currentTheme[0].Logo_Main,
        //                Logo_Save = currentTheme[0].Logo_Save,
        //                Hr_Color = currentTheme[0].Hr_Color,
        //                Logo_Garbage = currentTheme[0].Logo_Garbage,
        //                Arrow_Left = currentTheme[0].Arrow_Left,
        //                Arrow_Right = currentTheme[0].Arrow_Right,
        //                Arrow_Up = currentTheme[0].Arrow_Up,
        //                Arrow_Down = currentTheme[0].Arrow_Down,
        //                Name = currentTheme[0].Name,
        //                Text_Color = currentTheme[0].Text_Color,
        //                Theme_Icon_Path = currentTheme[0].Theme_Icon_Path,
        //                Theme_Id = currentTheme[0].Theme_Id,
        //                Contrast_Box_Background_Color = currentTheme[0].Contrast_Box_Background_Color,
        //                Contrast_Box_Background_Color_Opacity = currentTheme[0].Contrast_Box_Background_Color_Opacity,
        //                //themeType = bt.themeType,

        //            });








        //            foreach (var theme in backTableCustom)
        //                li.Add(new ThemeGallery()
        //                {

        //                    Background_Box_Color = theme.Background_Box_Color,
        //                    Background_Box_Opacity = theme.Background_Box_Opacity,
        //                    Background_Color_Opacity = theme.Background_Color_Opacity,
        //                    Background_Color = theme.Background_Color,
        //                    Background_Image = theme.Background_Image,
        //                    Background_Image_Color_Or_Theme = theme.Background_Image_Color_Or_Theme,
        //                    Input_Color = theme.Input_Color,
        //                    Input_Opacity = theme.Input_Opacity,
        //                    //Logo_Colors= bt.Logo_Colors,
        //                    Logo_BitBucket = theme.Logo_BitBucket,
        //                    Logo_Facebook = theme.Logo_Facebook,
        //                    Logo_Info = theme.Logo_Info,
        //                    Logo_Reddit = theme.Logo_Reddit,
        //                    Logo_Twitter = theme.Logo_Twitter,
        //                    Logo_Main = theme.Logo_Main,
        //                    Logo_Save = theme.Logo_Save,
        //                    Hr_Color = theme.Hr_Color,
        //                    Logo_Garbage = theme.Logo_Garbage,
        //                    Arrow_Left = theme.Arrow_Left,
        //                    Arrow_Right = theme.Arrow_Right,
        //                    Arrow_Up = theme.Arrow_Up,
        //                    Arrow_Down = theme.Arrow_Down,
        //                    Name = theme.Name,
        //                    Text_Color = theme.Text_Color,
        //                    Theme_Icon_Path = theme.Theme_Icon_Path,
        //                    Theme_Id = theme.Theme_Id,
        //                    Contrast_Box_Background_Color = theme.Contrast_Box_Background_Color,
        //                    Contrast_Box_Background_Color_Opacity = theme.Contrast_Box_Background_Color_Opacity,
        //                    User_Custom_Theme = theme.User_Custom_Theme,
        //                });





        //            foreach (var bt in backTable)
        //            {
        //                li.Add(new ThemeGallery()
        //                {

        //                    Background_Box_Color = bt.Background_Box_Color,
        //                    Background_Box_Opacity = bt.Background_Box_Opacity,
        //                    Background_Color_Opacity = bt.Background_Color_Opacity,
        //                    Background_Color = bt.Background_Color,
        //                    Background_Image = bt.Background_Image,
        //                    Background_Image_Color_Or_Theme = bt.Background_Image_Color_Or_Theme,
        //                    Input_Color = bt.Input_Color,
        //                    Input_Opacity = bt.Input_Opacity,
        //                    Logo_BitBucket = bt.Logo_BitBucket,
        //                    Logo_Facebook = bt.Logo_Facebook,
        //                    Logo_Info = bt.Logo_Info,
        //                    Logo_Reddit = bt.Logo_Reddit,
        //                    Logo_Twitter = bt.Logo_Twitter,
        //                    Logo_Main = bt.Logo_Main,
        //                    Logo_Save = bt.Logo_Save,
        //                    Hr_Color = bt.Hr_Color,
        //                    Logo_Garbage = bt.Logo_Garbage,
        //                    Arrow_Left = bt.Arrow_Left,
        //                    Arrow_Right = bt.Arrow_Right,
        //                    Arrow_Up = bt.Arrow_Up,
        //                    Arrow_Down = bt.Arrow_Down,
        //                    Name = bt.Name,
        //                    Text_Color = bt.Text_Color,
        //                    Theme_Icon_Path = bt.Theme_Icon_Path,
        //                    Theme_Id = bt.Theme_Id,
        //                    Contrast_Box_Background_Color = bt.Contrast_Box_Background_Color,
        //                    Contrast_Box_Background_Color_Opacity = bt.Contrast_Box_Background_Color_Opacity,
        //                    User_Custom_Theme = bt.User_Custom_Theme,
        //                    //themeType = bt.themeType,

        //                });
        //            }

        //            OptionsPage OptionsPage = new OptionsPage();
        //            Options uo = new Options() { NumberOfRecipesPerPage = userOptions[0].Recipes_Per_Page, showUpInSearch = userOptions[0].Search_Visibility, userTheme = currentThemeInfo };
        //            List<ThemeGallery> tlist = new List<ThemeGallery>();
        //            tlist = li;
        //            OptionsPage.opt = uo;
        //            OptionsPage.themeList = tlist;
        //            ThemeGallery g = new ThemeGallery();
        //            OptionsPage.galleryBack = g;
        //            return View(OptionsPage);
        //        }
        //    }
        //    catch (Exception) { }
        //    return RedirectToAction("Index", "Home");
        //}




        /// <summary>
        /// This is used to put any uploaded background images on the server
        /// </summary>
        /// <returns>Background location</returns>
        //[Authorize(Roles = "User")]
        public ActionResult uploadBackground()
        {
            String path = "";
            var uid = User.Identity.GetUserId();
            var file = Request.Files[0];
            var fname = uid + file.FileName;
            path = Path.Combine(Server.MapPath("../temp/"), fname);
            file.SaveAs(path);
            //Store the name of the uploaded image in a session variable
            Session["backgroundLocation"] = "../temp/" + fname;


            return Json(new { fname });
        }



        ///// <summary>
        ///// Set all icons chosen when chosing options
        ///// </summary>
        ///// <param name="logo">Logo class representing logos selected</param>
        //public void setIcons(logoClass logo)
        //{
        //    Session["Logos"] = logo;
        //}


        /// <summary>
        /// Theme chosen
        /// </summary>
        /// <param name="id">Id of theme chosen</param>
        public void uploadTheme(string id)
        {
            Session["themeID"] = id;
        }





        /// <summary>
        /// If any custom logos are uploaded this method is called and saves the logo on the server and sets the corresponding session variable to the custom location
        /// </summary>
        [HttpPost]
        public void uploadCustomIcons()
        {

            //Save the custom logo
            if (Request.Files.Count != 0)
            {
                var file = Request.Files[0];
                var rid = ac.returnRandomID();
                var fileName = Path.GetFileName(file.FileName);
                var userID = User.Identity.GetUserId();
                var path = Path.Combine(Server.MapPath("../temp/"), userID + rid + ".jpg");
                file.SaveAs(path);
                //Set the custom logo location to a session variable
                switch (Request.Files.Keys.Get(0))
                {
                    case "mrmLogo": Session["mrmLogo"] = path; break;
                    case "infoLogo": Session["infoLogo"] = path; break;
                    case "twitterLogo": Session["twitterLogo"] = path; break;
                    case "redditLogo": Session["redditLogo"] = path; break;
                    case "facebookLogo": Session["facebookLogo"] = path; break;
                    case "gitLogo": Session["gitLogo"] = path; break;
                    case "saveLogo": Session["saveLogo"] = path; break;
                    case "garbageLogo": Session["garbageLogo"] = path; break;
                    case "arrowLogoLeft": Session["arrowLogoLeft"] = path; break;
                    case "arrowLogoRight": Session["arrowLogoRight"] = path; break;
                    case "arrowLogoUp": Session["arrowLogoUp"] = path; break;
                    case "arrowLogoDown": Session["arrowLogoDown"] = path; break;
                }

            }
        }




        ///// <summary>
        ///// This method Saves the chosen or built theme
        ///// </summary>
        ///// <param name="op">options page view model</param>
        ///// <returns>Redirect to landing page view</returns>
        //public ActionResult ManualThemeSave(OptionsPage op)
        //{
        //    //If any logos were custom uploaded they will be in the corresponding session variable otherwise the strings are set to ""
        //    string mrmLogo = Session["mrmLogo"] as string ?? "";
        //    string infoLogo = Session["infoLogo"] as string ?? "";
        //    string twitterLogo = Session["twitterLogo"] as string ?? "";
        //    string redditLogo = Session["redditLogo"] as string ?? "";
        //    string facebookLogo = Session["facebookLogo"] as string ?? "";
        //    string gitLogo = Session["gitLogo"] as string ?? "";
        //    string saveLogo = Session["saveLogo"] as string ?? "";
        //    string garbageLogo = Session["garbageLogo"] as string ?? "";
        //    string arrowLogoLeft = Session["arrowLogoLeft"] as string ?? "";
        //    string arrowLogoRight = Session["arrowLogoRight"] as string ?? "";
        //    string arrowLogoUp = Session["arrowLogoUp"] as string ?? "";
        //    string arrowLogoDown = Session["arrowLogoDown"] as string ?? "";
        //    logoClass Logos = Session["Logos"] as logoClass;
        //    var id = User.Identity.GetUserId();
        //    var updateUserOptions = db.UserOptions.Where(s => s.User_Id == id).First();
        //    db.UserOptions.Attach(updateUserOptions);
        //    var profileExists = db.ThemeGalleries.Where(s => s.User_Id == id && s.Permenant == 0);
        //    while (profileExists.Count() == 0)
        //    {

        //        var baseTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "1" select usr).ToList();
        //        foreach (var bt in baseTheme)
        //        {
        //            ThemeGallery tg = new ThemeGallery()
        //            {
        //                User_Id = id,
        //                Theme_Id = ac.returnRandomID(),
        //                Background_Box_Color = bt.Background_Box_Color,
        //                Background_Box_Opacity = bt.Background_Box_Opacity,
        //                Background_Color_Opacity = bt.Background_Color_Opacity,
        //                Background_Color = bt.Background_Color,
        //                Background_Image = bt.Background_Image,
        //                Background_Image_Color_Or_Theme = bt.Background_Image_Color_Or_Theme,
        //                Input_Color = bt.Input_Color,
        //                Input_Opacity = bt.Input_Opacity,
        //                Logo_BitBucket = bt.Logo_BitBucket,
        //                Logo_Facebook = bt.Logo_Facebook,
        //                Logo_Info = bt.Logo_Info,
        //                Logo_Reddit = bt.Logo_Reddit,
        //                Logo_Twitter = bt.Logo_Twitter,
        //                Logo_Main = bt.Logo_Main,
        //                Logo_Save = bt.Logo_Save,
        //                Hr_Color = bt.Hr_Color,
        //                Logo_Garbage = bt.Logo_Garbage,
        //                Arrow_Left = bt.Arrow_Left,
        //                Arrow_Right = bt.Arrow_Right,
        //                Arrow_Up = bt.Arrow_Up,
        //                Arrow_Down = bt.Arrow_Down,
        //                Name = "",
        //                Text_Color = bt.Text_Color,
        //                Theme_Icon_Path = bt.Theme_Icon_Path,
        //                Contrast_Box_Background_Color = bt.Contrast_Box_Background_Color,
        //                Contrast_Box_Background_Color_Opacity = bt.Contrast_Box_Background_Color_Opacity,
        //            };
        //            var us = db.Set<ThemeGallery>();
        //            us.Add(tg);
        //        }

        //        db.SaveChanges();
        //        profileExists = db.ThemeGalleries.Where(s => s.User_Id == id && s.Permenant == 0);
        //    }



        //    updateUserOptions.Recipes_Per_Page = op.opt.NumberOfRecipesPerPage;
        //    updateUserOptions.Search_Visibility = op.opt.showUpInSearch;


        //    var updateTheme = db.ThemeGalleries.Where(s => s.User_Id == id && s.Permenant == 0 && s.User_Custom_Theme == 0).First();
        //    db.ThemeGalleries.Attach(updateTheme);




        //    var themeID = "";
        //    var themeChangeStatus = db.Entry(updateTheme);
        //    if (Session["themeID"] as string != null)
        //        themeID = Session["themeID"] as string;

        //    var backgroundLocation = Session["backgroundLocation"] as string ?? "";
        //    if (themeID == "")
        //    {

        //        //If any values have changed update them in the database
        //        if (op.galleryBack.Background_Color != null)
        //        {
        //            updateTheme.Background_Color = op.galleryBack.Background_Color;
        //            updateTheme.Background_Color_Opacity = op.galleryBack.Background_Color_Opacity;
        //        }
        //        else
        //        {
        //            themeChangeStatus.Property(e => e.Background_Color).IsModified = false;
        //            themeChangeStatus.Property(e => e.Background_Color_Opacity).IsModified = false;
        //        }





        //        if (backgroundLocation != "")
        //        {
        //            var fname = backgroundLocation.Substring(backgroundLocation.IndexOf("temp/") + 5, backgroundLocation.Length - backgroundLocation.IndexOf("temp/") - 5);
        //            var a = Server.MapPath(backgroundLocation);
        //            var b = Server.MapPath("~/Backgrounds/" + fname);

        //            var exists = true;
        //            while (exists)
        //            {
        //                try
        //                {
        //                    if (System.IO.File.Exists(b))
        //                        System.IO.File.Delete(b);
        //                    else
        //                    {
        //                        System.IO.File.Move(a, b);
        //                        exists = false;
        //                    }
        //                }
        //                catch (IOException)
        //                {

        //                }
        //            }
        //            updateTheme.Background_Image = "../../../Backgrounds/" + fname;
        //        }
        //        else
        //            themeChangeStatus.Property(e => e.Background_Image).IsModified = false;




        //        if (op.galleryBack.Text_Color == null)
        //            themeChangeStatus.Property(e => e.Text_Color).IsModified = false;
        //        else
        //        {
        //            updateTheme.Text_Color = op.galleryBack.Text_Color;

        //        }



        //        if (op.galleryBack.Background_Box_Color == null)
        //        {

        //            themeChangeStatus.Property(e => e.Background_Box_Color).IsModified = false;
        //            themeChangeStatus.Property(e => e.Background_Box_Opacity).IsModified = false;

        //        }
        //        else
        //        {
        //            updateTheme.Background_Box_Color = op.galleryBack.Background_Box_Color;
        //            updateTheme.Background_Box_Opacity = op.galleryBack.Background_Box_Opacity;
        //        }

        //        if (op.galleryBack.Contrast_Box_Background_Color == null)
        //        {

        //            themeChangeStatus.Property(e => e.Contrast_Box_Background_Color).IsModified = false;
        //            themeChangeStatus.Property(e => e.Contrast_Box_Background_Color_Opacity).IsModified = false;

        //        }
        //        else
        //        {
        //            updateTheme.Contrast_Box_Background_Color = op.galleryBack.Contrast_Box_Background_Color;
        //            updateTheme.Contrast_Box_Background_Color_Opacity = op.galleryBack.Contrast_Box_Background_Color_Opacity;
        //        }


        //        if (op.galleryBack.Input_Color == null)
        //        {
        //            themeChangeStatus.Property(e => e.Input_Color).IsModified = false;
        //            themeChangeStatus.Property(e => e.Input_Opacity).IsModified = false;
        //        }
        //        else
        //        {

        //            updateTheme.Input_Color = op.galleryBack.Input_Color;
        //            updateTheme.Input_Opacity = op.galleryBack.Input_Opacity;

        //        }

        //        if (op.galleryBack.Hr_Color == null)
        //        {
        //            themeChangeStatus.Property(e => e.Hr_Color).IsModified = false;
        //        }
        //        else
        //        {
        //            updateTheme.Hr_Color = op.galleryBack.Hr_Color;
        //        }



        //        #region Add Logos
        //        if (Logos != null)
        //        {

        //            //This is the same block for every logo uploaded :[
        //            //Checks to see if a logo was chosen for facebook on the options/theme page
        //            if (Logos.facebookLogo == null || updateTheme.Logo_Facebook == Logos.facebookLogo)
        //                themeChangeStatus.Property(e => e.Logo_Facebook).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                //If facebooklogo has been set to custom than the session variable facebooklogo has a custom location for an image
        //                if (Logos.facebookLogo.Equals("custom"))
        //                {
        //                    path = facebookLogo;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    //Move the image from temp to the actual directory
        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    //Set its new directory and update the database location
        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Logo_Facebook = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        path = Logos.facebookLogo;
        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);

        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Logo_Facebook = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }


        //                }
        //            }








        //            if (Logos.gitLogo == null || updateTheme.Logo_BitBucket == Logos.gitLogo)
        //                themeChangeStatus.Property(e => e.Logo_BitBucket).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.gitLogo.Equals("custom"))
        //                {
        //                    path = gitLogo;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Logo_BitBucket = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        path = Logos.gitLogo;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);


        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Logo_BitBucket = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }









        //            if (Logos.infoLogo == null || updateTheme.Logo_Info == Logos.infoLogo)
        //                themeChangeStatus.Property(e => e.Logo_Info).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.infoLogo.Equals("custom"))
        //                {
        //                    path = infoLogo;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Logo_Info = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {

        //                    try
        //                    {
        //                        path = Logos.infoLogo;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);

        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Logo_Info = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }










        //            if (Logos.mrmLogo == null || updateTheme.Logo_Main == Logos.mrmLogo)
        //                themeChangeStatus.Property(e => e.Logo_Main).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.mrmLogo.Equals("custom"))
        //                {
        //                    path = mrmLogo;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Logo_Main = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        path = Logos.mrmLogo;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);

        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Logo_Main = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }








        //            if (Logos.redditLogo == null || updateTheme.Logo_Reddit == Logos.redditLogo)
        //                themeChangeStatus.Property(e => e.Logo_Reddit).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.redditLogo.Equals("custom"))
        //                {
        //                    path = redditLogo;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Logo_Reddit = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {

        //                    try
        //                    {
        //                        path = Logos.redditLogo;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);


        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Logo_Reddit = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }



        //            if (Logos.saveLogo == null || updateTheme.Logo_Save == Logos.saveLogo)
        //                themeChangeStatus.Property(e => e.Logo_Save).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.saveLogo.Equals("custom"))
        //                {
        //                    path = saveLogo;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Logo_Save = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        path = Logos.saveLogo;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);


        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Logo_Save = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }







        //            if (Logos.garbageLogo == null || updateTheme.Logo_Garbage== Logos.garbageLogo)
        //                themeChangeStatus.Property(e => e.Logo_Garbage).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.garbageLogo.Equals("custom"))
        //                {
        //                    path = garbageLogo;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Logo_Garbage = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        path = Logos.garbageLogo;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);


        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Logo_Garbage = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }








        //            if (Logos.twitterLogo == null || updateTheme.Logo_Twitter == Logos.twitterLogo)
        //                themeChangeStatus.Property(e => e.Logo_Twitter).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.twitterLogo.Equals("custom"))
        //                {
        //                    path = twitterLogo;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Logo_Twitter = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        path = Logos.twitterLogo;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);


        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Logo_Twitter = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }








        //            if (Logos.arrowLogoDown == null || updateTheme.Arrow_Down == Logos.arrowLogoDown)
        //                themeChangeStatus.Property(e => e.Arrow_Down).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.arrowLogoDown.Equals("custom"))
        //                {
        //                    path = arrowLogoDown;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Arrow_Down = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {

        //                    try
        //                    {
        //                        path = Logos.arrowLogoDown;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);


        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Arrow_Down = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }









        //            if (Logos.arrowLogoLeft == null || updateTheme.Arrow_Left == Logos.arrowLogoLeft)
        //                themeChangeStatus.Property(e => e.Arrow_Left).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.arrowLogoLeft.Equals("custom"))
        //                {
        //                    path = arrowLogoLeft;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Arrow_Left = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        path = Logos.arrowLogoLeft;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);


        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Arrow_Left = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }








        //            if (Logos.arrowLogoRight == null || updateTheme.Arrow_Right == Logos.arrowLogoRight)
        //                themeChangeStatus.Property(e => e.Arrow_Right).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.arrowLogoRight.Equals("custom"))
        //                {
        //                    path = arrowLogoRight;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Arrow_Right = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {

        //                    try
        //                    {
        //                        path = Logos.arrowLogoRight;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);


        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Arrow_Right = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }









        //            if (Logos.arrowLogoUp == null || updateTheme.Arrow_Up == Logos.arrowLogoUp)
        //                themeChangeStatus.Property(e => e.Arrow_Up).IsModified = false;
        //            else
        //            {
        //                string path = "";
        //                if (Logos.arrowLogoUp.Equals("custom"))
        //                {
        //                    path = arrowLogoUp;
        //                    path = "~/" + path.Substring(path.IndexOf("temp"));

        //                    var a = Server.MapPath(path);
        //                    var b = Server.MapPath("~/Icons/" + path.Substring(path.IndexOf("temp\\") + 6));

        //                    var exists = true;
        //                    while (exists)
        //                    {
        //                        try
        //                        {
        //                            if (System.IO.File.Exists(b))
        //                                System.IO.File.Delete(b);
        //                            else
        //                            {
        //                                System.IO.File.Move(a, b);
        //                                exists = false;
        //                                updateTheme.Arrow_Up = "../../../Icons/" + path.Substring(path.IndexOf("temp\\") + 6);
        //                            }
        //                        }
        //                        catch (IOException)
        //                        {

        //                        }
        //                    }
        //                }
        //                else
        //                {

        //                    try
        //                    {
        //                        path = Logos.arrowLogoUp;

        //                        string originalPath = path;
        //                        path = path.Substring(path.IndexOf("Icons"));
        //                        path = "~/" + path;
        //                        var filePath = Server.MapPath(path);


        //                        if (System.IO.File.Exists(filePath))
        //                            updateTheme.Arrow_Up = originalPath;
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //            }







        //        }
        //        #endregion


                
        //        if ( op.radValueBox != null)
        //        {
        //            if (op.radValueBox == "image" && backgroundLocation != null && backgroundLocation != "")
        //                updateTheme.Background_Image_Color_Or_Theme = "image";
        //            else if (op.radValueBox == "color")
        //                updateTheme.Background_Image_Color_Or_Theme = "color";
        //        }





        //        if (!String.IsNullOrWhiteSpace(op.themeName))
        //        {
        //            ThemeGallery theme = new ThemeGallery();
        //            theme.Arrow_Down = updateTheme.Arrow_Down ?? "";
        //            theme.Arrow_Left = updateTheme.Arrow_Left ?? "";
        //            theme.Arrow_Right = updateTheme.Arrow_Right ?? "";
        //            theme.Arrow_Up = updateTheme.Arrow_Up ?? "";
        //            theme.Background_Box_Color = updateTheme.Background_Box_Color ?? "";
        //            theme.Background_Box_Opacity = updateTheme.Background_Box_Opacity ?? "";
        //            theme.Background_Color = updateTheme.Background_Color ?? "";
        //            theme.Background_Color_Opacity = updateTheme.Background_Color_Opacity ?? "";
        //            theme.Background_Image = updateTheme.Background_Image ?? "";
        //            theme.Background_Image_Color_Or_Theme = updateTheme.Background_Image_Color_Or_Theme ?? "";
        //            theme.Contrast_Box_Background_Color = updateTheme.Contrast_Box_Background_Color ?? "";
        //            theme.Contrast_Box_Background_Color_Opacity = updateTheme.Contrast_Box_Background_Color_Opacity ?? "";
        //            theme.Hr_Color = updateTheme.Hr_Color ?? "";
        //            theme.Input_Color = updateTheme.Input_Color ?? "";
        //            theme.Input_Opacity = updateTheme.Input_Opacity ?? "";
        //            theme.Logo_BitBucket = updateTheme.Logo_BitBucket ?? "";
        //            theme.Logo_Facebook = updateTheme.Logo_Facebook ?? "";
        //            theme.Logo_Garbage = updateTheme.Logo_Garbage ?? "";
        //            theme.Logo_Info = updateTheme.Logo_Info ?? "";
        //            theme.Logo_Main = updateTheme.Logo_Main ?? "";
        //            theme.Logo_Reddit = updateTheme.Logo_Reddit ?? "";
        //            theme.Logo_Save = updateTheme.Logo_Save ?? "";
        //            theme.Logo_Twitter = updateTheme.Logo_Twitter ?? "";
        //            theme.Name = op.themeName ?? "";
        //            theme.Permenant = 0;
        //            theme.Text_Color = updateTheme.Text_Color ?? "";
        //            theme.Theme_Icon_Path = updateTheme.Theme_Icon_Path ?? "";
        //            theme.Theme_Id = ac.returnRandomID();
        //            theme.User_Id = updateTheme.User_Id ?? "";
        //            theme.User_Custom_Theme = 1;
        //            var set = db.ThemeGalleries.Add(theme);

        //        }


        //        backgroundLocation = "";
        //        mrmLogo = "";
        //        infoLogo = "";
        //        twitterLogo = "";
        //        redditLogo = "";
        //        facebookLogo = "";
        //        gitLogo = "";
        //        saveLogo = "";
        //        arrowLogoLeft = "";
        //        arrowLogoRight = "";
        //        arrowLogoUp = "";
        //        arrowLogoDown = "";
        //    }
        //    else
        //    {

        //        var fromTheme = db.ThemeGalleries.Where(s => s.Theme_Id == themeID).ToList();
        //        updateTheme.Arrow_Down = fromTheme[0].Arrow_Down;
        //        updateTheme.Arrow_Left = fromTheme[0].Arrow_Left;
        //        updateTheme.Arrow_Right = fromTheme[0].Arrow_Right;
        //        updateTheme.Arrow_Up = fromTheme[0].Arrow_Up;
        //        updateTheme.Background_Box_Color = fromTheme[0].Background_Box_Color;
        //        updateTheme.Background_Box_Opacity = fromTheme[0].Background_Box_Opacity;
        //        updateTheme.Background_Color = fromTheme[0].Background_Color;
        //        updateTheme.Background_Color_Opacity = fromTheme[0].Background_Color_Opacity;
        //        updateTheme.Background_Image = fromTheme[0].Background_Image;
        //        updateTheme.Background_Image_Color_Or_Theme = fromTheme[0].Background_Image_Color_Or_Theme;
        //        updateTheme.Contrast_Box_Background_Color = fromTheme[0].Contrast_Box_Background_Color;
        //        updateTheme.Contrast_Box_Background_Color_Opacity = fromTheme[0].Contrast_Box_Background_Color_Opacity;
        //        updateTheme.Hr_Color = fromTheme[0].Hr_Color;
        //        updateTheme.Input_Color = fromTheme[0].Input_Color;
        //        updateTheme.Input_Opacity = fromTheme[0].Input_Opacity;
        //        updateTheme.Logo_BitBucket = fromTheme[0].Logo_BitBucket;
        //        updateTheme.Logo_Facebook = fromTheme[0].Logo_Facebook;
        //        updateTheme.Logo_Garbage = fromTheme[0].Logo_Garbage;
        //        updateTheme.Logo_Info = fromTheme[0].Logo_Info;
        //        updateTheme.Logo_Main = fromTheme[0].Logo_Main;
        //        updateTheme.Logo_Reddit = fromTheme[0].Logo_Reddit;
        //        updateTheme.Logo_Save = fromTheme[0].Logo_Save;
        //        updateTheme.Logo_Twitter = fromTheme[0].Logo_Twitter;
        //        updateTheme.Text_Color = fromTheme[0].Text_Color;
        //        themeID = "";

        //    }




        //    db.SaveChanges();
        //    Session["typeChosen"] = "";
        //    Session["themeID"] = "";
        //    Session["hexColorCode"] = "";
        //    Session["backgroundLocation"] = "";
        //    Session["mrmLogo"] = "";
        //    Session["infoLogo"] = "";
        //    Session["twitterLogo"] = "";
        //    Session["redditLogo"] = "";
        //    Session["facebookLogo"] = "";
        //    Session["gitLogo"] = "";
        //    Session["saveLogo"] = "";
        //    Session["garbageLogo"] = "";
        //    Session["arrowLogoLeft"] = "";
        //    Session["arrowLogoRight"] = "";
        //    Session["arrowLogoUp"] = "";
        //    Session["arrowLogoDown"] = "";




        //    return RedirectToAction("Index", "Home");
        //}

        /// <summary>
        /// Delete a custom theme
        /// </summary>
        /// <param name="id">Theme ID</param>
        /// <returns>true if success, false of failure</returns>
        public bool deleteTheme(String id)
        {

            var the = (from c in db.ThemeGalleries where c.Theme_Id == id select c).First();
            if (the != null)
            {
                if (the.User_Id == User.Identity.GetUserId())
                    db.ThemeGalleries.Remove(the);
                db.SaveChanges();

                return true;
            }
            return false;

        }


        /// <summary>
        /// Get signed in users background
        /// </summary>
        /// <returns>Background location</returns>
        public string getBackground()
        {
            string str = "";
            var uid = User.Identity.GetUserId() + "";
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();
            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();


            if (getTheme[0].Background_Image_Color_Or_Theme.Equals("image"))
            {
                str += getTheme[0].Background_Image;
            }
            else if (getTheme[0].Background_Image_Color_Or_Theme.Equals("color"))
            {
                str += getTheme[0].Background_Color + "!!!###";
                str += getTheme[0].Background_Color_Opacity;
            }


            else
                str += "../../../Backgrounds/stockBackground.jpg";
            return str;
        }


        /// <summary>
        /// Get signed in users color for active boxes
        /// </summary>
        /// <returns>Color for active boxes</returns>
        public string activeColor()
        {

            var uid = User.Identity.GetUserId() + "";
            string str = "";
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();
            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();

            str += getTheme[0].Background_Box_Color + "!!!###";
            str += getTheme[0].Background_Box_Opacity;

            return str;
        }




        /// <summary>
        /// Get signed in users text color
        /// </summary>
        /// <returns>Text Color</returns>
        public string getTextColor()
        {
            var uid = User.Identity.GetUserId();
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();
            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();
            return getTheme[0].Text_Color;

        }


        /// <summary>
        /// Get signed in users Hr Line colors
        /// </summary>
        /// <returns>Line Color</returns>
        public string getHrColor()
        {
            var uid = User.Identity.GetUserId();
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();
            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();
            return getTheme[0].Hr_Color;

        }

        /// <summary>
        /// Get signed in users input color
        /// </summary>
        /// <returns>Color of users input</returns>
        public string getInputColor()
        {
            string str = "";
            var uid = User.Identity.GetUserId() + "";
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();

            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();


            str += getTheme[0].Input_Color + "!!!###";
            str += getTheme[0].Input_Opacity;


            return str;
        }

        /// <summary>
        /// Get signed in users contrast color
        /// </summary>
        /// <returns>users Contrast color</returns>
        public string getContrastColor()
        {
            string str = "";
            var uid = User.Identity.GetUserId() + "";
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();

            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();


            str += getTheme[0].Contrast_Box_Background_Color + "!!!###" + getTheme[0].Contrast_Box_Background_Color_Opacity;


            return str;
        }


        /// <summary>
        /// Get signed in users landing page logos
        /// </summary>
        /// <returns>logos location</returns>
        public string getIcons()
        {
            string str = "";
            var uid = User.Identity.GetUserId() + "";
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();

            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();


            str += getTheme[0].Logo_BitBucket + "!!!###";
            str += getTheme[0].Logo_Facebook + "!!!###";
            str += getTheme[0].Logo_Reddit + "!!!###";
            str += getTheme[0].Logo_Twitter;


            return str;
        }


        /// <summary>
        /// Get a random recipe for search page
        /// </summary>
        /// <returns>Random recipe</returns>
        public string getRandomRow()
        {
            string str = "";

            var getRecipes = (from getRandomRecipe in db.Recipes where getRandomRecipe.Recipe_Visibility == 1 select getRandomRecipe).ToList();
            Random r = new Random();
            str = getRecipes[r.Next(0, getRecipes.Count)].Recipe_ID;
            return str;
        }

        /// <summary>
        /// Get signed in users Myrecipemanager image
        /// </summary>
        /// <returns>mrm logo location</returns>
        public string getLogo()
        {
            string str = "";
            var uid = User.Identity.GetUserId() + "";
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();

            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();



            str += getTheme[0].Logo_Main;


            return str;
        }


        /// <summary>
        /// Get signed in users tip icon
        /// </summary>
        /// <returns>tip image location</returns>
        public string getTip()
        {
            string str = "";
            var uid = User.Identity.GetUserId() + "";
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();

            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();

            str += getTheme[0].Logo_Info;
            return str;
        }


        /// <summary>
        /// Get signed in users save icon
        /// </summary>
        /// <returns>save icon location</returns>
        public string getSave()
        {
            string str = "";
            var uid = User.Identity.GetUserId() + "";
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();

            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();



            str += getTheme[0].Logo_Save;


            return str;
        }


        /// <summary>
        /// Get signed in users trash icon
        /// </summary>
        /// <returns>trash icon location</returns>
        public string getTrash()
        {
            string str = "";
            var uid = User.Identity.GetUserId() + "";
            var getTheme = (from usr in db.ThemeGalleries where usr.User_Id == uid && usr.Permenant == 0 && usr.User_Custom_Theme == 0 select usr).ToList();

            if (getTheme.Count == 0)
                getTheme = (from usr in db.ThemeGalleries where usr.Theme_Id == "0" select usr).ToList();



            str += getTheme[0].Logo_Garbage;


            return str;
        }


        /// <summary>
        /// Get signed in users profile image
        /// </summary>
        /// <returns>Profile image location</returns>
        public string getImg()
        {

            string str = "";
            var uid = User.Identity.GetUserId() + "";
            var geticon = (from usr in db.AspNetUsers where usr.Id == uid select usr).ToList();

            if (geticon.Count == 0)
                str = "defaultProfile.png";
            else
                str += geticon[0].User_Picture_Path;
            return str;
        }

    }
}