using System.Web.Mvc;

namespace BotMakerPlatform.Web.Areas.BroadcastBot
{
    public class BroadcastBotAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "BroadcastBot";

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "BroadcastBot_default",
                "BroadcastBot/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}