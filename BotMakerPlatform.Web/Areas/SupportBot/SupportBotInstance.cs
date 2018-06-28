using BotMakerPlatform.Web.Areas.SupportBot.Manager;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class SupportBotInstance : IBotInstance
    {
        public int Id { get; set; }

        private WaitingManager WaitingManager { get; }
        private SupporterRepo SupporterRepo { get; }
        private SettingRepo SettingRepo { get; }
        private ConnectionManager ConnectionManager { get; }
        private StateManager StateManager { get; }
        private ITelegramBotClient TelegramClient { get; }

        public SupportBotInstance(
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
            if (update.Type != UpdateType.MessageUpdate)
                return;

            if (SupporterRepo.IsSupporter(subscriber))
                HandleSupporterMessage(update, subscriber);
            else
                HandleCustomerMessage(update, subscriber);
        }

        private void HandleSupporterMessage(Update update, Subscriber supporter)
        {
            ConnectionManager.Direct(supporter, update);
        }

        private void HandleCustomerMessage(Update update, Subscriber customer)
        {
            switch (update.Message.Text)
            {
                case StateManager.Keyboards.StartCommand:
                    const string defaultWelcomeMessage = "Welcome to your support!\nWe never leave you alone😊";
                    TelegramClient.SendTextMessageAsync(customer.ChatId, SettingRepo.GetWelcomeMessage() ?? defaultWelcomeMessage,
                        replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));
                    break;
                case StateManager.Keyboards.ConnectCommand:
                    WaitingManager.AddToQueue(customer);
                    break;
                case StateManager.Keyboards.CancelCommand:
                    WaitingManager.Cancel(customer);
                    break;
                case StateManager.Keyboards.DisconnectCommand:
                    ConnectionManager.Disconnect(customer);
                    break;
                default:
                    ConnectionManager.Direct(customer, update);
                    break;
            }
        }
    }
}