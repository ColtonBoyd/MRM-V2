using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using Microsoft.AspNet.Identity;
using System.Drawing;
using Novacode;
using System.Web.UI.WebControls;
using MRMV2.Models;
using System.Collections;

namespace MRMV2.Controllers
{
    public class RecipesController : Controller
    {

        private rdbEntities db = new rdbEntities();
        AccountController ac = new AccountController();



        /// <summary>
        /// This method lets a user edit their recipe after creation. 
        /// It finds the recipe of the id passed in and passes all relevant information of that recipe to the view to auto fill fields with the original values provided to the recipe
        /// </summary>
        /// <param name="id">The id of the recipe to edit</param>
        /// <returns>Information about this recipe and the EditRecipe View</returns>
        public ActionResult EditRecipe(string id)
        {
            if (id == null)
                return RedirectToAction("Index", "Home");

            try
            {
                Session["pictureName"] = null;
                //Get Recipe information from database
                var rec = (from r in db.Recipes where r.Recipe_ID == id select r).ToList();
                var appliances = (from a in db.Appliances_Used where a.Recipe_Id == id select a).ToList();
                var photos = (from p in db.Pictures where p.Recipe_ID == id select p).ToList();
                var ingr = (from i in db.Ingredients where i.Recipe_ID == id select i).ToList();
                var tags = (from t in db.User_Defined_Tags where t.Recipe_ID == id select t).ToList();

                //If the recipe is found
                if (rec != null)
                    if (rec.Count > 0)
                    {
                        //If its a different user trying to edit someone elses recipe redirect to home
                        if (rec[0].User_ID != User.Identity.GetUserId())
                            return RedirectToAction("Index", "Home");
                        Session["editID"] = id;

                        //Create a new recipeinfo object to pass back to the view which will hold pointers to the 4 objects beneath it
                        EditPageModel recipeInfo = new EditPageModel();
                        List<Appliances_Used> applianceInfo = new List<Appliances_Used>();
                        List<Picture> photoInfo = new List<Picture>();
                        List<User_Defined_Tags> userTags = new List<User_Defined_Tags>();
                        List<Ingredients> ingredInfo = new List<Ingredients>();

                        //If there are photos for the recipe add them to an object
                        if (photos != null)
                        {
                            foreach (var photo in photos)
                                photoInfo.Add(new Picture() { Picture_Location = photo.Picture_Location, Picture_ID = photo.Picture_ID });

                            recipeInfo.getImages = photoInfo;
                        }


                        //If there are appliances for the recipe add them to an object
                        if (appliances != null)
                        {
                            foreach (var appl in appliances)
                                applianceInfo.Add(new Appliances_Used() { Appliance_Name = appl.Appliance_Name, Id = appl.Id });
                            recipeInfo.getAppliances = applianceInfo;
                        }


                        //If there are tags for the recipe add them to an object
                        if (tags != null)
                        {

                            foreach (var tag in tags)
                            {
                                userTags.Add(new User_Defined_Tags() { User_Defined_Tag = tag.User_Defined_Tag, Tag_ID = tag.Tag_ID });
                            }
                            recipeInfo.getTags = userTags;

                        }

                        //If there are ingredients for the recipe add them to an object
                        if (ingr != null)
                        {
                            foreach (var ing in ingr)
                                ingredInfo.Add(new Ingredients() { ingredientName = ing.Ingredient_Name, measurement = ing.Measurement, ingredientID = ing.Recipe_Ingredient_ID });
                            recipeInfo.getIngredients = ingredInfo;
                        }



                        //Add information to the recipe object
                        var a = rec[0].Cooking_Time;
                        recipeInfo.getRecipe.Cooking_Time = rec[0].Cooking_Time;
                        recipeInfo.Instructions = rec[0].Instructions;
                        recipeInfo.Description = rec[0].Recipe_Description;
                        recipeInfo.getRecipe.Recipe_Name = rec[0].Recipe_Name;
                        recipeInfo.getRecipe.Recipe_Visibility = rec[0].Recipe_Visibility;
                        recipeInfo.getRecipe.Recipe_ID = id;
                        Session["UploadingRecipeID"] = id;
                        recipeInfo.recipeVisibility = rec[0].Recipe_Visibility;

                        //Pass all of the recipe information to the view
                        return View(recipeInfo);

                    }
            }
            catch (Exception) { }
            return View("Error");
        }


