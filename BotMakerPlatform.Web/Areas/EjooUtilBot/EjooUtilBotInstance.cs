using System;
using System.IO;
using System.Linq;
using System.Web.Mvc.Routing.Constraints;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Repo;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using BotMakerPlatform.Web.Repo;
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Element;
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
        public const string FlushCommand = "/flush";

        public static IReplyMarkup StartReplyMarkup => new ReplyKeyboardMarkup(new KeyboardButton[] { StartCommand });
        public static IReplyMarkup FlusMarkup => new ReplyKeyboardMarkup(new KeyboardButton[] { FlushCommand });
    }

    public class EjooUtilBotInstance : IBotInstance
    {
        public int Id { get; set; }
        public string Username { get; set; }

        private ImagesQueueRepo ImagesQueueRepo { get; }
        private SettingRepo SettingRepo { get; }
        private ITelegramBotClient TelegramClient { get; }

        public EjooUtilBotInstance(
            SettingRepo settingRepo,
            ImagesQueueRepo imagesQueueRepo,
            ITelegramBotClient telegramClient)
        {
            SettingRepo = settingRepo;
            ImagesQueueRepo = imagesQueueRepo;
            TelegramClient = telegramClient;
        }

        public void Update(Update update, SubscriberRecord subscriberRecord)
        {
            if (update.Type != UpdateType.Message)
                return;

            switch (update.Message.Type)
            {
                case MessageType.Text:
                    HandleTextMessage(update, subscriberRecord);
                    break;
                case MessageType.Photo:
                    AddImage(update, subscriberRecord);
                    break;
                default:
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, $"Message Type = {update.Message.Type.ToString()}");
                    break;
            }
        }

        private void HandleTextMessage(Update update, SubscriberRecord subscriberRecord)
        {
            switch (update.Message.Text)
            {
                case Keyboards.StartCommand:
                    const string defaultWelcomeMessage = "1.Send Images\n2.Use /flush to get you nice pdf :)";
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, defaultWelcomeMessage, replyMarkup: Keyboards.FlusMarkup);
                    break;
                case Keyboards.FlushCommand:
                    Flush(update, subscriberRecord);
                    break;
            }
        }

        private void AddImage(Update update, SubscriberRecord subscriberRecord)
        {
            ImagesQueueRepo.Add(subscriberRecord, update.Message.Photo[update.Message.Photo.Length - 1], update.Message.MessageId);
        }

        private void Flush(Update update, SubscriberRecord subscriberRecord)
        {
            var currentSessionImages = ImagesQueueRepo.GetCurrentSessionImages(subscriberRecord).ToArray();

            if (!currentSessionImages.Any())
            {
                TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "Please send some images first.");
                return;
            }

            TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, $"Processing { currentSessionImages.Length } Images...");
            var progreessMsgId = TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, GetProgressString(0, 10)).Result.MessageId;

            var inImageStream = new MemoryStream();
            var file = TelegramClient.GetInfoAndDownloadFileAsync(currentSessionImages[0].PhotoSize.FileId, inImageStream).Result;
            var standardImage = new Image(ImageDataFactory.Create(inImageStream.ToArray()));
            inImageStream.Close();

            var memStream = new MemoryStream();
            using (var pdfWriter = new PdfWriter(memStream))
            using (var pdfDoc = new PdfDocument(pdfWriter))
            using (var document = new iText.Layout.Document(pdfDoc, new PageSize(standardImage.GetImageWidth(), standardImage.GetImageHeight())))
            {
                for (var i = 0; i < currentSessionImages.Length; i++)
                {
                    var currentSessionImage = currentSessionImages[i];
                    var imageStream = new MemoryStream();
                    var result = TelegramClient.GetInfoAndDownloadFileAsync(currentSessionImage.PhotoSize.FileId, imageStream).Result;

                    var image = new Image(ImageDataFactory.Create(imageStream.ToArray()));
                    pdfDoc.AddNewPage(new PageSize(image.GetImageWidth(), image.GetImageHeight()));
                    image.SetFixedPosition(i + 1, 0, 0);
                    document.Add(image);
                    TelegramClient.EditMessageTextAsync(subscriberRecord.ChatId, progreessMsgId,
                        GetProgressString(i + 1, currentSessionImages.Length));
                }
            }


            TelegramClient.EditMessageTextAsync(subscriberRecord.ChatId, progreessMsgId, "Uploading File...");
            SendPdf(subscriberRecord, memStream);
            memStream.Close();
            ImagesQueueRepo.ClearCurrentSessionImages(subscriberRecord);
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