using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BotMakerPlatform.Web.Areas.StoreBot.Models;
using BotMakerPlatform.Web.Areas.StoreBot.Record;
using BotMakerPlatform.Web.Areas.StoreBot.Repo;
using BotMakerPlatform.Web.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotMakerPlatform.Web.Areas.StoreBot
{
    public class StateManager
    {
        public class Keyboards
        {
            public const string StartCommand = "/start";
            public const string NewProductCommand = "محصول جدید";
            public const string ListProductsCommand = "لیست محصولات";
            public const string CancelCommand = "لغو";
            public const string AddProductSkip = "رد کردن";
            public const string AddProductSubmit = "ثبت";

            public static IReplyMarkup StartAdmin => new ReplyKeyboardMarkup(new KeyboardButton[] { NewProductCommand, ListProductsCommand }, resizeKeyboard: true);
            public static IReplyMarkup StartUser => new ReplyKeyboardMarkup(new KeyboardButton[] { ListProductsCommand }, resizeKeyboard: true);
            public static IReplyMarkup AddingProductAdmin => new ReplyKeyboardMarkup(new KeyboardButton[] { CancelCommand, AddProductSkip }, resizeKeyboard: true);
            public static IReplyMarkup AddingProductImagesAdmin => new ReplyKeyboardMarkup(new KeyboardButton[] { CancelCommand, AddProductSubmit }, resizeKeyboard: true);
            public static IReplyMarkup Empty => new ReplyKeyboardRemove();
        }
    }

    public class StoreBotInstance : IBotInstance
    {
        public int Id { get; set; }
        public string Username { get; set; }

        private ITelegramBotClient TelegramClient { get; }
        private SettingRepo SettingRepo { get; }
        private StoreAdminRepo StoreAdminRepo { get; }

        public enum NewProductSteps
        {
            Begin,
            Name,
            Code,
            Price,
            Desciption,
            Images
        }

        public class NewProductInState
        {
            public NewProductInState()
            {
                NewProductStep = NewProductSteps.Begin;
                ProductRecord = new StoreProductRecord();
            }

            public NewProductSteps NewProductStep { get; set; }
            public StoreProductRecord ProductRecord { get; set; }
        }

        private static readonly Dictionary<long, NewProductInState> NewProductStates = new Dictionary<long, NewProductInState>();

        public StoreBotInstance(ITelegramBotClient telegramClient, SettingRepo settingRepo, StoreAdminRepo storeAdminRepo)
        {
            TelegramClient = telegramClient;
            SettingRepo = settingRepo;
            StoreAdminRepo = storeAdminRepo;
        }

        public void Update(Update update, SubscriberRecord subscriberRecord)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                HandleCallbackQuery(update);

                return;
            }

            if (update.Type != UpdateType.Message)
            {
                TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, $"ببخشید، فعلا این چیزا ({update.Type}) رو متوجه نمیشم");
                return;
            }

            var isAdmin = StoreAdminRepo.GetAdmin(subscriberRecord.ChatId) != null;

            switch (update.Message.Text)
            {
                case StateManager.Keyboards.StartCommand:
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, SettingRepo.Load<Setting>().WelcomeMessage, parseMode: ParseMode.Markdown, replyMarkup: isAdmin ? StateManager.Keyboards.StartAdmin : StateManager.Keyboards.StartUser);
                    break;
                case StateManager.Keyboards.NewProductCommand:
                    if (!isAdmin)
                    {
                        TelegramClient.SendTextMessageAsync(update.Message.Chat.Id, "متاسفانه مجوز اجرای این دستور را ندارید.");
                        return;
                    }

                    if (!NewProductStates.TryGetValue(subscriberRecord.ChatId, out _))
                        NewProductStates.Add(subscriberRecord.ChatId, null);

                    NewProductStates[subscriberRecord.ChatId] = new NewProductInState();
                    NewProductStates[subscriberRecord.ChatId].ProductRecord.BotInstanceRecordId = Id;

                    HandleNewProductMessage(update, subscriberRecord);
                    break;
                case StateManager.Keyboards.ListProductsCommand:
                    using (var db = new Db())
                    {
                        var products = db.StoreProductRecords.AsNoTracking().Where(x => x.BotInstanceRecordId == Id);

                        if (!products.Any())
                        {
                            TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "محصولی برای نمایش وجود ندارد.");
                            return;
                        }

                        var i = 0;
                        var productDetailTemplate = SettingRepo.Load<Setting>().ProductDetailTemplate;

                        foreach (var product in products)
                        {
                            var detail = productDetailTemplate
                                .Replace("[Index]", (++i).ToString())
                                .Replace("[Name]", product.Name)
                                .Replace("[Code]", product.Code)
                                .Replace("[Price]", product.Price.ToCurrency())
                                .Replace("[Description]", product.Description);

                            var buttons = new List<InlineKeyboardButton>
                            {
                                InlineKeyboardButton.WithCallbackData($"تصاویر", "image:" + product.Id)
                            };

                            if (isAdmin)
                                buttons.Add(InlineKeyboardButton.WithCallbackData("حذف", "delete:" + product.Id));

                            Task.Delay(i * 500).ContinueWith(task =>
                            {
                                TelegramClient.SendPhotoAsync(
                                    subscriberRecord.ChatId,
                                    product.ImageFileRecords.First().ImageFileId,
                                    detail,
                                    parseMode: ParseMode.Markdown,
                                    replyMarkup: new InlineKeyboardMarkup(buttons));
                            });
                        }
                    }
                    break;
                case StateManager.Keyboards.CancelCommand:
                    if (NewProductStates.TryGetValue(subscriberRecord.ChatId, out _))
                        NewProductStates.Remove(subscriberRecord.ChatId);

                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "عملیات فعلی لغو شد", replyMarkup: StateManager.Keyboards.StartAdmin);
                    break;
                default:
                    if (NewProductStates.TryGetValue(subscriberRecord.ChatId, out _))
                    {
                        if (!isAdmin)
                        {
                            TelegramClient.SendTextMessageAsync(update.Message.Chat.Id, "متاسفانه مجوز اجرای این دستور را ندارید.");
                            return;
                        }

                        HandleNewProductMessage(update, subscriberRecord);
                    }
                    else
                        TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "متوجه پیام نشدم..");
                    break;
            }
        }

        private void HandleCallbackQuery(Update update)
        {
            var parts = update.CallbackQuery.Data.Split(':');
            if (parts.Length == 2 && parts[0] == "delete" && int.TryParse(parts[1], out var productId))
            {
                var isAdmin = StoreAdminRepo.GetAdmin(update.CallbackQuery.Message.Chat.Id) != null;

                if (!isAdmin)
                {
                    TelegramClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "متاسفانه مجوز اجرای این دستور را ندارید.");
                    return;
                }

                using (var db = new Db())
                {
                    var product = db.StoreProductRecords.SingleOrDefault(x => x.Id == productId);

                    if (product != null)
                    {
                        db.StoreProductRecords.Remove(product);
                        db.SaveChanges();
                    }

                    TelegramClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "محصول با موفقیت حذف شد.");
                    TelegramClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
                }
            }
        }

        private void HandleNewProductMessage(Update update, SubscriberRecord subscriberRecord)
        {
            switch (NewProductStates[subscriberRecord.ChatId].NewProductStep)
            {
                case NewProductSteps.Begin:
                    NewProductStates[subscriberRecord.ChatId].NewProductStep = NewProductSteps.Name;
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "نام محصول:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Name:
                    if (update.Message.Text == StateManager.Keyboards.AddProductSkip)
                    {
                        TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "برای کارکرد بهتر بات لطفا نام محصول را وارد نمایید:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                        return;
                    }

                    NewProductStates[subscriberRecord.ChatId].ProductRecord.Name = update.Message.Text;
                    NewProductStates[subscriberRecord.ChatId].NewProductStep = NewProductSteps.Code;
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "کد:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Code:
                    if (update.Message.Text != StateManager.Keyboards.AddProductSkip)
                        NewProductStates[subscriberRecord.ChatId].ProductRecord.Code = update.Message.Text;

                    NewProductStates[subscriberRecord.ChatId].NewProductStep = NewProductSteps.Price;
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "قیمت:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Price:
                    if (update.Message.Text != StateManager.Keyboards.AddProductSkip)
                    {
                        if (!int.TryParse(update.Message.Text, out var price))
                        {
                            TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "قیمت وارد شده صحیح نیست. یک مقدار عددی وارد کنید.", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                            return;
                        }

                        NewProductStates[subscriberRecord.ChatId].ProductRecord.Price = price;
                    }

                    NewProductStates[subscriberRecord.ChatId].NewProductStep = NewProductSteps.Desciption;
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "توضیحات:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Desciption:
                    if (update.Message.Text != StateManager.Keyboards.AddProductSkip)
                        NewProductStates[subscriberRecord.ChatId].ProductRecord.Description = update.Message.Text;

                    NewProductStates[subscriberRecord.ChatId].NewProductStep = NewProductSteps.Images;
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "تصاویر:", replyMarkup: StateManager.Keyboards.AddingProductImagesAdmin);
                    break;
                case NewProductSteps.Images:
                    if (StateManager.Keyboards.AddProductSubmit == update.Message.Text)
                    {
                        var hasImage = NewProductStates[subscriberRecord.ChatId].ProductRecord.ImageFileRecords.Any();

                        if (!hasImage)
                        {
                            TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "برای کارکرد بهتر بات حداقل یک تصویر ارسال نمایید", replyMarkup: StateManager.Keyboards.AddingProductImagesAdmin);
                            return;
                        }

                        using (var db = new Db())
                        {
                            db.StoreProductRecords.Add(NewProductStates[subscriberRecord.ChatId].ProductRecord);
                            db.SaveChanges();
                        }

                        TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "محصول شما با موفقیت ثبت گردید", replyMarkup: StateManager.Keyboards.StartAdmin);
                        NewProductStates.Remove(subscriberRecord.ChatId);

                        return;
                    }

                    if (update.Message.Type != MessageType.Photo)
                    {
                        TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "لطفا تصویر ارسال نمایید", replyMarkup: StateManager.Keyboards.AddingProductImagesAdmin);
                        return;
                    }

                    NewProductStates[subscriberRecord.ChatId].ProductRecord.ImageFileRecords.Add(
                        new ImageFileRecord
                        {
                            ImageFileId = update.Message.Photo.Last().FileId,
                            StoreProductRecordId = Id
                        });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}