        /// <summary>
        /// Edit Recipe method takes the model from the editRecipe view and makes changes to a recipe
        /// </summary>
        /// <param name="model">Recipe Information from editRecipe view</param>
        /// <returns>Landing page view</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult EditRecipe(EditPageModel model)
        {

            var rid = Session["editID"] as string;
            //If there is no recipe id return to the landing page
            if (rid == null)
                return RedirectToAction("Index", "Home");
            var uid = User.Identity.GetUserId() ?? "";
            //Get recipe to edit.
            var rec = (from r in db.Recipes where r.Recipe_ID == rid && r.User_ID == uid select r).ToList();
            if (rec != null)
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                try
                {
                    //Set the recipe to edit
                    var editRecipe = rec.First();

                    //Edit details
                    editRecipe.Recipe_Name = model.recipeName;
                    editRecipe.Recipe_Search_Name = model.recipeName.Replace(" ", "_");
                    editRecipe.Recipe_Description = model.Description;
                    if (model.Description.Length > 200)
                        editRecipe.Recipe_Short_Description = model.Description.Substring(0, 200) + "...";
                    else
                        editRecipe.Recipe_Short_Description = model.Description;
                    if (model.recipeVisibility == 1 || model.recipeVisibility == 0)
                        editRecipe.Recipe_Visibility = model.recipeVisibility;
                    else
                        editRecipe.Recipe_Visibility = 0;

                    editRecipe.Instructions = model.Instructions;


                    editRecipe.Cooking_Time = model.time;
                    editRecipe.Cooking_Time_Search_Name = model.time.Replace(" ", "_");




                    var uploadedRecipeID = (string)Session["UploadingRecipeID"];
                    List<string> pictureName = new List<string>();
                    pictureName = (List<string>)Session["pictureName"];

                    while (uploadedRecipeID == "")
                        uploadedRecipeID = Session["UploadingRecipeID"] as string;

                    //Add any new pictures
                    if (pictureName != null)
                    {
                        if (pictureName.Count != 0)
                            for (int i = 0; i < pictureName.Count; i++)
                            {
                                try
                                {
                                    var a = Server.MapPath("~/temp/" + pictureName[i]);
                                    var b = Server.MapPath("~/RecipeImages/" + pictureName[i]);
                                    System.IO.File.Move(a, b);
                                }
                                catch (IOException)
                                {

                                }
                            }

                        if (pictureName.Count != 0)
                            editRecipe.Display_Picture_Location = "../../RecipeImages/" + pictureName[0];

                        if (pictureName.Count != 0)
                        {

                            for (int i = 0; i < pictureName.Count; i++)
                            {
                                editRecipe.Pictures.Add(new Picture() { Picture_Location = "../../RecipeImages/" + pictureName[i], Recipe_ID = uploadedRecipeID, Picture_ID = uploadedRecipeID + returnRandomID() });
                            }
                        }

                    }

                    //Remove old tags
                    //Add new tags
                    var tagsList = (List<string>)Session["tagsList"];

                    if (tagsList != null)
                    {

                        var tags = (from c in db.User_Defined_Tags where c.Recipe_ID == rid select c);
                        foreach (var q in tags)
                            db.User_Defined_Tags.Remove(q);

                        for (int i = 0; i < tagsList.Count; i++)
                        {

                            if (tagsList[i] != "")
                            {
                                User_Defined_Tags ud = new User_Defined_Tags() { Recipe_ID = rec[0].Recipe_ID, Tag_ID = returnRandomID(), User_Defined_Tag = tagsList[i], User_Defined_Tag_Search = tagsList[i].Replace(" ", "_") };
                                var udfdb = db.Set<User_Defined_Tags>();
                                udfdb.Add(ud);
                            }
                        }



                    }


                    //Remove old appliances
                    //Add new appliances
                    var appliancesList = (List<string>)Session["appliancesList"];
                    if (appliancesList != null)
                    {
                        var appliance = (from c in db.Appliances_Used where c.Recipe_Id == rid select c);
                        foreach (var q in appliance)
                            db.Appliances_Used.Remove(q);
                        for (int i = 0; i < appliancesList.Count; i++)
                        {


                            if (appliancesList[i] != "")
                            {
                                Appliances_Used au = new Appliances_Used() { Id = returnRandomID(), Appliance_Name = appliancesList[i], Appliance_Name_Search = appliancesList[i].Replace(" ", "_"), Recipe_Id = rid };
                                var apudb = db.Set<Appliances_Used>();
                                apudb.Add(au);
                            }

                        }
                    }

                    //Remove old measurements
                    //Add new measurements
                    var measurements = (from c in db.Ingredients where c.Recipe_ID == rid select c);
                    foreach (var q in measurements)
                        db.Ingredients.Remove(q);

                    for (int i = 0; i < model.Measurement.Count; i++)
                    {
                        Ingredient ing = new Ingredient() { Measurement = model.Measurement[i], Ingredient_Name = model.Ingredient_Name[i], Ingredient_Name_Search = model.Ingredient_Name[i].Replace(" ", "_"), Recipe_ID = rec[0].Recipe_ID, Recipe_Ingredient_ID = returnRandomID() };
                        var ingdb = db.Set<Ingredient>();
                        ingdb.Add(ing);
                    }
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return RedirectToAction("Search", "Home");
                }

            }

