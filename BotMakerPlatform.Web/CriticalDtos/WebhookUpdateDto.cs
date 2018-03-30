using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.CriticalDtos
{
    public class WebhookUpdateDto
    {
        public Update Update { get; set; }
        public int BotInstanceId { get; set; }
        public string Secret { get; set; }
    }
}