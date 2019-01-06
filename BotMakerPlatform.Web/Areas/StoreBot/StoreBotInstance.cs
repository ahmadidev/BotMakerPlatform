using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BotMakerPlatform.Web.Areas.StoreBot.Models;
using BotMakerPlatform.Web.Areas.StoreBot.Record;
using BotMakerPlatform.Web.Areas.StoreBot.Repo;
using BotMakerPlatform.Web.Controllers;
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
                IsEdit = false;
                NewProductStep = NewProductSteps.Begin;
                ProductRecord = new StoreProductRecord();
            }

            public bool IsEdit { get; set; }
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
                return;

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
                        var products = db.StoreProductRecords.Include(x => x.ImageFileRecords).AsNoTracking().Where(x => x.BotInstanceRecordId == Id);

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

                            var buttons = new List<List<InlineKeyboardButton>>
                            {
                                new List<InlineKeyboardButton>
                                {
                                    InlineKeyboardButton.WithCallbackData($"تصاویر ({product.ImageFileRecords.Count})", "images:" + product.Id)
                                }
                            };

                            if (isAdmin)
                                buttons.Add(GetAdminRow(product));

                            Task.Delay(i * 500)
                                .ContinueWith(task =>
                                {
                                    TelegramClient.SendPhotoAsync(
                                        subscriberRecord.ChatId,
                                        product.ImageFileRecords.First().ImageFileId,
                                        detail,
                                        parseMode: ParseMode.Markdown,
                                        replyMarkup: new InlineKeyboardMarkup(buttons));
                                })
                                .ContinueWith(task =>
                                {
                                    HomeController.LogRecords.Add("Delayed Message Exception: " + task.Exception?.GetBaseException().Message);
                                }, TaskContinuationOptions.OnlyOnFaulted);
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

        private static List<InlineKeyboardButton> GetAdminRow(StoreProductRecord product)
        {
            return new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("حذف", "delete_approve:" + product.Id),
                InlineKeyboardButton.WithCallbackData("ویرایش", "edit:" + product.Id)
            };
        }

        private void HandleCallbackQuery(Update update)
        {
            var parts = update.CallbackQuery.Data.Split(':');

            var isAdmin = StoreAdminRepo.GetAdmin(update.CallbackQuery.Message.Chat.Id) != null;

            if (parts.Length == 2 && parts[0] == "reset" && int.TryParse(parts[1], out var productId))
            {
                using (var db = new Db())
                {
                    var product = db.StoreProductRecords.SingleOrDefault(x => x.Id == productId);

                    var rows = new List<List<InlineKeyboardButton>>
                    {
                        new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData($"تصاویر ({product.ImageFileRecords.Count})", "images:" + product.Id)
                        }
                    };

                    if (isAdmin)
                        rows.Add(GetAdminRow(product));

                    TelegramClient.EditMessageReplyMarkupAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, new InlineKeyboardMarkup(rows));
                }
            }
            else if (parts.Length == 2 && parts[0] == "edit" && int.TryParse(parts[1], out productId))
            {
                if (!isAdmin)
                {
                    TelegramClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "متاسفانه مجوز اجرای این دستور را ندارید.");
                    return;
                }

                using (var db = new Db())
                {
                    var product = db.StoreProductRecords.Include(x => x.ImageFileRecords).AsNoTracking().SingleOrDefault(x => x.Id == productId);
                    var chatId = update.CallbackQuery.Message.Chat.Id;

                    if (!NewProductStates.TryGetValue(chatId, out _))
                        NewProductStates.Add(chatId, null);

                    NewProductStates[chatId] = new NewProductInState();
                    NewProductStates[chatId].IsEdit = true;
                    NewProductStates[chatId].ProductRecord = product;

                    HandleNewProductMessage(update, new SubscriberRecord { ChatId = chatId });
                }
            }
            else if (parts.Length == 2 && parts[0] == "delete_approve" && int.TryParse(parts[1], out productId))
            {
                if (!isAdmin)
                {
                    TelegramClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "متاسفانه مجوز اجرای این دستور را ندارید.");
                    return;
                }

                var optionButtons = new[]
                {
                    InlineKeyboardButton.WithCallbackData("بیخیال", "reset:" + productId),
                    InlineKeyboardButton.WithCallbackData("نه، اصلا", "reset:" + productId),
                    InlineKeyboardButton.WithCallbackData("مطمئنم، حذفش کن", "delete:" + productId)
                };
                var rows = optionButtons.OrderBy(x => Guid.NewGuid()).Select(x => new List<InlineKeyboardButton> { x });

                TelegramClient.EditMessageReplyMarkupAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, new InlineKeyboardMarkup(rows));
            }
            else if (parts.Length == 2 && parts[0] == "delete" && int.TryParse(parts[1], out productId))
            {
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
            else if (parts.Length == 2 && parts[0] == "images" && int.TryParse(parts[1], out productId))
            {
                using (var db = new Db())
                {
                    var product = db.StoreProductRecords.Single(x => x.Id == productId);
                    var total = product.ImageFileRecords.Count;
                    var secondImageFileId = product.ImageFileRecords.Skip(total > 1 ? 1 : 0).First().ImageFileId;
                    var currentIndex = product.ImageFileRecords.Select(x => x.ImageFileId).ToList().IndexOf(secondImageFileId) + 1;

                    var rows = new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("<<", $"images_prev:{currentIndex},{productId}"),
                        InlineKeyboardButton.WithCallbackData($"تصویر {currentIndex} از {total}"),
                        InlineKeyboardButton.WithCallbackData(">>", $"images_next:{currentIndex},{productId}")
                    };

                    TelegramClient.SendPhotoAsync(update.CallbackQuery.Message.Chat.Id, secondImageFileId, replyMarkup: new InlineKeyboardMarkup(rows));
                }
            }
            else if (parts.Length == 2 && parts[0] == "images_next")
            {
                using (var db = new Db())
                {
                    var currentIndex = int.Parse(parts[1].Split(',')[0]);
                    productId = int.Parse(parts[1].Split(',')[1]);

                    var product = db.StoreProductRecords.Single(x => x.Id == productId);
                    var total = product.ImageFileRecords.Count;

                    if (currentIndex < total)
                        currentIndex++;

                    var rows = new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("<<", $"images_prev:{currentIndex}," + productId),
                        InlineKeyboardButton.WithCallbackData($"تصویر {currentIndex} از {total}", "images_nav_hide:" + productId),
                        InlineKeyboardButton.WithCallbackData(">>", $"images_next:{currentIndex}," + productId)
                    };

                    var imageFileId = product.ImageFileRecords.Skip(total > 1 ? currentIndex - 1 : 0).First().ImageFileId;
                    TelegramClient.EditMessageMediaAsync(
                        update.CallbackQuery.Message.Chat.Id,
                        update.CallbackQuery.Message.MessageId,
                        new InputMediaPhoto(imageFileId),
                        new InlineKeyboardMarkup(rows)
                    );
                }
            }
            else if (parts.Length == 2 && parts[0] == "images_prev")
            {
                using (var db = new Db())
                {
                    var currentIndex = int.Parse(parts[1].Split(',')[0]);
                    productId = int.Parse(parts[1].Split(',')[1]);

                    var product = db.StoreProductRecords.Single(x => x.Id == productId);
                    var total = product.ImageFileRecords.Count;

                    if (currentIndex > 1)
                        currentIndex--;

                    var rows = new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("<<", $"images_prev:{currentIndex}," + productId),
                        InlineKeyboardButton.WithCallbackData($"تصویر {currentIndex} از {total}", "images_nav_hide:" + productId),
                        InlineKeyboardButton.WithCallbackData(">>", $"images_next:{currentIndex}," + productId)
                    };

                    var imageFileId = product.ImageFileRecords.Skip(total > 1 ? currentIndex - 1 : 0).First().ImageFileId;
                    TelegramClient.EditMessageMediaAsync(
                        update.CallbackQuery.Message.Chat.Id,
                        update.CallbackQuery.Message.MessageId,
                        new InputMediaPhoto(imageFileId),
                        new InlineKeyboardMarkup(rows)
                    );
                }
            }
            else if (parts.Length == 2 && parts[0] == "delete_image")
            {
                var imageRecord = NewProductStates[update.CallbackQuery.Message.Chat.Id].ProductRecord.ImageFileRecords.SingleOrDefault(x => x.Id == int.Parse(parts[1]));

                if (imageRecord != null)
                {
                    NewProductStates[update.CallbackQuery.Message.Chat.Id].ProductRecord.ImageFileRecords.Remove(imageRecord);
                    TelegramClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
                    TelegramClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "عکس مورد نظر از محصول حذف شد.");
                    return;
                }
            }

            TelegramClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
        }

        private void HandleNewProductMessage(Update update, SubscriberRecord subscriberRecord)
        {
            var chatId = subscriberRecord.ChatId;

            switch (NewProductStates[chatId].NewProductStep)
            {
                case NewProductSteps.Begin:
                    NewProductStates[chatId].NewProductStep = NewProductSteps.Name;
                    TelegramClient.SendTextMessageAsync(chatId, "نام محصول:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Name:
                    if (update.Message.Text == StateManager.Keyboards.AddProductSkip && !NewProductStates[chatId].IsEdit)
                    {
                        TelegramClient.SendTextMessageAsync(chatId, "برای کارکرد بهتر بات لطفا نام محصول را وارد نمایید:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                        return;
                    }

                    if (update.Message.Text != StateManager.Keyboards.AddProductSkip)
                        NewProductStates[chatId].ProductRecord.Name = update.Message.Text;

                    NewProductStates[chatId].NewProductStep = NewProductSteps.Code;
                    TelegramClient.SendTextMessageAsync(chatId, "کد:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Code:
                    if (update.Message.Text != StateManager.Keyboards.AddProductSkip)
                    {
                        if (!NewProductStates[chatId].IsEdit ||
                            (NewProductStates[chatId].IsEdit && NewProductStates[chatId].ProductRecord.Code != update.Message.Text))
                        {
                            using (var db = new Db())
                            {
                                var duplicateCodeProduct = db.StoreProductRecords.SingleOrDefault(x => x.BotInstanceRecordId == Id && x.Code == update.Message.Text);

                                if (duplicateCodeProduct != null)
                                {
                                    TelegramClient.SendTextMessageAsync(chatId,
                                        $"کد وارد شده با کد محصول {duplicateCodeProduct.Name} یکسان است. لطفا یک کد یکتا وارد کنید:",
                                        replyMarkup: StateManager.Keyboards.AddingProductAdmin);

                                    return;
                                }
                            }
                        }
                        NewProductStates[chatId].ProductRecord.Code = update.Message.Text;
                    }

                    NewProductStates[chatId].NewProductStep = NewProductSteps.Price;
                    TelegramClient.SendTextMessageAsync(chatId, "قیمت (ریال):", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Price:
                    if (update.Message.Text != StateManager.Keyboards.AddProductSkip)
                    {
                        if (!int.TryParse(update.Message.Text.ToEnglishDigits(), out var price))
                        {
                            TelegramClient.SendTextMessageAsync(chatId, "قیمت وارد شده صحیح نیست. یک مقدار عددی وارد کنید.", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                            return;
                        }

                        NewProductStates[chatId].ProductRecord.Price = price;
                    }

                    NewProductStates[chatId].NewProductStep = NewProductSteps.Desciption;
                    TelegramClient.SendTextMessageAsync(chatId, "توضیحات:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Desciption:
                    if (update.Message.Text != StateManager.Keyboards.AddProductSkip)
                        NewProductStates[chatId].ProductRecord.Description = update.Message.Text;

                    NewProductStates[chatId].NewProductStep = NewProductSteps.Images;

                    if (!NewProductStates[chatId].IsEdit)
                        TelegramClient.SendTextMessageAsync(chatId, "تصاویر:", replyMarkup: StateManager.Keyboards.AddingProductImagesAdmin);
                    else
                    {
                        TelegramClient.SendTextMessageAsync(chatId, "تصاویر موجود را حذف یا تصاویر جدید ارسال نمایید:", replyMarkup: StateManager.Keyboards.AddingProductImagesAdmin);

                        foreach (var record in NewProductStates[chatId].ProductRecord.ImageFileRecords)
                        {
                            TelegramClient.SendPhotoAsync(
                                chatId,
                                record.ImageFileId,
                                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("حذف", "delete_image:" + record.Id))
                            );
                        }
                    }
                    break;
                case NewProductSteps.Images:
                    if (StateManager.Keyboards.AddProductSubmit == update.Message.Text)
                    {
                        var hasImage = NewProductStates[chatId].ProductRecord.ImageFileRecords.Any();

                        if (!hasImage)
                        {
                            TelegramClient.SendTextMessageAsync(chatId, "برای کارکرد بهتر بات حداقل یک تصویر ارسال نمایید", replyMarkup: StateManager.Keyboards.AddingProductImagesAdmin);
                            return;
                        }

                        using (var db = new Db())
                        {
                            if (NewProductStates[chatId].IsEdit)
                            {
                                var productId = NewProductStates[chatId].ProductRecord.Id;
                                var productRecord = db.StoreProductRecords.Include(x => x.ImageFileRecords).Single(x => x.Id == productId);

                                db.Entry(productRecord).CurrentValues.SetValues(NewProductStates[chatId].ProductRecord);

                                foreach (var imageFileRecord in productRecord.ImageFileRecords.ToList())
                                    db.ImageFileRecords.Remove(imageFileRecord);

                                var imageFileRecords = NewProductStates[chatId].ProductRecord.ImageFileRecords.ToList();
                                imageFileRecords.ForEach(record => { record.StoreProductRecordId = productId; });

                                foreach (var imageFileRecord in imageFileRecords)
                                    db.ImageFileRecords.Add(imageFileRecord);
                            }
                            else
                                db.StoreProductRecords.Add(NewProductStates[chatId].ProductRecord);

                            db.SaveChanges();
                        }

                        TelegramClient.SendTextMessageAsync(chatId, "محصول شما با موفقیت ثبت گردید", replyMarkup: StateManager.Keyboards.StartAdmin);
                        NewProductStates.Remove(chatId);

                        return;
                    }

                    if (update.Message.Type != MessageType.Photo)
                    {
                        TelegramClient.SendTextMessageAsync(chatId, "لطفا تصویر ارسال نمایید", replyMarkup: StateManager.Keyboards.AddingProductImagesAdmin);
                        return;
                    }

                    NewProductStates[chatId].ProductRecord.ImageFileRecords.Add(
                        new ImageFileRecord
                        {
                            ImageFileId = update.Message.Photo.Last().FileId
                        });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}