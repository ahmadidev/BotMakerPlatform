using System.Web.Mvc;

namespace BotMakerPlatform.Web.Areas.StoreBot
{
    public class StoreBotAreaRegistration : AreaRegistration
    {
        public override string AreaName => "StoreBot";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "StoreBot_default",
                "StoreBot/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}