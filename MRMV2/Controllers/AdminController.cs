using MRMV2.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace MRMV2.Controllers
{
    public class AdminController : Controller
    {
        private rdbEntities db = new rdbEntities();
        


        /// <summary>
        /// Admin index page, loads the admin landing page
        /// </summary>
        /// <returns></returns>
        // GET: Admin
        public ActionResult Index()
        {
            try { 
            if (!isAdmin())
                return RedirectToAction("Index", "Home");
            AdminPageModel ap = new AdminPageModel();
            var getReports = (from rep in db.Reports select rep).Take(15);
            List<Report> comRepList = new List<Report>();
            List<Report> recRepList = new List<Report>();
            foreach (var reps in getReports)
            {

                if (reps.Type == "Comment")
                    comRepList.Add(new Report() { Comment_Or_Recipe_Id = reps.Comment_Or_Recipe_Id, Reason_For_Report = reps.Reason_For_Report.Substring(0, reps.Reason_For_Report.Length - 2), Report_Id = reps.Report_Id, Type = reps.Type, Content_Creator_Id = reps.Content_Creator_Id, Content_Creator_Name = reps.Content_Creator_Name, Reporter_Id = reps.Reporter_Id, Reporter_Name = reps.Reporter_Name, Comment = reps.Comment, Recipe_ID = reps.Recipe_ID, Comment_ID = reps.Comment_ID });
                else
                    recRepList.Add(new Report() { Comment_Or_Recipe_Id = reps.Comment_Or_Recipe_Id, Reason_For_Report = reps.Reason_For_Report.Substring(0, reps.Reason_For_Report.Length - 2), Report_Id = reps.Report_Id, Type = reps.Type, Content_Creator_Id = reps.Content_Creator_Id, Content_Creator_Name = reps.Content_Creator_Name, Reporter_Id = reps.Reporter_Id, Reporter_Name = reps.Reporter_Name, Comment = reps.Comment, Recipe_Name = reps.Recipe_Name, Recipe_ID = reps.Recipe_ID });
                
            }

            ap.comReports = comRepList;
            ap.recReports = recRepList;
            ViewBag.Message = "User has been permenently banned";
            return View(ap);
            }
            catch (Exception) { return RedirectToAction("Index", "Home");}
        }


        /// <summary>
        /// Remove a recipe or comment. Admin only
        /// </summary>
        /// <param name="commentID">Id of the comment to remove</param>
        /// <param name="recipeID">Id of the recipe to remove</param>
        /// <param name="reportID">Id of the report</param>
        /// <param name="type">Comment or Recipe removal</param>
        /// <returns></returns>
        public ActionResult Remove(string commentID, string recipeID, string reportID, string type)
        {
            try { 
            if (!isAdmin())
                return RedirectToAction("Index", "Home");
            else
            {
                if (!String.IsNullOrEmpty(type))
                    if (type == "Comment" && !String.IsNullOrEmpty(reportID) && !String.IsNullOrEmpty(commentID))
                    {

                        var comm = (from c in db.Comments where c.Comment_ID == commentID select c).First();
                        db.Comments.Remove(comm);

                        var report = (from rep in db.Reports where rep.Comment_ID == commentID select rep);
                        foreach (var r in report)
                            db.Reports.Remove(r);

                    }
                    else if (type == "Recipe" && !String.IsNullOrEmpty(recipeID))
                    {
                        var ing = (from c in db.Ingredients where c.Recipe_ID == recipeID select c);
                        foreach (var q in ing)
                            db.Ingredients.Remove(q);


                        var comm = (from c in db.Comments where c.Recipe_ID == recipeID select c);
                        foreach (var c in comm)
                            db.Comments.Remove(c);





                        var tags = (from c in db.User_Defined_Tags where c.Recipe_ID == recipeID select c);
                        foreach (var q in tags)
                            db.User_Defined_Tags.Remove(q);

                        var appl = (from c in db.Appliances_Used where c.Recipe_Id == recipeID select c);
                        foreach (var q in appl)
                            db.Appliances_Used.Remove(q);

                        var pics = (from c in db.Pictures where c.Recipe_ID == recipeID select c);
                        foreach (var q in pics)
                        {
                            string path = q.Picture_Location;
                            path = path.Substring(5);
                            var filePath = Server.MapPath(path);
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                            db.Pictures.Remove(q);
                        }

                        var likedRec = (from lik in db.Liked_Recipes where lik.Recipe_ID == recipeID select lik);
                        foreach (var like in likedRec)
                            db.Liked_Recipes.Remove(like);

                        var recipe = (from rec in db.Recipes where rec.Recipe_ID == recipeID select rec).First();
                        db.Recipes.Remove(recipe);

                        var report = (from rep in db.Reports where rep.Recipe_ID == recipeID select rep);
                        foreach (var r in report)
                            db.Reports.Remove(r);


                    }
            }
            db.SaveChanges();
            }
            catch (Exception) { }
            return null;

        }

        /// <summary>
        /// Check if user is admin
        /// </summary>
        /// <returns>True if admin, false if not</returns>
        [AllowAnonymous]
        public bool isAdmin()
        {
            string role = "";
            var getRole = from usr in db.AspNetUsers where usr.UserName == User.Identity.Name select new { usr };
            if (getRole != null)
                foreach (var i in getRole)
                {
                    role = i.usr.Roles;
                    
                }
            if (role == "Admin")
                return true;
            return false;

        }



        

        /// <summary>
        /// Ban a user. Admin only
        /// </summary>
        /// <param name="id">Id of the user to ban</param>
        /// <returns>Json success message</returns>
        public ActionResult banUser(string id)
        {
            string message = "";
            try { 
            
            if (!isAdmin())
                return RedirectToAction("Index", "Home");
            else
            if (!String.IsNullOrEmpty(id))
            {
                var user = (from usr in db.AspNetUsers where usr.Id == id select usr).First();
                user.Ban_Count += 1;
                int bans = user.Ban_Count;
                if (bans >= 3)
                {
                    user.Banned = "t";
                    message = "User has been permenently banned";
                }
                else
                {
                    var date = DateTime.UtcNow.Date;
                    date = date.AddHours(48 * bans);
                    user.LockoutEndDateUtc = date;
                    message = "User has been temporarily banned";
                }

                db.SaveChanges();

            }
                //return null;
            }
            catch (Exception) { message = "An error has occured"; }
            return Json(new { success = message }, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// Remove the report. Admin only
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult removeReport(string id)
        {

            if (!isAdmin())
                return RedirectToAction("Index", "Home");
            else
            if (!String.IsNullOrEmpty(id))
            {
                var report = (from rep in db.Reports where rep.Report_Id == id select rep).First();
                db.Reports.Remove(report);
                db.SaveChanges();

            }
            return null;

        }


    }
}