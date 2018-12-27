using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web
{
    public class Dumper
    {
        public ITelegramBotClient TelegramClient = new TelegramBotClient("722020775:AAF5-JEI2W8WWIyddF_VaJ731wylIJW9a8Y");

        /// <summary>
        /// How to resolve chatId of Dump channel https://stackoverflow.com/a/33862907
        /// </summary>
        public static readonly ChatId ChatId = new ChatId(-1001469430239);

        public static Dumper Instance() => new Dumper();
    }
}