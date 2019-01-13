using System;
using System.IO;
using System.Linq;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Record;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Repo;
using BotMakerPlatform.Web.Controllers;
using BotMakerPlatform.Web.Repo;
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot
{
    public class Keyboards
    {
        public const string StartCommand = "/start";
        public const string FlushCommand = "دریافت فایل ⬇️";
        public const string BroadcastCommand = "/broadcast";

        public static IReplyMarkup StartReplyMarkup => new ReplyKeyboardMarkup(new KeyboardButton[] { StartCommand }, resizeKeyboard: true);
        public static IReplyMarkup FlushMarkup => new ReplyKeyboardMarkup(new KeyboardButton[] { FlushCommand }, resizeKeyboard: true);
    }

    public class EjooUtilBotInstance : IBotInstance
    {
        public int BotInstanceId { get; set; }
        public string Username { get; set; }

        private ItemsQueueRepo ItemsQueueRepo { get; }
        private SubscriberRepo SubscriberRepo { get; }
        private ITelegramBotClient TelegramClient { get; }

        private static long _broadcastingSubscriberChatId;

        public EjooUtilBotInstance(
            ItemsQueueRepo itemsQueueRepo,
            SubscriberRepo subscriberRepo,
            ITelegramBotClient telegramClient)
        {
            ItemsQueueRepo = itemsQueueRepo;
            SubscriberRepo = subscriberRepo;
            TelegramClient = telegramClient;
        }

        public void Update(Update update, SubscriberRecord subscriberRecord)
        {
            if (update.Type != UpdateType.Message)
                return;

            if (IsAdmin(subscriberRecord) && subscriberRecord.ChatId == _broadcastingSubscriberChatId)
            {
                foreach (var record in SubscriberRepo.GetAll())
                    SendMeesage(update, subscriberRecord, record.ChatId);

                _broadcastingSubscriberChatId = 0;
            }

            switch (update.Message.Type)
            {
                case MessageType.Text:
                    HandleTextMessage(update, subscriberRecord);
                    break;
                case MessageType.Document:
                case MessageType.Photo:
                    AddItem(update, subscriberRecord);
                    break;
            }
        }

        private static bool IsAdmin(SubscriberRecord subscriberRecord)
        {
            return subscriberRecord.Username == "ahmadierfan" || subscriberRecord.Username == "ahmadidev";
        }

        private void HandleTextMessage(Update update, SubscriberRecord subscriberRecord)
        {
            switch (update.Message.Text)
            {
                case Keyboards.BroadcastCommand:
                    if (IsAdmin(subscriberRecord))
                    {
                        TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId,
                            "پیام مورد نظر را برای انتشار ارسال کنید.",
                            replyMarkup: Keyboards.FlushMarkup, disableNotification: true);
                        _broadcastingSubscriberChatId = subscriberRecord.ChatId;
                    }
                    break;
                case Keyboards.StartCommand:
                    const string defaultWelcomeMessage = "1.عکس ها را بفرستید\n2.دکمه را بزنید";
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, defaultWelcomeMessage,
                        replyMarkup: Keyboards.FlushMarkup);
                    break;
                case Keyboards.FlushCommand:
                    try
                    {
                        Flush(update, subscriberRecord);
                    }
                    catch (Exception exception)
                    {
                        var baseException = exception.GetBaseException();
                        Log.Error(baseException, "Error in Flush: {Message} -> {StackTrace}", baseException.Message, baseException.StackTrace);
                    }
                    break;
                default:
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, $"متوجه پیام نشدم.",
                        replyMarkup: Keyboards.FlushMarkup);
                    break;
            }
        }

        private void SendMeesage(Update update, SubscriberRecord subscriberRecord, long chatId)
        {
            string fileId;
            switch (update.Message.Type)
            {
                case MessageType.Text:
                    if (update.Message.Text != null)
                        TelegramClient.SendTextMessageAsync(chatId, update.Message.Text, disableNotification: true);
                    break;
                case MessageType.Photo:
                    fileId = update.Message.Photo.Last().FileId;
                    TelegramClient.SendPhotoAsync(chatId, new InputOnlineFile(fileId), update.Message.Caption ?? "", disableNotification: true);
                    break;
                case MessageType.Audio:
                    fileId = update.Message.Audio.FileId;
                    TelegramClient.SendAudioAsync(chatId, new InputOnlineFile(fileId), update.Message.Caption ?? "",
                        ParseMode.Default,
                        update.Message.Audio.Duration, update.Message.Audio.Performer ?? "", update.Message.Audio.Title ?? "", disableNotification: true);
                    break;
                case MessageType.Video:
                    fileId = update.Message.Video.FileId;
                    TelegramClient.SendVideoAsync(chatId, new InputOnlineFile(fileId),
                        caption: update.Message.Caption ?? "", disableNotification: true);
                    break;
                case MessageType.Voice:
                    fileId = update.Message.Voice.FileId;
                    TelegramClient.SendVoiceAsync(chatId, new InputOnlineFile(fileId), update.Message.Caption ?? "", disableNotification: true);
                    break;
                case MessageType.Document:
                    fileId = update.Message.Document.FileId;
                    TelegramClient.SendDocumentAsync(chatId, new InputOnlineFile(fileId), update.Message.Caption ?? "", disableNotification: true);
                    break;
                case MessageType.Sticker:
                    fileId = update.Message.Sticker.FileId;
                    TelegramClient.SendStickerAsync(chatId, new InputOnlineFile(fileId), disableNotification: true);
                    break;
                case MessageType.Location:
                    TelegramClient.SendLocationAsync(chatId, update.Message.Location.Latitude,
                        update.Message.Location.Longitude, disableNotification: true);
                    break;
                case MessageType.VideoNote:
                    fileId = update.Message.VideoNote.FileId;
                    TelegramClient.SendVideoNoteAsync(chatId, new InputOnlineFile(fileId),
                        update.Message.VideoNote.Duration, update.Message.VideoNote.Length, disableNotification: true);
                    break;
                case MessageType.Contact:
                case MessageType.Venue:
                case MessageType.Game:
                case MessageType.Invoice:
                case MessageType.SuccessfulPayment:
                case MessageType.Unknown:
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
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId,
                        $"Message type {update.Message.Type} is not supported.");
                    break;
            }
        }

        private void AddItem(Update update, SubscriberRecord subscriberRecord)
        {
            var item = new ItemRecord
            {
                BotInstanceId = BotInstanceId,
                MessageId = update.Message.MessageId,
                ChatId = subscriberRecord.ChatId,
            };

            if (update.Message.Type == MessageType.Photo)
            {
                item.ItemType = ItemRecord.ItemTypes.IMAGE;
                item.FileId = update.Message.Photo[update.Message.Photo.Length - 1].FileId;
                ItemsQueueRepo.Add(subscriberRecord, item);
            }
            else if (update.Message.Type == MessageType.Text)
            {
                var isUri = Uri.IsWellFormedUriString(update.Message.Text, UriKind.RelativeOrAbsolute);
                item.ItemType = ItemRecord.ItemTypes.WEB_PAGE;
                item.Text = update.Message.Text;
            }
            else if (update.Message.Type == MessageType.Document)
            {
                switch (update.Message.Document.MimeType)
                {
                    case "application/pdf":
                        item.ItemType = ItemRecord.ItemTypes.PDF_FILE;
                        item.FileId = update.Message.Document.FileId;
                        ItemsQueueRepo.Add(subscriberRecord, item);
                        break;
                    case "image/jpeg":
                    case "image/png":
                        item.ItemType = ItemRecord.ItemTypes.IMAGE;
                        item.FileId = update.Message.Document.FileId;
                        ItemsQueueRepo.Add(subscriberRecord, item);
                        break;
                }
            }
        }

        private void Flush(Update update, SubscriberRecord subscriberRecord)
        {
            var currentSessionImages = ItemsQueueRepo.GetCurrentSessionImages(subscriberRecord).ToArray();

            if (!currentSessionImages.Any())
            {
                TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "چیزی برای تبدیل به پی دی اف وجود ندارد.",
                    replyMarkup: Keyboards.FlushMarkup);
                return;
            }

            TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, $"در حال بارگزاری { currentSessionImages.Length } فایل",
                replyMarkup: Keyboards.FlushMarkup);
            var progreessMsgId = TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, GetProgressString(0, 10)).Result.MessageId;

            var memStream = new MemoryStream();
            using (var pdfWriter = new PdfWriter(memStream))
            using (var pdfDoc = new PdfDocument(pdfWriter))
            using (var document = new iText.Layout.Document(pdfDoc, new PageSize(0, 0)))
            {
                for (var i = 0; i < currentSessionImages.Length; i++)
                {
                    var currentSessionImage = currentSessionImages[i];

                    switch (currentSessionImage.ItemType)
                    {
                        case ItemRecord.ItemTypes.IMAGE:
                            {
                                var imageStream = new MemoryStream();
                                var result = TelegramClient
                                    .GetInfoAndDownloadFileAsync(currentSessionImage.FileId, imageStream).Result;

                                var image = new Image(ImageDataFactory.Create(imageStream.ToArray()));
                                pdfDoc.AddNewPage(new PageSize(image.GetImageWidth(), image.GetImageHeight()));
                                image.SetFixedPosition(pdfDoc.GetNumberOfPages(), 0, 0);
                                document.Add(image);
                            }
                            break;

                        case ItemRecord.ItemTypes.PDF_FILE:
                            {
                                try
                                {
                                    var memoryStream = new MemoryStream();
                                    var result = TelegramClient.GetInfoAndDownloadFileAsync(currentSessionImage.FileId, memoryStream).Result;

                                    var bytes = memoryStream.ToArray();
                                    var tempStream = new MemoryStream(bytes);

                                    using (var pdfReader = new PdfReader(tempStream))
                                    {
                                        using (var newPdfDoc = new PdfDocument(pdfReader))
                                        {
                                            newPdfDoc.CopyPagesTo(1,
                                                newPdfDoc.GetNumberOfPages(),
                                                pdfDoc);
                                        }
                                    }

                                    tempStream.Close();
                                    memoryStream.Close();
                                }
                                catch (Exception exception)
                                {
                                    var baseException = exception.GetBaseException();
                                    Log.Error(baseException, "Read Pdf File Failed: {Message} -> {StackTrace}", baseException.Message, baseException.StackTrace);
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    TelegramClient.EditMessageTextAsync(subscriberRecord.ChatId, progreessMsgId,
                        GetProgressString(i + 1, currentSessionImages.Length));
                }
            }


            var resultEdit = TelegramClient.EditMessageTextAsync(subscriberRecord.ChatId, progreessMsgId, "در حال آپلود فایل...").Result;
            SendPdf(subscriberRecord, memStream);
            memStream.Close();
            ItemsQueueRepo.ClearCurrentSessionImages(subscriberRecord);
        }

        private static string GetProgressString(int i, int n)
        {
            var percentage = (float)(i) / n;
            var inTen = (int)(percentage * 10);

            var spaces = new String(' ', 10 - inTen);
            var hashtags = new String('#', inTen);

            return $"[{hashtags}{spaces}]  {(percentage * 100):0.00}%";
        }

        private void SendPdf(SubscriberRecord subscriberRecord, MemoryStream memStream)
        {
            var bytes = memStream.ToArray();

            var tempStream = new MemoryStream(bytes);


            // Do not wait and close stream -> Do Continue With Close Stream
            var message = TelegramClient.SendDocumentAsync(
                subscriberRecord.ChatId,
                new InputOnlineFile(tempStream, $"Document {DateTime.Now:yyyy MMMM dd}.pdf")
                ).Result;

            tempStream.Close();
        }
    }
}