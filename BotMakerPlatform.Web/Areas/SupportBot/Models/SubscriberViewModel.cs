namespace BotMakerPlatform.Web.Areas.SupportBot.Models
{
    public class SubscriberViewModel
    {
        public long ChatId { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsSupporter { get; set; }
    }
}