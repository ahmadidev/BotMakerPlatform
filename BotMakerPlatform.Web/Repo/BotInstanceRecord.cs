namespace BotMakerPlatform.Web.Repo
{
    public class BotInstanceRecord
    {
        public int Id { get; set; }

        public string BotUsername { get; set; }

        public string BotUniqueName { get; set; }

        public string UserId { get; set; }

        public string Token { get; set; }

        public string WebhookSecret { get; set; }
    }
}