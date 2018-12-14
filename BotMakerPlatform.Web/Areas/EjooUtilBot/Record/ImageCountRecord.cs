using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot.Record
{
    public class ImageCountRecord
    {
        public int BotInstanceId { get; set; }

        public int EditingMessageId { get; set; }

        public long ChatId { get; set; }
    }
}