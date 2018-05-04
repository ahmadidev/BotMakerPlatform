using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotMakerPlatform.Web.Areas.SupportBot.Manager
{
    public class StateManager
    {
        public const string StartCommand = "/start";
        public const string ConnectCommand = "/connect";
        public const string DisconnectCommand = "/end";
        public const string CancelCommand = "/cancel";

        public static IReplyMarkup ConnectedKeyboardMarkup => new ReplyKeyboardMarkup(new KeyboardButton[] { DisconnectCommand }, true, true);
        public static IReplyMarkup NotConnectedKeyboardMarkup => new ReplyKeyboardMarkup(new KeyboardButton[] { ConnectCommand }, true, true);
        public static IReplyMarkup InQueueKeyboardMarkup => new ReplyKeyboardMarkup(new KeyboardButton[] { CancelCommand }, true, true);
        public static IReplyMarkup EmptyKeyboardMarkup => new ReplyKeyboardRemove();

        private WaitingQueueRepo WaitingQueueRepo { get; }
        private ConnectionRepo ConnectionRepo { get; }

        public StateManager(
            WaitingQueueRepo waitingQueueRepo,
            ConnectionRepo connectionRepo)
        {
            WaitingQueueRepo = waitingQueueRepo;
            ConnectionRepo = connectionRepo;
        }

        public IReplyMarkup GetCustomerReplyKeyboardMarkup(Subscriber customer)
        {
            if (ConnectionRepo.FindPartyChatId(customer) != default(long))
                return ConnectedKeyboardMarkup;
            else if (WaitingQueueRepo.HasWaiter(customer))
                return InQueueKeyboardMarkup;
            else
                return NotConnectedKeyboardMarkup;
        }

        public IReplyMarkup GetSupporterReplyKeyboardMarkup(Subscriber subscriber)
        {
            return EmptyKeyboardMarkup;
        }
    }
}