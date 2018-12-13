using System;
using System.IO;
using System.Linq;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Repo;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
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
        public static IReplyMarkup NotConnected => new ReplyKeyboardMarkup(new KeyboardButton[] { FlushCommand });
    }

    public class EjooUtilBotInstance : IBotInstance
    {
        public int Id { get; set; }

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

        public void Update(Update update, Subscriber subscriber)
        {
            if (update.Type != UpdateType.Message)
                return;

            switch (update.Message.Type)
            {
                case MessageType.Text:
                    HandleTextMessage(update, subscriber);
                    break;
                case MessageType.Photo:
                    //var result = TelegramClient.SendTextMessageAsync(subscriber.ChatId, $"Image Recieved : {update.Message.MessageId}").Result;
                    AddImage(update, subscriber);
                    break;
                default:
                    TelegramClient.SendTextMessageAsync(subscriber.ChatId, $"Message Type = {update.Message.Type.ToString()}");
                    break;
            }
        }

        private void HandleTextMessage(Update update, Subscriber subscriber)
        {
            switch (update.Message.Text)
            {
                case Keyboards.StartCommand:
                    TelegramClient.SendTextMessageAsync(subscriber.ChatId, "1.Send Images\n2.Use /flush to get you nice pdf :)");
                    break;
                case Keyboards.FlushCommand:
                    Flush(update, subscriber);
                    break;
            }
        }

        private void AddImage(Update update, Subscriber subscriber)
        {
            ImagesQueueRepo.Add(subscriber, update.Message.Photo[update.Message.Photo.Length - 1], update.Message.MessageId);
        }

        private void Flush(Update update, Subscriber subscriber)
        {
            var currentSessionImages = ImagesQueueRepo.GetCurrentSessionImages(subscriber).ToArray();

            if (!currentSessionImages.Any())
            {
                TelegramClient.SendTextMessageAsync(subscriber.ChatId, "Please send some images first.");
                return;
            }

            TelegramClient.SendTextMessageAsync(subscriber.ChatId, $"Processing { currentSessionImages.Length } Images...");

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
                }
            }

            SendPdf(subscriber, memStream);
            memStream.Close();
            ImagesQueueRepo.ClearCurrentSessionImages(subscriber);
        }

        private void SendPdf(Subscriber subscriber, MemoryStream memStream)
        {
            var bytes = memStream.ToArray();
            var tempStream = new MemoryStream(bytes);

            var message = TelegramClient.SendDocumentAsync(subscriber.ChatId, new InputOnlineFile(tempStream, $"Document {DateTime.UtcNow:yyyy MMMM dd}.pdf")).Result;

            tempStream.Close();
        }

    }
}