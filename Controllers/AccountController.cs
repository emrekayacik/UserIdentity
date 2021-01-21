using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using UserIdentity.Identity;
using UserIdentity.Models;

namespace UserIdentity.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> userManager;

        public AccountController()
        {
            var userStore = new UserStore<ApplicationUser>(new IdentityDataContext());
            userManager = new UserManager<ApplicationUser>(userStore);
            userManager.PasswordValidator= new PasswordValidator()
            {
                RequireDigit = true,
                RequiredLength = 8,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonLetterOrDigit = true
            };
            userManager.UserValidator=new UserValidator<ApplicationUser>(userManager)
            {
                RequireUniqueEmail = true,
                AllowOnlyAlphanumericUserNames = false
            };
        }
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Register()
        {

            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Register model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser();
                user.UserName = model.UserName;
                user.Email = model.Email;

                var result = userManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    userManager.AddToRole(user.Id, "User");
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (var resultError in result.Errors)
                    {
                        ModelState.AddModelError("",resultError);
                    }
                }
            }
            else
            {
                return View(model);
            }
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return View("Error",new string[] {"Erişim Engellendi"});
            }
            ViewBag.returnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model,string returnUrl)
        {
            if (ModelState.IsValid)
            {

                var user = userManager.Find(model.UserName, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Yanlış kullanıcı adı veya parola");
                }
                else
                {
                    var authManager = HttpContext.GetOwinContext().Authentication;
                    var identity = userManager.CreateIdentity(user, "ApplicationCookie");
                    var authProperties = new AuthenticationProperties()
                    {
                        IsPersistent = true
                    };
                    authManager.SignOut();
                    authManager.SignIn(authProperties, identity);
                    return Redirect(string.IsNullOrEmpty(returnUrl) ? "/":returnUrl);
                }
            }
            else
            {
                ViewBag.returnUrl = returnUrl;
                return View(model);
            }

            return View();
        }

        public ActionResult Logout()
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut();
            return RedirectToAction("Login");
        }
    }
}