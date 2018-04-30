using System.Collections.Generic;

namespace BotMakerPlatform.Web.Areas.SupportBot.Models
{
    public class HomeViewModel
    {
        public List<SubscriberViewModel> Subscribers { get; set; }

        public string WelcomeMessage { get; set; }
    }
}