namespace BotMakerPlatform.Web.Models
{
    public class UserBot
    {
        public int BotId { get; set; }

        public string BotUsername { get; set; }

        public string BotUniqueName { get; set; }

        public string UserId { get; set; }

        public string Token { get; set; }

        public string WebhookSecret { get; set; }
    }
}