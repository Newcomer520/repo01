using System.Web.Mvc;

namespace MvcApplicationIPS2.Web.Areas.Schedule
{
    public class ScheduleAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Schedule";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Schedule_sysmod_abbrev",
                "ScheduleSM/{sysid}/{moduleid}",
                new { controller = "Default", action = "SysMod", sysid = UrlParameter.Optional, moduleid = UrlParameter.Optional }
            );
            context.MapRoute(
                "Schedule_sysmod",
                "Schedule/SysMod/{sysid}/{moduleid}",
                new { controller = "Default", action = "SysMod", sysid = UrlParameter.Optional, moduleid = UrlParameter.Optional}
            );
            context.MapRoute(
                "Schedule_default0",
                "Schedule/{action}/{id}",
                new { controller="Default", action = "Index", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Schedule_default",
                "Schedule/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
