using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json.Linq;
using SelectOnWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace SelectOnWeb.Infrastructure
{
    public class CustomAuthorize:AuthorizeAttribute
    {
        public string AuthorizationFailView { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {            
            base.OnAuthorization(filterContext);
            bool islogin = filterContext.HttpContext.GetOwinContext().Authentication.User.Identity.IsAuthenticated;
            bool result = filterContext.HttpContext.GetOwinContext().Authentication.User.IsInRole(Roles);
            if (islogin && !result)
                filterContext.Result = new ViewResult
                {
                    ViewName = "Error"
                };
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}