            return RedirectToAction("Index", "Home");
        }




        
        /// <summary>
        /// Return a random string to be used as an id
        /// </summary>
        /// <returns>Random string</returns>
        public string returnRandomID()
        {
            Random rng = new Random();
            String randomString = "";
            ArrayList ran = new ArrayList() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l",
                  "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
                  "O", "P", "Q", "R", "S", "T","U","V","W","X","Y","Z"};

            for (var i = 0; i < 50; i++)
                randomString += ran[rng.Next(0, ran.Count)];
            return randomString;
        }




        /// <summary>
        /// This method displays the recipe page.
        /// </summary>
        /// <param name="id">The id of the recipe to be loaded</param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult RecipeID(string id)
        {

            //If there is no id return back to the landing page
            if (id == null)
                return RedirectToAction("Index", "Home");
            try
            {
                string recID = id;
                //Store the recipe id in a session variable so when like or report or anything that needs the currently viewed recipe id can have it
                Session["recipeID"] = recID;

                RecipePageModel rp = new RecipePageModel();
                Recipe rid = new Recipe();
                Users Uploader = new Users();
                List<Comments> com = new List<Comments>();
                List<Picture> img = new List<Picture>();
                List<Ingredients> ing = new List<Ingredients>();
                List<Appliances_Used> appl = new List<Appliances_Used>();
                List<User_Defined_Tags> udl = new List<User_Defined_Tags>();



                //Get all tags appliances and information about the recipe
                var getRecipess = (from rec in db.Recipes where rec.Recipe_ID == recID select rec);
                var getAppliances = (from app in db.Appliances_Used where app.Recipe_Id == recID select app);
                var getTags = (from tag in db.User_Defined_Tags where tag.Recipe_ID == recID select tag);

                //Set appliance objects
                foreach (var appliance in getAppliances)
                {
                    appl.Add(new Appliances_Used() { Recipe_Id = recID, Appliance_Name = appliance.Appliance_Name, Id = appliance.Id });
                }

                //Set Tag objects
                foreach (var tag in getTags)
                {
                    udl.Add(new User_Defined_Tags() { Recipe_ID = recID, Tag_ID = tag.Tag_ID, User_Defined_Tag = tag.User_Defined_Tag, User_Defined_Tag_Search = tag.User_Defined_Tag_Search });
                }

                var uploaderID = "";
                //Set Recipe object
                foreach (var recipeInfo in getRecipess)
                {
                    rid.Recipe_Name = recipeInfo.Recipe_Name;
                    rid.Recipe_Description = recipeInfo.Recipe_Description;
                    rid.Cooking_Time = recipeInfo.Cooking_Time;
                    rid.Date_Uploaded = recipeInfo.Date_Uploaded;
                    rid.Recipe_ID = recID;
                    rid.Instructions = recipeInfo.Instructions;
                    uploaderID = recipeInfo.User_ID;
                }

                //If for whatever reason the recipe name is returned null return to the landing page
                if (rid.Recipe_Name == null)
                    return RedirectToAction("Index", "Home");


                var getComments = from usr in db.AspNetUsers
                                  join comm in db.Comments on usr.Id equals comm.User_ID
                                  where comm.Recipe_ID == recID
                                  select new { Comms = comm, userInfo = usr };

                //Create comment objects
                foreach (var commentInfo in getComments)
                {

                    com.Add(new Comments()
                    {
                        comment = commentInfo.Comms.Comment1,
                        commentDate = commentInfo.Comms.Comment_Date,
                        getUser = new Users { userName = commentInfo.userInfo.UserName, userProfilePicture = "../../ProfilePicture/" + commentInfo.userInfo.User_Picture_Path, userID = commentInfo.userInfo.Id },
                        commentID = commentInfo.Comms.Comment_ID

                    });
                }

                var getUsername = (from usr in db.AspNetUsers where usr.Id == uploaderID select usr);
                //Get user Information and Create user object
                //Most of the foreaches used serve to ensure that there are results in the querys.
                //This will never run if getUsername returns nothing
                foreach (var userinfo in getUsername)
                {
                    Uploader.userName = userinfo.UserName;
                    Uploader.userProfilePicture = userinfo.User_Picture_Path;
                    Uploader.userID = uploaderID;
                }

                var getIngredients = (from ingr in db.Ingredients where ingr.Recipe_ID == recID select ingr);

                //Get Ingredient Information and create ingredient objects
                foreach (var ingredientInfo in getIngredients)
                {
                    ing.Add(new Ingredients()
                    {
                        ingredientName = ingredientInfo.Ingredient_Name,
                        measurement = ingredientInfo.Measurement,
                    });
                }




                var getImages = (from imgs in db.Pictures where imgs.Recipe_ID == recID select imgs);

                //Get images information and create picture objects
                foreach (var imagesInfo in getImages)
                {

                    img.Add(new Picture()
                    {
                        Picture_Location = imagesInfo.Picture_Location
                    });
                }

                //Pass the recipePage object all other created objects
                rp.getComments = com;
                rp.getRecipe = rid;
                rp.getImages = img;
                rp.getAppliances = appl;
                rp.getIngredients = ing;
                rp.getTags = udl;
                rp.Uploader = Uploader;

                if (User.Identity.IsAuthenticated == true)
                    rp.signedIn = "t";
                else
                    rp.signedIn = "f";

                //Update the number of view on the recipe
                var updateViews = (from views in db.Recipes where views.Recipe_ID == id select views).First();
                updateViews.Number_Of_Views = updateViews.Number_Of_Views + 1;
                db.SaveChanges();


                return View("Recipe", rp);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }




        }

        /// <summary>
        /// Check to see if a recipe is liked so we know if we should display the Like or Unlike button on a recipe
        /// </summary>
        /// <returns>True if liked. False if not liked</returns>
        public bool isLiked()
        {
            var uid = User.Identity.GetUserId();
            var rid = Session["recipeID"] as string;
            if (uid != "")
            {
                //If there is a result than this recipe is liked
                var getLiked = (from l in db.Liked_Recipes where l.User_ID == uid && l.Recipe_ID == rid select l);
                foreach (var liked in getLiked)
                    return true;
            }


            return false;
        }



        /// <summary>
        /// This method saves the currently viewed recipe as a docx file
        /// </summary>
        /// <returns>The file</returns>
        public ActionResult saveRecipe()
        {
            try
            {
                var id = Session["recipeID"] as string;
                var title = "";
                var desc = "";
                var uploadingUser = "";
                List<String> ingred = new List<string>();
                var instr = "";
                List<String> toolsUsed = new List<string>();
                var timeToCook = "";
                List<String> picturesList = new List<string>();

                //Get all values for the recipe
                var getRecipe = (from rec in db.Recipes where rec.Recipe_ID == id select rec).First();


                title = getRecipe.Recipe_Name;
                desc = getRecipe.Recipe_Description;
                uploadingUser = getRecipe.AspNetUser.UserName;
                instr = getRecipe.Instructions;
                var appl = getRecipe.Appliances_Used.ToList();
                for (int a = 0; a < appl.Count; a++)
                    toolsUsed.Add(appl[a].Appliance_Name);

                var ingredList = getRecipe.Ingredients.ToList();
                for (int a = 0; a < ingredList.Count; a++)
                {
                    ingred.Add(ingredList[a].Measurement + " " + ingredList[a].Ingredient_Name);
                }
                var picList = getRecipe.Pictures.ToList();
                for (int a = 0; a < picList.Count; a++)
                {
                    picturesList.Add(Path.Combine(Server.MapPath("../"), picList[a].Picture_Location.Substring(6)));
                }
                timeToCook = getRecipe.Cooking_Time;


                //Set a path to save
                var path = Path.Combine(Server.MapPath("~/SaveRecipe"), title + User.Identity.GetUserId() + ".docx");
                //Create the document and insert data
                var doc = DocX.Create(path);
                doc.InsertParagraph(title).Bold().FontSize(32).UnderlineColor(Color.Black);
                doc.InsertParagraph();
                doc.InsertParagraph();
                doc.InsertParagraph("This Recipe was added by the user : " + uploadingUser);
                doc.InsertParagraph();
                doc.InsertParagraph("Description").Bold().FontSize(22);
                doc.InsertParagraph(desc);
                doc.InsertParagraph();
                doc.InsertParagraph("Ingredients").Bold().FontSize(22);
                var t = "";
                foreach (var ing in ingred)
                    t += ing + "          ";
                doc.InsertParagraph(t);
                doc.InsertParagraph();
                doc.InsertParagraph("Instructions").Bold().FontSize(22);
                doc.InsertParagraph(instr);
                doc.InsertParagraph();
                doc.InsertParagraph("Tools used").Bold().FontSize(22);
                foreach (var tool in toolsUsed)
                    doc.InsertParagraph(tool);
                doc.InsertParagraph();
                doc.InsertParagraph("Preparation Time").Bold().FontSize(22);
                doc.InsertParagraph(timeToCook);
                doc.InsertParagraph();
                doc.InsertParagraph();
                doc.InsertParagraph();
                doc.InsertParagraph("Images").Bold().FontSize(22);
                for (int i = 0; i < picturesList.Count; i++)
                {
                    Novacode.Image img = doc.AddImage(picturesList[i] + "");
                    Novacode.Picture pic1 = img.CreatePicture();
                    pic1.Width = (int)560;
                    pic1.Height = (int)400;
                    Paragraph p = doc.InsertParagraph("", false);
                    p.InsertPicture(pic1, 0).Position(.5);

                }
                doc.Save();


                string filePath = path;
                string contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";


                return File(filePath, contentType, title + ".docx");
            }
            catch (Exception)
            {
                return View("Error");
            }


        }


        /// <summary>
        /// Lets a user like a recipe
        /// </summary>
        /// <param name="lod">U or L depending on if the recipe being viewed is Liked or not</param>
        public void Like(string lod)
        {
            try
            {
                string userid = User.Identity.GetUserId();
                if (!string.IsNullOrEmpty(userid))
                {
                    var rid = Session["recipeID"] as string;
                    if (lod == "L")
                    {
                        Liked_Recipes lr = new Liked_Recipes();
                        lr.Recipe_ID = rid;
                        lr.User_ID = userid;
                        lr.Liked_Recipe_ID = returnRandomID();
                        lr.Date_Liked = DateTime.Today.ToString();
                        var likedR = db.Set<Liked_Recipes>();
                        likedR.Add(lr);
                        db.SaveChanges();
                    }
                    else if (lod == "U")
                    {
                        var getLiked = (from l in db.Liked_Recipes where l.User_ID == userid && l.Recipe_ID == rid select l).First();
                        db.Liked_Recipes.Remove(getLiked);
                        db.SaveChanges();
                    }
                }

            }
            catch (Exception)
            {
            }

        }



        /// <summary>
        /// This AddRecipe method calls the AddRecipe view
        /// </summary>
        /// <returns>AddRecipe View</returns>
        public ActionResult AddRecipe()
        {
            if (!string.IsNullOrEmpty(User.Identity.GetUserId()))
            {
                ViewBag.UserID = User.Identity.GetUserId();
                Session["pictureName"] = null;

                return View("AddRecipe");
            }
            return RedirectToAction("Index", "Home");
        }


        /// <summary>
        /// Add all Tags to a list and pass back to the view so the tags field can predict tags. 
        /// </summary>
        /// <returns>List of Tags</returns>
        public JsonResult SearchTags()
        {
            if (!string.IsNullOrEmpty(User.Identity.GetUserId()))
            {
                //Get all tags where they aren't header tags and add them to a list
                var getTagsList = (from ing in db.tagsLists where ing.AlphaTag == "0" select ing);
                List<string> resultsList = new List<string>();
                foreach (var tagsInfo in getTagsList)
                {
                    resultsList.Add(tagsInfo.tag_Name.Replace("_", " "));
                }
                //Pass that list to the view
                return Json(resultsList, JsonRequestBehavior.AllowGet);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// Add all appliances to a list and pass back to the view so the appliances field can predict appliances. 
        /// </summary>
        /// <returns>List of appliances</returns>
        public JsonResult SearchAppliances()
        {
            if (!string.IsNullOrEmpty(User.Identity.GetUserId()))
            {
                //Get all appliances and add them to a list
                var getAppl = (from apl in db.AppliancesLists select apl);
                List<string> resultsList = new List<string>();
                foreach (var applInfo in getAppl)
                {
                    resultsList.Add(applInfo.Appliance_Name.Replace("_", " "));

                }
                //Pass that list back to the view.
                return Json(resultsList, JsonRequestBehavior.AllowGet);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Add all ingredients to a list and pass back to the view so the ingredients field can predict ingredients. 
        /// </summary>
        /// <returns>List of ingredients</returns>
        public JsonResult AutoIngredient()
        {
            if (!string.IsNullOrEmpty(User.Identity.GetUserId()))
            {
                //Get all ingredients from the database and add them to a list
                var autocom = (from aut in db.IngredientsLists select aut);
                List<string> resultsList = new List<string>();
                foreach (var autocomplete in autocom)
                {
                    resultsList.Add(autocomplete.Ingredient_Name);

                }
                //Pass the list back to the view
                return Json(resultsList, JsonRequestBehavior.AllowGet);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }






        /// <summary>
        /// This method uploads the images that have been uploaded for the recipe before the recipe has been submitted.
        /// The reason the images are being saved before the recipe is being saved is so that the add an image page can show the image that the user has just uploaded.
        /// Because there is a possibility that the user may close the add a recipe page mid way through these images are stored in a temporary folder which can be periodically cleared
        /// </summary>
        /// <returns>Location of the newly uploaded file so the add image page may display the newly uploaded image</returns>
        [HttpPost]
        public ActionResult Upload()
        {

            try
            {
                if (!string.IsNullOrEmpty(User.Identity.GetUserId()))
                {
                    List<string> pictureName = new List<string>();

                    //Image is a required part of the recipe and the first thing to interact with the database.
                    //Because of this the random recipe id is generated here and passed to the recipe through a session varuabke
                    var rid = returnRandomID();
                    Session["UploadingRecipeID"] = rid;

                    var fileName = "";
                    List<String> names = new List<string>();

                    var path = "";
                    var prev = "11";
                    string x = "";

                    //For all files uploaded
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        //Make sure every id is unique
                        var file = Request.Files[i];
                        while (x == "" || x == prev)
                        {
                            x = returnRandomID();
                        }
                        prev = x;

                        //Get the filename
                        fileName = Path.GetFileName(file.FileName);



                        if (file != null && file.FileName.Length >= 5)
                        {
                            //Find the file extension
                            var ext = file.FileName.Substring(file.FileName.Length - 5);
                            if (ext.Contains(".bmp"))
                                ext = ".bmp";
                            else if (ext.Contains(".jpg"))
                                ext = ".jpg";
                            else if (ext.Contains(".jpeg"))
                                ext = ".jpeg";
                            else if (ext.Contains(".png"))
                                ext = ".png";

                            //Add location of image to pass back to view
                            names.Add("../../temp/" + rid + x + ext);

                            //Set save path
                            path = Path.Combine(Server.MapPath("~/temp/"), rid + x + ext);

                            //Add to list of uploaded images
                            pictureName.Add(rid + x + ext);

                            //Save image to the temporary location
                            file.SaveAs(path);

                            //If the image in temp is in portrait switch it to landscape
                            System.Drawing.Image img = System.Drawing.Image.FromFile(path);
                            if (img.Width < img.Height)
                            {
                                //rotate the image
                                img.RotateFlip(RotateFlipType.Rotate90FlipXY);

                                img.Save(path);



                            }
                            img.Dispose();

                        }

                    }
                    List<string> allPictures = new List<string>();

                    //Check if there are any other image already uploaded for this recipe and set them to a new list if there are.
                    if (Session["pictureName"] != null)
                        allPictures = (List<string>)Session["pictureName"];

                    //For all images uploaded this time add them to the allPictures variable 
                    foreach (string pn in pictureName)
                        allPictures.Add(pn);

                    //Set pictureName to allPictures which contains all values from itself + any new uploaded images
                    Session["pictureName"] = allPictures;

                    //Return the list of image locations to display in the view
                    return Json(new { names });
                }
            }
            catch (Exception)
            {
            }
            return Json(new { });
        }


        /// <summary>
        /// This rotates an image at the location of the passed src
        /// </summary>
        /// <param name="src">Image location</param>
        public void rotateImage(string src)
        {

            try
            {

                string sub = "";
                if (src.Contains("RecipeImages"))
                {
                    //The recipe will only be flipped on the current or last recipe edited and only if its src is accurate.
                    //This should prevent anyone from just typing Recipes/rotateImage/randomguess.jpg because it will only flip if they are signed in and have - 
                    //been in the edit view of a recipe as well as having the src for the image
                    string uid = User.Identity.GetUserId();
                    string recipeID = Session["editID"] as String ?? "";
                    var isCreator = (from rec in db.Recipes where rec.User_ID == uid && rec.Recipe_ID == recipeID select rec);
                    foreach (var item in isCreator)
                        sub = "~/" + src.Substring(src.IndexOf("RecipeImages"));
                }
                else
                {
                    sub = "~/" + src.Substring(src.IndexOf("temp"));
                }

                if (sub != "")
                {


                    var path = Server.MapPath(sub);

                    System.Drawing.Image img = System.Drawing.Image.FromFile(path);

                    //rotate the image
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    if (System.IO.File.Exists(path))
                    {
                        img.Save(path);
                    }

                    img.Dispose();
                }


            }
            catch (Exception)
            {

            }
        }



        /// <summary>
        /// This method takes the list of tags and appliances for the recipe that is being uploaded.
        /// The lists are stored in session varaibles and added to the recipe in the addrecipe method
        /// </summary>
        /// <param name="tags">Tag List for the uploaded recipe</param>
        /// <param name="appl">Appliance List for the uploaded recipe</param>
        public void setLists(List<string> tags, List<string> appl)
        {
            Session["tagsList"] = tags;
            Session["appliancesList"] = appl;
        }



        /// <summary>
        /// This is a users version of remove a comment, they may remove a comment that they have created
        /// </summary>
        /// <param name="commentid">The id of the comment to be deleted</param>
        public void removeComment(string commentid)
        {
            try
            {
                string uid = User.Identity.GetUserId() ?? "";
                var com = (from rec in db.Comments select rec).First();
                if (com != null)
                    //Make sure that the user who is trying to remove the comment is the comment creator
                    if (com.User_ID == uid)
                    {
                        //If they are, remove the comment
                        db.Comments.Remove(com);
                        db.SaveChanges();
                    }
            }
            catch (Exception)
            {
            }
        }



        /// <summary>
        /// This AddRecipe method takes all recipe information from the AddRecipe view and creates a new recipe 
        /// </summary>
        /// <param name="model">The model containing recipe information</param>
        /// <returns>view to Landing page</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult AddRecipe(AddRecipeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var uploadedRecipeID = (string)Session["UploadingRecipeID"];
                List<string> pictureName = new List<string>();
                pictureName = (List<string>)Session["pictureName"];

                if (pictureName.Count != 0)
                    for (int i = 0; i < pictureName.Count; i++)
                    {

                        try
                        {
                            var a = Server.MapPath("~/temp/" + pictureName[i]);
                            var b = Server.MapPath("~/RecipeImages/" + pictureName[i]);

                            System.IO.File.Move(a, b);

                        }
                        catch (IOException)
                        {

                        }

                    }



                //Set all information in a new Recipe object
                Recipe rec = new Recipe();

                rec.Recipe_ID = uploadedRecipeID;
                rec.Recipe_Name = model.recipeName;
                rec.Recipe_Search_Name = model.recipeName.Replace(" ", "_");
                rec.Recipe_Description = model.Description;
                if (model.Description.Length > 45)
                    rec.Recipe_Short_Description = model.Description.Substring(0, 45) + "...";
                else
                    rec.Recipe_Short_Description = model.Description;
                if (model.recipeVisibility == 1 || model.recipeVisibility == 0)
                    rec.Recipe_Visibility = model.recipeVisibility;
                else
                    rec.Recipe_Visibility = 0;
                rec.User_ID = User.Identity.GetUserId();
                rec.Instructions = model.Instructions;
                rec.Cooking_Time = model.time;
                rec.Cooking_Time_Search_Name = model.time.Replace(" ", "_");
                rec.Date_Uploaded = DateTime.Today.ToString();
                rec.Number_Of_Views = 1;
                rec.Display_Picture_Location = "../../RecipeImages/" + pictureName[0];

                //Add and save the recipe object
                var us = db.Set<Recipe>();
                us.Add(rec);
                db.SaveChanges();

                //Get all tags from the model and add them to new User_Defined_Tags objects
                var tagsList = (List<string>)Session["tagsList"];
                if (tagsList != null)
                    for (int i = 0; i < tagsList.Count; i++)
                    {
                        User_Defined_Tags ud = new User_Defined_Tags() { Recipe_ID = uploadedRecipeID, Tag_ID = returnRandomID(), User_Defined_Tag = tagsList[i], User_Defined_Tag_Search = tagsList[i].Replace(" ", "_") };
                        var udfdb = db.Set<User_Defined_Tags>();
                        udfdb.Add(ud);
                    }

                //Get all appliances from the model and add them to a new appliances models
                var appliancesList = (List<string>)Session["appliancesList"];
                if (appliancesList != null)
                    for (int i = 0; i < appliancesList.Count; i++)
                    {
                        var check = returnRandomID();
                        var exists = "t";
                        do
                        {
                            var getAppl = (from apl in db.AppliancesLists where apl.Appliance_Id == check select apl).ToList();

                            if (getAppl.Count == 0)
                                exists = "f";
                            check = returnRandomID();

                        } while (exists == "t");

                        Appliances_Used au = new Appliances_Used() { Id = check, Appliance_Name = appliancesList[i], Appliance_Name_Search = appliancesList[i].Replace(" ", "_"), Recipe_Id = uploadedRecipeID };
                        var apudb = db.Set<Appliances_Used>();
                        apudb.Add(au);
                    }


                //Get all images from the recipe and set them to new Picture objects
                for (int i = 0; i < pictureName.Count; i++)
                {
                    Picture p = new Picture() { Picture_Location = "../../RecipeImages/" + pictureName[i], Recipe_ID = uploadedRecipeID, Picture_ID = uploadedRecipeID + returnRandomID() };
                    var pdb = db.Set<Picture>();
                    pdb.Add(p);
                }


                //Get all ingredents and add them to new ingredients objects
                for (int i = 0; i < model.Measurement.Count; i++)
                {
                    Ingredient ing = new Ingredient() { Measurement = model.Measurement[i], Ingredient_Name = model.Ingredient_Name[i], Ingredient_Name_Search = model.Ingredient_Name[i].Replace(" ", "_"), Recipe_ID = uploadedRecipeID, Recipe_Ingredient_ID = returnRandomID() };
                    var ingdb = db.Set<Ingredient>();
                    ingdb.Add(ing);
                }

                //Save all objects added
                db.SaveChanges();
            }
            catch (Exception)
            {

            }

            return RedirectToAction("Index", "Home");
        }


        /// <summary>
        /// User submit comment
        /// </summary>
        /// <param name="comment">Comment to submit</param>
        /// <returns>Comment submitted to be displayed</returns>
        [HttpPost]
        public ActionResult commentSubmit(string comment)
        {
            string rid = Session["recipeID"] as String;
            string userid = User.Identity.GetUserId();
            if (ModelState.IsValid && userid != null && rid != null)
            {
                List<String> returnList = new List<string>();

                try
                {
                    //Get the users image
                    var getImg = (from img in db.AspNetUsers where img.Id == userid select img);

                    //Add the comment to the the return list thats being passed back
                    returnList.Add(comment);

                    //Add the user image to the return variable
                    foreach (var img in getImg)
                    {
                        returnList.Add(img.User_Picture_Path);
                        returnList.Add(img.UserName);
                    }

                    //Add a new comment object to database
                    Comment com = new Comment
                    {
                        Comment_ID = returnRandomID(),
                        Comment1 = comment,
                        Recipe_ID = rid,
                        Comment_Date = DateTime.Today.ToString(),
                        User_ID = userid,
                    };
                    var comments = db.Set<Comment>();
                    comments.Add(com);
                    db.SaveChanges();

                    //Add the comment id to the returns list
                    returnList.Add("" + com.Comment_ID);
                    //Return that list so the comment can be added to the page
                    return Json(new { returnList });
                }
                catch (Exception)
                {

                }

            }
            return Json(new { });
        }


        /// <summary>
        /// Report Either a recipe or comment
        /// </summary>
        /// <param name="commentID">Comment id if its a comment being reported empty string otherwise</param>
        /// <param name="reasonForReport">The </param>
        /// <param name="rType">Whats being reported, comment or recipe</param>
        public void Report(string commentID, string reasonForReport, string rType)
        {
            try
            {
                string rid = Session["recipeID"] as String;
                string cName = "";
                string cID = "";
                string comUrl = "";
                string recName = "";
                if (rid != null && reasonForReport != "" && rType != null)
                {
                    if (rType == "Comment")
                    {
                        //Get Information about the comment being reported
                        var getCom = (from com in db.Comments where com.Comment_ID == commentID select com).First();
                        cName = getCom.AspNetUser.UserName;
                        cID = getCom.User_ID;
                        comUrl = getCom.Comment1;

                    }
                    else if (rType == "Recipe")
                    {
                        //Get information about the recipe being reported
                        var getRec = (from rec in db.Recipes where rec.Recipe_ID == rid select rec).First();
                        cName = getRec.AspNetUser.UserName;
                        cID = getRec.User_ID;
                        recName = getRec.Recipe_Name;
                    }


                    //Create and save the report
                    Report r = new Report
                    {
                        Report_Id = returnRandomID(),
                        Comment_Or_Recipe_Id = rid.ToString(),
                        Reason_For_Report = reasonForReport,
                        Type = rType,
                        Reporter_Name = User.Identity.Name,
                        Content_Creator_Name = cName,
                        Content_Creator_Id = cID,
                        Reporter_Id = User.Identity.GetUserId(),
                        Comment = comUrl,
                        Recipe_Name = recName,
                        Recipe_ID = rid,
                        Comment_ID = commentID

                    };
                    var us = db.Set<Report>();
                    us.Add(r);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        /// Delete the recipe
        /// </summary>
        /// <returns>Landing page</returns>
        public ActionResult deleteRecipe()
        {
            try
            {
                string rid = Session["recipeID"] as String;
                var getAuthorID = (from getAuthor in db.Recipes where getAuthor.Recipe_ID == rid select getAuthor.User_ID).First();
                string authorID = "";
                if (getAuthorID != null)
                    authorID = getAuthorID;


                //If the currently logged in user is the creator of the recipe
                if (authorID.Equals(User.Identity.GetUserId()))
                {

                    //Remove Ingredients
                    var ing = (from c in db.Ingredients where c.Recipe_ID == rid select c);
                    foreach (var q in ing)
                        db.Ingredients.Remove(q);

                    //Remove Tags
                    var tags = (from c in db.User_Defined_Tags where c.Recipe_ID == rid select c);
                    foreach (var q in tags)
                        db.User_Defined_Tags.Remove(q);

                    //Remove Appliances
                    var appl = (from c in db.Appliances_Used where c.Recipe_Id == rid select c);
                    foreach (var q in appl)
                        db.Appliances_Used.Remove(q);

                    //Remove Pictures
                    var pics = (from c in db.Pictures where c.Recipe_ID == rid select c);
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

                    //Remove comments on recipe
                    var removeComments = (from comm in db.Comments where comm.Recipe_ID == rid select comm);
                    foreach (var c in removeComments)
                        db.Comments.Remove(c);

                    //Remove reports from recipe
                    var removeReports = (from report in db.Reports where report.Recipe_ID == rid select report);
                    foreach (var rr in removeReports)
                        db.Reports.Remove(rr);

                    //Remove Likes from the recipe
                    var like = (from l in db.Liked_Recipes where l.Recipe_ID == rid select l);
                    foreach (var removeLike in like)
                        db.Liked_Recipes.Remove(removeLike);

                    //Remove the recipe
                    var rec = (from c in db.Recipes where c.Recipe_ID == rid select c).First();
                    db.Recipes.Remove(rec);
                    db.SaveChanges();

                    //A message is set so home index can display it
                    Session["messageExists"] = true;
                    Session["message"] = "The recipe has been removed";
                }
                else
                {
                    Session["messageExists"] = true;
                    Session["message"] = "Invalid Operation";
                }
            }
            catch (Exception)
            {
                Session["messageExists"] = true;
                Session["message"] = "An Error has occured";
            }

            return RedirectToAction("Index", "Home");
        }



    }


}