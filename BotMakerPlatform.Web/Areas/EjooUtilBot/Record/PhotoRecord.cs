using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot.Record
{
    public class PhotoRecord
    {
        public int BotInstanceId { get; set; }

        public PhotoSize PhotoSize { get; set; }

        public int MessageId { get; set; }

        public long ChatId { get; set; }
    }
}