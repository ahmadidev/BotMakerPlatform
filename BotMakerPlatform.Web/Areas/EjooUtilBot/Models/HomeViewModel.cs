using System.Collections.Generic;
using BotMakerPlatform.Web.Repo;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot.Models
{
    public class HomeViewModel
    {
        public IList<SubscriberRecord> Subscribers { get; set; }
    }
}