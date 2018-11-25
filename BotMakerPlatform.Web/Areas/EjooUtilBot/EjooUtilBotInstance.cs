using BotMakerPlatform.Web.Areas.SupportBot.Manager;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using BotMakerPlatform.Web.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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


        public EjooUtilBotInstance(
            SettingRepo settingRepo,
            ITelegramBotClient telegramClient)
        {
            SettingRepo = settingRepo;
            TelegramClient = telegramClient;
        }

        public void Update(Update update, Subscriber subscriber)
        {
            if (update.Type != UpdateType.Message)
                return;

            TelegramClient.SendTextMessageAsync(subscriber.ChatId, $"سلام {subscriber.FirstName}\nدر حال توسعه ایم...");

            if (update.Message.Type == MessageType.Photo)
            {
                for (int i = 0; i < update.Message.Photo.Length; i++)
                {

                    var fileId = update.Message.Photo[i].FileId;
                    var file = TelegramClient.GetFileAsync(fileId).Result;
                    //TODO: get token
                    var url = "https://api.telegram.org/file/bot" + "Kossheer Token" + "/" + file.FilePath;
                    // PDF = Renderer.RenderUrlAsPdf("")
                }

                //TelegramClient.SendDocumentAsync(subscriber.ChatId, new InputOnlineFile(PDF.Stream));
            }
        }
    }
}