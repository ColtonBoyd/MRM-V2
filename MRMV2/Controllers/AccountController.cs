using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections;
using System.Net.Mail;
using System.Collections.Generic;
using System.Net;
using MRMV2.Models;

namespace MRMV2.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private rdbEntities db = new rdbEntities();
        private EmailAuth ea = new EmailAuth();


        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// This Login method loads the login view
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View("Login");
        }

        /// <summary>
        /// The login method is a scaffolded asp mvc piece that handles logging in
        /// </summary>
        /// <param name="model">Login view model</param>
        /// <param name="returnUrl">Page to return to</param>
        /// <returns></returns>
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                return View(model);
            }



            var user = await UserManager.FindAsync(model.Email, model.Password);

            if (user != null)
                if (user.EmailConfirmed == false)
                {
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);


                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 25;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(ea.getEmail(), ea.getPassword());

                    using (var message = new MailMessage(ea.getEmail(), model.Email))
                    {
                        message.Subject = "Confirm Your Account"; ;
                        message.Body = "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>";
                        message.IsBodyHtml = true;
                        smtp.Send(message);
                    }


                    return View("DisplayEmail");
                }

            var isBanned = (from usr in db.AspNetUsers where usr.Email == model.Email select usr).ToList();
            if (isBanned != null && isBanned.Count > 0)
                if (isBanned[0].Banned == "t")
                {


                    Session["messageExists"] = true;
                    Session["message"] = "This account has been banned!";
                    return RedirectToAction("Index", "Home");

                }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: true);


            switch (result)
            {
                case SignInStatus.Success:
                    var uid = "";
                    var getImgLocation = from usr in db.AspNetUsers
                                         where usr.Email == model.Email
                                         select new { usr };
                    foreach (var i in getImgLocation)
                    {
                        uid = i.usr.Id;
                        ViewBag.UserID = i.usr.Id;
                        ViewBag.UserImageLoc = i.usr.User_Picture_Path;
                        ViewBag.UserName = i.usr.UserName;

                    }


                    if (isAdmin())
                        return RedirectToAction("Index", "Admin");
                    else
                    {
                        var thm = (from th in db.ThemeGalleries
                                   where th.User_Id == uid
                                   select th).ToList();
                        if (thm.Count == 0)
                        {
                            ThemeGallery theme = new ThemeGallery();
                            var mainTheme = (from mt in db.ThemeGalleries where mt.Theme_Id == "0" select mt);
                            foreach (var updateTheme in mainTheme)
                            {

                                theme.Arrow_Down = updateTheme.Arrow_Down ?? "";
                                theme.Arrow_Left = updateTheme.Arrow_Left ?? "";
                                theme.Arrow_Right = updateTheme.Arrow_Right ?? "";
                                theme.Arrow_Up = updateTheme.Arrow_Up ?? "";
                                theme.Background_Box_Color = updateTheme.Background_Box_Color ?? "";
                                theme.Background_Box_Opacity = updateTheme.Background_Box_Opacity ?? "";
                                theme.Background_Color = updateTheme.Background_Color ?? "";
                                theme.Background_Color_Opacity = updateTheme.Background_Color_Opacity ?? "";
                                theme.Background_Image = updateTheme.Background_Image ?? "";
                                theme.Background_Image_Color_Or_Theme = updateTheme.Background_Image_Color_Or_Theme ?? "";
                                theme.Contrast_Box_Background_Color = updateTheme.Contrast_Box_Background_Color ?? "";
                                theme.Contrast_Box_Background_Color_Opacity = updateTheme.Contrast_Box_Background_Color_Opacity ?? "";
                                theme.Hr_Color = updateTheme.Hr_Color ?? "";
                                theme.Input_Color = updateTheme.Input_Color ?? "";
                                theme.Input_Opacity = updateTheme.Input_Opacity ?? "";
                                theme.Logo_BitBucket = updateTheme.Logo_BitBucket ?? "";
                                theme.Logo_Facebook = updateTheme.Logo_Facebook ?? "";
                                theme.Logo_Garbage = updateTheme.Logo_Garbage ?? "";
                                theme.Logo_Info = updateTheme.Logo_Info ?? "";
                                theme.Logo_Main = updateTheme.Logo_Main ?? "";
                                theme.Logo_Reddit = updateTheme.Logo_Reddit ?? "";
                                theme.Logo_Save = updateTheme.Logo_Save ?? "";
                                theme.Logo_Twitter = updateTheme.Logo_Twitter ?? "";
                                theme.Name = updateTheme.Name ?? "";
                                theme.Permenant = 0;
                                theme.Text_Color = updateTheme.Text_Color ?? "";
                                theme.Theme_Icon_Path = updateTheme.Theme_Icon_Path ?? "";
                                theme.Theme_Id = returnRandomID();
                                theme.User_Id = uid ?? "";
                                theme.User_Custom_Theme = 0;
                            }
                            var upd = db.Set<ThemeGallery>();
                            upd.Add(theme);
                            db.SaveChanges();

                            List<ThemeGallery> themeGal = new List<ThemeGallery>();
                            themeGal.Add(theme);

                            var usr = (from usrTheme in db.AspNetUsers where usrTheme.Id == uid select usrTheme).First();
                            usr.ThemeGalleries = themeGal;
                            db.SaveChanges();
                        }



                        var opt = (from th in db.UserOptions
                                   where th.User_Id == uid
                                   select th).ToList();
                        if (opt.Count == 0)
                        {
                            UserOption addUserOptions = new UserOption() { User_Id = uid, Recipes_Per_Page = 20, Search_Visibility = 1 };
                            var upd = db.Set<UserOption>();
                            upd.Add(addUserOptions);
                            db.SaveChanges();

                        }


                        return RedirectToLocal(returnUrl);
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }





        }


        /// <summary>
        /// Update the users description on their profile
        /// </summary>
        /// <param name="description">New Description</param>
        public void updateUserDescription(String description)
        {
            var temp = User.Identity.GetUserId();
            var updateUserDescription = db.AspNetUsers.Where(s => s.Id == temp).First();
            updateUserDescription.User_Description = description;
            db.SaveChanges();

        }




        /// <summary>
        /// Get the user id
        /// </summary>
        /// <returns>User id</returns>
        [AllowAnonymous]
        public string getUserID()
        {
            return User.Identity.GetUserId();
        }



        /// <summary>
        /// Invite a new admin to the website
        /// </summary>
        /// <param name="ap">The admin page model</param>
        /// <returns></returns>
        public ActionResult inviteNewAdmin(AdminPageModel ap)
        {

            if (isAdmin())
            {
                try
                {


                    var id = ap.inviteEmail;
                    //If that user already exists dont invite the user.
                    var exists = (from user in db.AspNetUsers where user.Email == id select user);
                    foreach (var usr in exists)
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    if (!string.IsNullOrEmpty(id))
                    {
                        DateTime t = DateTime.Now.AddDays(2);
                        string rid = returnRandomID();
                        Invitation u = new Invitation
                        {
                            End_Date = t,
                            Code = rid,


                        };
                        var us = db.Set<Invitation>();
                        us.Add(u);
                        db.SaveChanges();





                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        smtp.Port = 25;
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(ea.getEmail(), ea.getPassword());

                        using (var message = new MailMessage(ea.getEmail(), id))
                        {
                            message.Subject = "Registration Code";
                            message.Body = string.Format("{0}://{1}{2}{3}", Request.Url.Scheme, Request.Url.Authority, "/Account/sRegister/", rid);
                            message.IsBodyHtml = true;
                            smtp.Send(message);
                        }



                    }
                }
                catch (Exception) { }

            }
            return RedirectToAction("Index", "Admin");

        }



        Random rng = new Random();
        /// <summary>
        /// Return a random string to be used as an id
        /// </summary>
        /// <returns>Random string</returns>
        public string returnRandomID()
        {
            String randomString = "";
            ArrayList ran = new ArrayList() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l",
                  "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
                  "O", "P", "Q", "R", "S", "T","U","V","W","X","Y","Z"};

            for (var i = 0; i < 50; i++)
                randomString += ran[rng.Next(0, ran.Count)];
            return randomString;
        }


        /// <summary>
        /// Loads the register view model
        /// </summary>
        /// <returns>Register view model</returns>
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }


        /// <summary>
        /// Loads the Admin registration page 
        /// </summary>
        /// <param name="s">The invite key</param>
        /// <returns></returns>
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult sRegister(object s)
        {
            ViewBag.code = s.ToString();
            return View("sRegister");
        }





        /// <summary>
        /// One of the default asp mvc methods used for regualar user registration
        /// </summary>
        /// <param name="model">Register model</param>
        /// <returns></returns>
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, User_Picture_Path = "defaultProfile.png" };
                var result = await UserManager.CreateAsync(user, model.Password);
                var roleManager = new RoleManager<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();

                if (!roleManager.RoleExists("User"))
                {
                    role.Name = "User";
                    roleManager.Create(role);

                }

                var currentUser = UserManager.FindByName(model.Email);

                UserManager.AddToRole(currentUser.Id, "User");

                if (result.Succeeded)
                {
                    
                    Security_Questions sq1 = new Security_Questions { Security_Question = model.Security_Question_1, Security_Answer = UserManager.PasswordHasher.HashPassword(model.Security_Question_Answer_1), User_ID = user.Id, Security_Question_ID = returnRandomID() };
                    Security_Questions sq2 = new Security_Questions { Security_Question = model.Security_Question_2, Security_Answer = UserManager.PasswordHasher.HashPassword(model.Security_Question_Answer_2), User_ID = user.Id, Security_Question_ID = returnRandomID() };
                    Security_Questions sq3 = new Security_Questions { Security_Question = model.Security_Question_3, Security_Answer = UserManager.PasswordHasher.HashPassword(model.Security_Question_Answer_3), User_ID = user.Id, Security_Question_ID = returnRandomID() };

                    var sec = db.Set<Security_Questions>();
                    sec.Add(sq1);
                    sec.Add(sq2);
                    sec.Add(sq3);


                    UserOption opt = new UserOption() { User_Id = user.Id, Recipes_Per_Page = 5, Search_Visibility = 1 };
                    var op = db.Set<UserOption>();
                    op.Add(opt);



                    db.SaveChanges();
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);


                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 25;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(ea.getEmail(), ea.getPassword());

                    using (var message = new MailMessage(ea.getEmail(), model.Email))
                    {
                        message.Subject = "Confirm Your Account"; ;
                        message.Body = "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>";
                        message.IsBodyHtml = true;
                        smtp.Send(message);
                    }



                    // await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    // await UserManager.AddToRoleAsync(user.Id, "User");
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link

                    return View("DisplayEmail");

                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        /// <summary>
        /// The admin register method
        /// </summary>
        /// <param name="model">Admin registration model</param>
        /// <returns>Landing page view</returns>
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> sRegister(RegisterViewModel model)
        {


            var url = Request.Url.AbsolutePath.LastIndexOf("/");
            var tempUrl = Request.Url.AbsolutePath.Substring(url + 1, Request.Url.AbsolutePath.Length - url - 1);

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, User_Picture_Path = "defaultProfile.png" };
                var result = await UserManager.CreateAsync(user, model.Password);
                var roleManager = new RoleManager<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();

                if (!roleManager.RoleExists("Admin"))
                {
                    role.Name = "Admin";
                    roleManager.Create(role);

                }

                var currentUser = UserManager.FindByName(model.Email);

                UserManager.AddToRole(currentUser.Id, "Admin");

                if (result.Succeeded)
                {

                    Security_Questions sq1 = new Security_Questions { Security_Question = model.Security_Question_1, Security_Answer = UserManager.PasswordHasher.HashPassword(model.Security_Question_Answer_1), User_ID = user.Id, Security_Question_ID = returnRandomID() };
                    Security_Questions sq2 = new Security_Questions { Security_Question = model.Security_Question_2, Security_Answer = UserManager.PasswordHasher.HashPassword(model.Security_Question_Answer_2), User_ID = user.Id, Security_Question_ID = returnRandomID() };
                    Security_Questions sq3 = new Security_Questions { Security_Question = model.Security_Question_3, Security_Answer = UserManager.PasswordHasher.HashPassword(model.Security_Question_Answer_3), User_ID = user.Id, Security_Question_ID = returnRandomID() };

                    var sec = db.Set<Security_Questions>();
                    sec.Add(sq1);
                    sec.Add(sq2);
                    sec.Add(sq3);


                    var usr = (from users in db.AspNetUsers where users.Id == user.Id select users).First();
                    usr.Roles = "Admin";

                    db.SaveChanges();

                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);





                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 25;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(ea.getEmail(), ea.getPassword());

                    using (var message = new MailMessage(ea.getEmail(), model.Email))
                    {
                        message.Subject = "Confirm Your Account"; ;
                        message.Body = "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>";
                        message.IsBodyHtml = true;
                        smtp.Send(message);
                    }



                    return View("DisplayEmail");

                }
                AddErrors(result);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }


            // If we got this far, something failed, redisplay form
            return View(model);
        }


        /// <summary>
        /// Check if user is admin
        /// </summary>
        /// <returns>True if user is admin, false if they are not</returns>
        public bool isAdmin()
        {

            string role = "";
            var uid = User.Identity.GetUserId() ?? "";
            if (String.IsNullOrEmpty(uid))
            {
                return false;
            }
            var getRole = (from usr in db.AspNetUsers where usr.Id == uid select new { usr }).First();
            if (getRole != null)
                role = getRole.usr.Roles;
            if (role == "Admin")
                return true;
            return false;

        }



        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }


        /// <summary>
        /// Loads the ForgotPassword View
        /// </summary>
        /// <returns></returns>
        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            if (!User.Identity.IsAuthenticated)
                return View();
            else
                return RedirectToAction("Index", "Home");
        }




        /// <summary>
        /// This method takes username and security question and ensures they are valid, If they are valid an email is sent to the address with a reset link. 
        /// If they are not valid the user is told an email is being sent but no email will be sent
        /// </summary>
        /// <param name="model">Forgot Password view model</param>
        /// <returns></returns>
        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {

            try
            {

                if (ModelState.IsValid)
                {

                    var user = await UserManager.FindByNameAsync(model.Email);
                    if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        return View("ForgotPasswordConfirmation");
                    }
                    var sec = (from s in db.Security_Questions where s.AspNetUser.Email == model.Email && s.Security_Question == model.questionAsked select s).ToList();
                    if (sec.Count > 0 && sec != null)
                    {
                        var result = UserManager.PasswordHasher.VerifyHashedPassword(sec[0].Security_Answer, model.Security_Question_Answer);
                        //If the email and security question answer are correct create a code and send an email
                        if (result == PasswordVerificationResult.Success)
                        {

                            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);


                            SmtpClient smtp = new SmtpClient();
                            smtp.Host = "smtp.gmail.com";
                            smtp.Port = 25;
                            smtp.EnableSsl = true;
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new NetworkCredential(ea.getEmail(), ea.getPassword());

                            using (var message = new MailMessage(ea.getEmail(), model.Email))
                            {
                                message.Subject = "Confirm Your Account"; ;
                                message.Body = "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>";
                                message.IsBodyHtml = true;
                                smtp.Send(message);
                            }

                        }
                        return RedirectToAction("ForgotPasswordConfirmation", "Account");
                    }
                    else
                        return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }
            }
            catch (Exception)
            {

            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Load the ForgotPasswordConfirmation View
        /// </summary>
        /// <returns> ForgotPasswordConfirmation view</returns>
        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {

            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }


        /// <summary>
        /// This method is called on click of reset link sent to email
        /// </summary>
        /// <param name="code">Code sent</param>
        /// <returns>Error view or reset password view</returns>
        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return code == null ? View("Error") : View();
        }


        /// <summary>
        /// This method takes the new password and updates the database
        /// </summary>
        /// <param name="model">Reset Password view model values</param>
        /// <returns>redirect to resetpasswordconfirmation method</returns>
        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }





        /// <summary>
        /// This method logs off the user
        /// </summary>
        /// <returns>Landing page View</returns>
        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}