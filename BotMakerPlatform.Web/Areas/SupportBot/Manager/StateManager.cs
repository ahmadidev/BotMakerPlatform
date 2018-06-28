using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotMakerPlatform.Web.Areas.SupportBot.Manager
{
    public class StateManager
    {
        public class Keyboards
        {
            public const string StartCommand = "/start";
            public const string ConnectCommand = "/connect";
            public const string DisconnectCommand = "/end";
            public const string CancelCommand = "/cancel";

            public static IReplyMarkup Connected => new ReplyKeyboardMarkup(new KeyboardButton[] { DisconnectCommand });
            public static IReplyMarkup NotConnected => new ReplyKeyboardMarkup(new KeyboardButton[] { ConnectCommand });
            public static IReplyMarkup InQueue => new ReplyKeyboardMarkup(new KeyboardButton[] { CancelCommand });
            public static IReplyMarkup Empty => new ReplyKeyboardRemove();
        }

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
                return Keyboards.Connected;
            else if (WaitingQueueRepo.HasWaiter(customer))
                return Keyboards.InQueue;
            else
                return Keyboards.NotConnected;
        }

        public IReplyMarkup GetSupporterReplyKeyboardMarkup(Subscriber subscriber)
        {
            return Keyboards.Empty;
        }
    }
}