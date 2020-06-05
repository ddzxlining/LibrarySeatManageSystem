using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SelectOnWeb.Areas.Admin.Models;
using SelectOnWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SelectOnWeb.Infrastructure
{
    public class AppRoleManager : RoleManager<AppRole>, IDisposable
    {
        public AppRoleManager(IRoleStore<AppRole, string> store) : base(store) { }
        public static AppRoleManager Create(
            IdentityFactoryOptions<AppRoleManager> options,
            IOwinContext context)
        {
            return new AppRoleManager(new RoleStore<AppRole>(context.Get<ApplicationDbContext>()));
        }
    }
}