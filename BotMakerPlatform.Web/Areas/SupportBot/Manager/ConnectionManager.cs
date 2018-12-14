using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using BotMakerPlatform.Web.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace BotMakerPlatform.Web.Areas.SupportBot.Manager
{
    public class ConnectionManager
    {
        private SupporterRepo SupporterRepo { get; }
        private ConnectionRepo ConnectionRepo { get; }
        private ITelegramBotClient TelegramClient { get; }
        private ConnectionNotifier ConnectionNotifier { get; }
        private StateManager StateManager { get; }

        public ConnectionManager(
            SupporterRepo supporterRepo,
            ConnectionRepo connectionRepo,
            StateManager stateManager,
            ITelegramBotClient telegramClient,
            ConnectionNotifier connectionNotifier)
        {
            SupporterRepo = supporterRepo;
            ConnectionRepo = connectionRepo;
            StateManager = stateManager;
            TelegramClient = telegramClient;
            ConnectionNotifier = connectionNotifier;
        }

        public bool TryConnect(SubscriberRecord customer)
        {
            var supporters = SupporterRepo.GetAll();
            var connections = ConnectionRepo.GetAll();

            if (supporters.Count() > connections.Count())
            {
                var supporter = FairSelectSupporter();
                ConnectionRepo.Add(supporter, customer);

                TelegramClient.SendTextMessageAsync(supporter.ChatId, $"You're Connected to {customer.FirstName} {customer.LastName}",
                    replyMarkup: StateManager.GetSupporterReplyKeyboardMarkup(supporter));

                TelegramClient.SendTextMessageAsync(customer.ChatId, "You're Connected.",
                    replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));

                return true;
            }

            return false;
        }

        public void Direct(SubscriberRecord subscriberRecord, Update update)
        {
            var partyChatId = ConnectionRepo.FindPartyChatId(subscriberRecord);

            var replyKeyboardMarkup = SupporterRepo.IsSupporter(subscriberRecord) ?
                StateManager.GetCustomerReplyKeyboardMarkup(subscriberRecord) :
                StateManager.GetSupporterReplyKeyboardMarkup(subscriberRecord);

            if (partyChatId != default(long))
            {
                var message = update.Message;
                string fileId;

                switch (message.Type)
                {
                    case MessageType.Text:
                        if (message.Text != null)
                            TelegramClient.SendTextMessageAsync(partyChatId, message.Text, replyMarkup: replyKeyboardMarkup);
                        break;
                    case MessageType.Photo:
                        fileId = message.Photo.Last().FileId;
                        TelegramClient.SendPhotoAsync(partyChatId, new InputOnlineFile(fileId), message.Caption ?? "",
                            replyMarkup: replyKeyboardMarkup);
                        break;
                    case MessageType.Audio:
                        fileId = message.Audio.FileId;
                        TelegramClient.SendAudioAsync(partyChatId, new InputOnlineFile(fileId), message.Caption ?? "", ParseMode.Default,
                            message.Audio.Duration, message.Audio.Performer ?? "", message.Audio.Title ?? "",
                            replyMarkup: replyKeyboardMarkup);
                        break;
                    case MessageType.Video:
                        fileId = message.Video.FileId;
                        TelegramClient.SendVideoAsync(partyChatId, new InputOnlineFile(fileId), caption: message.Caption ?? "",
                            replyMarkup: replyKeyboardMarkup);
                        break;
                    case MessageType.Voice:
                        fileId = message.Voice.FileId;
                        TelegramClient.SendVoiceAsync(partyChatId, new InputOnlineFile(fileId), message.Caption ?? "",
                            replyMarkup: replyKeyboardMarkup);
                        break;
                    case MessageType.Document:
                        fileId = message.Document.FileId;
                        TelegramClient.SendDocumentAsync(partyChatId, new InputOnlineFile(fileId), message.Caption ?? "",
                            replyMarkup: replyKeyboardMarkup);
                        break;
                    case MessageType.Sticker:
                        fileId = message.Sticker.FileId;
                        TelegramClient.SendStickerAsync(partyChatId, new InputOnlineFile(fileId),
                            replyMarkup: replyKeyboardMarkup);
                        break;
                    case MessageType.Location:
                        TelegramClient.SendLocationAsync(partyChatId, message.Location.Latitude, message.Location.Longitude,
                            replyMarkup: replyKeyboardMarkup);
                        break;
                    case MessageType.VideoNote:
                        fileId = message.VideoNote.FileId;
                        TelegramClient.SendVideoNoteAsync(partyChatId, new InputOnlineFile(fileId),
                            message.VideoNote.Duration, message.VideoNote.Length, replyMarkup: replyKeyboardMarkup);
                        break;
                    case MessageType.Contact:
                    case MessageType.Venue:
                    case MessageType.Game:
                    case MessageType.Invoice:
                    case MessageType.SuccessfulPayment:
                    case MessageType.Unknown:
                        TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId,
                            $"Message type {message.Type} is not supported.", replyMarkup: replyKeyboardMarkup);
                        break;
                    case MessageType.WebsiteConnected:
                    case MessageType.ChatMembersAdded:
                    case MessageType.ChatMemberLeft:
                    case MessageType.ChatTitleChanged:
                    case MessageType.ChatPhotoChanged:
                    case MessageType.MessagePinned:
                    case MessageType.ChatPhotoDeleted:
                    case MessageType.GroupCreated:
                    case MessageType.SupergroupCreated:
                    case MessageType.ChannelCreated:
                    case MessageType.MigratedToSupergroup:
                    case MessageType.MigratedFromGroup:
                    default:
                        break;
                }
            }
            else
            {
                TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "You have no current session.", replyMarkup: replyKeyboardMarkup);
            }
        }

        public void Disconnect(SubscriberRecord customer)
        {
            var supporterChatId = ConnectionRepo.FindPartyChatId(customer);

            if (supporterChatId != default(long))
            {
                ConnectionRepo.RemoveByCustomer(customer);

                TelegramClient.SendTextMessageAsync(supporterChatId, $"You're Disconnected from {customer.FirstName} {customer.LastName}");
                TelegramClient.SendTextMessageAsync(customer.ChatId, "You're Disconnected.",
                    replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));

                ConnectionNotifier.CustomerDisconnected();
            }
            else
            {
                TelegramClient.SendTextMessageAsync(customer.ChatId, "No session to end.",
                    replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));
            }
        }

        private SubscriberRecord FairSelectSupporter()
        {
            var supporters = SupporterRepo.GetAll();
            var connections = ConnectionRepo.GetAll();

            //TODO: Fair select
            return supporters.FirstOrDefault(x => connections.All(c => c.SupporterChatId != x.ChatId));
        }

        public bool HasCustomerConnection(SubscriberRecord customer)
        {
            return ConnectionRepo.FindPartyChatId(customer) != default(long);
        }
    }
}