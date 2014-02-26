using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web;

using MvcApplicationIPS2.Repository.Schedule;
using MvcApplicationIPS2.Service.Schedule;
using MvcApplicationIPS2.Service.SAM;


namespace MvcApplicationIPS2.Web.Areas.Schedule.Attribute
{
    public class ScheduleAttribute: AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (!AuthorizeCore(filterContext.HttpContext))
            {
                HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            }
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");            

            SchSysModuleRelService sysModuleRelService = new SchSysModuleRelService();
            string sName;
            string mainid = httpContext.Request["mainid"];
            string sysid = httpContext.Request.RequestContext.RouteData.Values["sysid"] as string ?? httpContext.Request["sysid"] as string;
            string moduleid = httpContext.Request.RequestContext.RouteData.Values["moduleid"] as string ?? httpContext.Request["moduleid"] as string;            
            SchSysModuleRelRepository repo = sysModuleRelService.GetSysModRel(mainid: mainid, sysid: sysid, moduleid: moduleid);
            if (mainid == "F0EC4C5F384E2973E043E0C4380A7471" || (string.IsNullOrEmpty(mainid) && string.IsNullOrEmpty(sysid) && string.IsNullOrEmpty(moduleid)))
            {
                mainid = "F0EC4C5F384E2973E043E0C4380A7471";
                sName = "SCH_" + UserService.Pernr + "_" + mainid;

                httpContext.Session[sName] = true;
                return true;
            }
            sysid = repo.SYSID;
            moduleid = repo.MODULEID;

            List<string> memids = sysModuleRelService.GetOwnersBySysMod(sysid, moduleid);                                   



            sName = (repo != null && !string.IsNullOrEmpty(repo.MAIN_ID)) ? "SCH_" + UserService.Pernr  + "_" + repo.MAIN_ID : "SCH_";

            httpContext.Session[sName] = memids.Contains(UserService.Pernr);
            
            if (this.Roles == "ALL")
            {
                return true;
            }
            else if (mainid == "F0EC4C5F384E2973E043E0C4380A7471")
            {
                return true;
            }
            else if (this.Roles == "ADM")
            {
                return memids.Contains(UserService.Pernr);
            }
            else
            {
                return false;
            }
        }

    }
}