using System.Web.Mvc;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot
{
    public class EjooUtilAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "EjooUtil";

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "EjooUtil_default",
                "EjooUtil/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}