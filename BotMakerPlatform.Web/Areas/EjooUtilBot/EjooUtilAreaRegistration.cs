using System.Web.Mvc;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot
{
    public class EjooUtilAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "EjooUtilBot";

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "EjooUtilBot_default",
                "EjooUtilBot/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}