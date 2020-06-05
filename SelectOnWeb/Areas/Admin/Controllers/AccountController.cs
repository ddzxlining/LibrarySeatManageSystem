using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SelectOnWeb.Areas.Admin.Models;
using SelectOnWeb.Infrastructure;
using SelectOnWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SelectOnWeb.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles = "学生")]
    public class AccountController : Controller
    {
        private ApplicationUserManager _userManager;
        private AppRoleManager _roleManager;
        public ApplicationUserManager UserManager { get {return  _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); } set { _userManager = value; } }
        public AppRoleManager RoleManager { get { return _roleManager ?? HttpContext.GetOwinContext().GetUserManager<AppRoleManager>(); } set { _roleManager = value; } }

        // GET: Admin/Account
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Add()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in RoleManager.Roles)
                list.Add(new SelectListItem() { Text = item.Name, Value = item.Name });
            ViewData.Add("roles", list);
            return View();
        }
        [HttpPost]
        public ActionResult Add(AccountViewModel model)
        {
            ApplicationUser user = UserManager.FindByName(model.sno);
            if (user != null)
            {
                var result=UserManager.AddToRole(user.Id,model.role);
                if (result.Succeeded)
                    return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}