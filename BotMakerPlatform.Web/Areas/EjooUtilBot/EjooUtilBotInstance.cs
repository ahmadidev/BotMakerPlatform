using BotMakerPlatform.Web.Areas.SupportBot.Manager;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot
{
    public class EjooUtilBotInstance : IBotInstance
    {
        public int Id { get; set; }

        private WaitingManager WaitingManager { get; }
        private SupporterRepo SupporterRepo { get; }
        private SettingRepo SettingRepo { get; }
        private ConnectionManager ConnectionManager { get; }
        private StateManager StateManager { get; }
        private ITelegramBotClient TelegramClient { get; }


        private IronPdf.HtmlToPdf Renderer { get; }

        public EjooUtilBotInstance(
            SettingRepo settingRepo,
            ITelegramBotClient telegramClient)
        {
            SettingRepo = settingRepo;
            TelegramClient = telegramClient;
            Renderer = new IronPdf.HtmlToPdf();
        }

        public void Update(Update update, Subscriber subscriber)
        {
            if (update.Type != UpdateType.Message)
                return;

            TelegramClient.SendTextMessageAsync(subscriber.ChatId, $"سلام {subscriber.FirstName}\nدر حال توسعه ایم...");

            var PDF = Renderer.RenderUrlAsPdf("https://en.wikipedia.org/wiki/Portable_Document_Format");

            if (update.Message.Type == MessageType.Photo)
            {
                TelegramClient.SendTextMessageAsync(subscriber.ChatId, "Photooooo");
                TelegramClient.SendDocumentAsync(subscriber.ChatId, new InputOnlineFile(PDF.Stream));
            }
        }
    }
}