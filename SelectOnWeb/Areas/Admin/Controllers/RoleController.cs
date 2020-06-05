using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SelectOnWeb.Areas.Admin.Models;
using SelectOnWeb.Infrastructure;
using SelectOnWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SelectOnWeb.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles ="超级管理员")]
    public class RoleController : Controller
    {
        private AppRoleManager _roleManager;
        private ApplicationUserManager _userManager;
        public AppRoleManager RoleManager { get { return _roleManager ?? HttpContext.GetOwinContext().GetUserManager<AppRoleManager>(); }private  set { _roleManager = value; } }
        public ApplicationUserManager UserManager { get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); } private set { _userManager = value; } }

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Role
        [CustomAuthorize(Roles ="管理员")]
        public ActionResult Index()
        {         
            return View(db.Set<AppRole>().ToList());
        }
        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(AddRoleViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            AppRole role = new AppRole() { Name = model.roleName };
            var result =await RoleManager.CreateAsync(role);
            if(result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                AddErrors(result);
            }
            return View(model);
        }
        public ActionResult Delete(string roleName)
        {
            if (roleName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }           
            AppRole role = RoleManager.FindByName(roleName);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View((object)roleName);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string roleName)
        {
            var role=RoleManager.FindByName(roleName);
            var result=RoleManager.Delete(role);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return HttpNotFound();
            }
        }
        public ActionResult Details(string roleName)
        {
            AppRole role = RoleManager.FindByName(roleName);           
            if (role == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                return View(role);
            }           
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }      
    }
}