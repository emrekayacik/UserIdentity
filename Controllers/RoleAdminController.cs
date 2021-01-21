using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using UserIdentity.Identity;
using UserIdentity.Models;

namespace UserIdentity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleAdminController : Controller
    {
        private RoleManager<IdentityRole> RoleManager { get; set; }
        private UserManager<ApplicationUser> userManager;

        public RoleAdminController()
        {
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new IdentityDataContext()));
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new IdentityDataContext()));
        }
        // GET: RoleAdmin
        public ActionResult Index()
        {
            return View(RoleManager.Roles);
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(string name)
        {
            if (ModelState.IsValid)
            {
                var result = RoleManager.Create(new IdentityRole((name)));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var resultError in result.Errors)
                    {
                        ModelState.AddModelError("", resultError);
                    }
                }
            }
            return View(name);
        }

        [HttpPost]
        public ActionResult Delete(string Id)
        {
            var role = RoleManager.FindById(Id);
            if (role != null)
            {
                var result = RoleManager.Delete(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Error", result.Errors);
                }
            }
            return View("Error", new string[] { "Role not found" });
        }
        [HttpGet]
        public ActionResult Edit(string Id)
        {
            var role = RoleManager.FindById(Id);

            var members = new List<ApplicationUser>();
            var nonMembers = new List<ApplicationUser>();

            foreach (var user in userManager.Users.ToList())
            {
                var list = userManager.IsInRole(user.Id, role.Name) ?
                    members : nonMembers;
                list.Add(user);
            }

            return View(new RoleEditModel()
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }
        [HttpPost]
        public ActionResult Edit(RoleUpdateModel model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (var userId in model.IdsToAdd ?? new string[] { })
                {
                    result = userManager.AddToRole(userId,model.RoleName);
                    if (!result.Succeeded)
                    {
                        return View("Error",result.Errors);
                    }
                }

                foreach (var userId in model.IdsToDelete ?? new string[]{})
                {
                    result = userManager.RemoveFromRole(userId,model.RoleName);
                    if (!result.Succeeded)
                    {
                        return View("Error", result.Errors);
                    }
                }

                return RedirectToAction("Index");
            }
            return View("Error", new string[]{"aranılan rol yok"});
        }
    }

}