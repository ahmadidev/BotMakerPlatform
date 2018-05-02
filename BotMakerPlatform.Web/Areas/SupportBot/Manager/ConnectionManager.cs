using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Areas.SupportBot.Manager
{
    public class ConnectionManager
    {
        private SupporterRepo SupporterRepo { get; }
        private ConnectionRepo ConnectionRepo { get; }
        private ITelegramBotClient TelegramClient { get; }
        private ConnectionNotifier ConnectionNotifier { get; }

        public ConnectionManager(
            SupporterRepo supporterRepo,
            ConnectionRepo connectionRepo,
            ITelegramBotClient telegramClient,
            ConnectionNotifier connectionNotifier)
        {
            SupporterRepo = supporterRepo;
            ConnectionRepo = connectionRepo;
            TelegramClient = telegramClient;
            ConnectionNotifier = connectionNotifier;
        }

        public bool TryConnect(Subscriber customer)
        {
            var supporters = SupporterRepo.GetAll();
            var connections = ConnectionRepo.GetAll();

            if (supporters.Count() > connections.Count())
            {
                var supporter = FairSelectSupporter();
                ConnectionRepo.Add(supporter, customer);

                TelegramClient.SendTextMessageAsync(supporter.ChatId, $"You're Connected to {customer.FirstName} {customer.LastName}");
                TelegramClient.SendTextMessageAsync(customer.ChatId, "You're Connected.");

                return true;
            }

            return false;
        }

        public void Direct(Subscriber subscriber, Update update)
        {
            var partyChatId = ConnectionRepo.FindPartyChatId(subscriber);

            if (partyChatId != default(long))
            {
                var message = update.Message;
                string fileId;

                switch (message.Type)
                {
                    case MessageType.TextMessage:
                        if (message.Text != null)
                            TelegramClient.SendTextMessageAsync(partyChatId, message.Text);
                        break;
                    case MessageType.PhotoMessage:
                        fileId = message.Photo.Last().FileId;
                        TelegramClient.SendPhotoAsync(partyChatId, new FileToSend(fileId), message.Caption ?? "");
                        break;
                    case MessageType.AudioMessage:
                        fileId = message.Audio.FileId;
                        TelegramClient.SendAudioAsync(partyChatId, new FileToSend(fileId), message.Caption ?? "",
                            message.Audio.Duration, message.Audio.Performer ?? "", message.Audio.Title ?? "");
                        break;
                    case MessageType.VideoMessage:
                        fileId = message.Video.FileId;
                        TelegramClient.SendVideoAsync(partyChatId, new FileToSend(fileId), caption: message.Caption ?? "");
                        break;
                    case MessageType.VoiceMessage:
                        fileId = message.Voice.FileId;
                        TelegramClient.SendVoiceAsync(partyChatId, new FileToSend(fileId), message.Caption ?? "");
                        break;
                    case MessageType.DocumentMessage:
                        fileId = message.Document.FileId;
                        TelegramClient.SendDocumentAsync(partyChatId, new FileToSend(fileId), message.Caption ?? "");
                        break;
                    case MessageType.StickerMessage:
                        fileId = message.Sticker.FileId;
                        TelegramClient.SendStickerAsync(partyChatId, new FileToSend(fileId));
                        break;
                    case MessageType.LocationMessage:
                        TelegramClient.SendLocationAsync(partyChatId, message.Location.Latitude, message.Location.Longitude);
                        break;
                    case MessageType.VideoNoteMessage:
                        fileId = message.VideoNote.FileId;
                        TelegramClient.SendVideoNoteAsync(partyChatId, new FileToSend(fileId),
                            message.VideoNote.Duration, message.VideoNote.Length);
                        break;
                    case MessageType.ContactMessage:
                    case MessageType.ServiceMessage:
                    case MessageType.VenueMessage:
                    case MessageType.GameMessage:
                    case MessageType.Invoice:
                    case MessageType.SuccessfulPayment:
                    case MessageType.UnknownMessage:
                        TelegramClient.SendTextMessageAsync(subscriber.ChatId, $"Message type {message.Type} is not supported.");
                        break;
                }
            }
            else
                TelegramClient.SendTextMessageAsync(subscriber.ChatId, "You have no current session.");
        }

        public void Disconnect(Subscriber customer)
        {
            var supporterChatId = ConnectionRepo.FindPartyChatId(customer);

            if (supporterChatId != default(long))
            {
                ConnectionRepo.RemoveByCustomer(customer);

                TelegramClient.SendTextMessageAsync(supporterChatId, $"You're Disconnected from {customer.FirstName} {customer.LastName}");
                TelegramClient.SendTextMessageAsync(customer.ChatId, "You're Disconnected.");

                ConnectionNotifier.CustomerDisconnected();
            }
            else
            {
                TelegramClient.SendTextMessageAsync(customer.ChatId, "No session to end.");
            }
        }

        private Subscriber FairSelectSupporter()
        {
            var supporters = SupporterRepo.GetAll();
            var connections = ConnectionRepo.GetAll();

            //TODO: Fair select
            return supporters.FirstOrDefault(x => connections.All(c => c.SupporterChatId != x.ChatId));
        }

        public bool HasCustomerConnection(Subscriber customer)
        {
            return ConnectionRepo.FindPartyChatId(customer) != default(long);
        }
    }
}