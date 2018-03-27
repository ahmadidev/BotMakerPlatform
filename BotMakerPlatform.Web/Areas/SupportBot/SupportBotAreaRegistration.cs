using System.Web.Mvc;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class SupportBotAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "SupportBot";

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SupportBot_default",
                "SupportBot/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}