using BotMakerPlatform.Web.Areas.SupportBot.Manager;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
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
            WaitingManager waitingManager,
            SupporterRepo supporterRepo,
            SettingRepo settingRepo,
            ConnectionManager connectionManager,
            StateManager stateManager,
            ITelegramBotClient telegramClient)
        {
            WaitingManager = waitingManager;
            SupporterRepo = supporterRepo;
            SettingRepo = settingRepo;
            ConnectionManager = connectionManager;
            StateManager = stateManager;
            TelegramClient = telegramClient;
        }

        public void Update(Update update, Subscriber subscriber)
        {
            if (update.Type != UpdateType.Message)
                return;

            TelegramClient.SendTextMessageAsync(subscriber.ChatId, $"سلام {subscriber.FirstName}\nدر حال توسعه ایم...");
        }
    }
}