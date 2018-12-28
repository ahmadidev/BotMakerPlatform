using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BotMakerPlatform.Web.Areas.StoreBot.Controllers;
using BotMakerPlatform.Web.Areas.StoreBot.Models;
using BotMakerPlatform.Web.Areas.StoreBot.Record;
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

            public static IReplyMarkup StartAdmin => new ReplyKeyboardMarkup(new KeyboardButton[] { NewProductCommand, ListProductsCommand }, resizeKeyboard: true);
            public static IReplyMarkup AddingProductAdmin => new ReplyKeyboardMarkup(new KeyboardButton[] { CancelCommand }, resizeKeyboard: true);
            public static IReplyMarkup Empty => new ReplyKeyboardRemove();
        }
    }

    public class StoreBotInstance : IBotInstance
    {
        public int Id { get; set; }
        public string Username { get; set; }

        private ITelegramBotClient TelegramClient { get; }
        private SettingRepo SettingRepo { get; }

        public enum NewProductSteps
        {
            Begin,
            Name,
            Code,
            Price,
            Desciption,
            Image
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

        public StoreBotInstance(ITelegramBotClient telegramClient, SettingRepo settingRepo)
        {
            TelegramClient = telegramClient;
            SettingRepo = settingRepo;
        }

        public void Update(Update update, SubscriberRecord subscriberRecord)
        {
            if (update.Type != UpdateType.Message)
            {
                TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, $"ببخشید، فعلا این چیزا ({update.Type}) رو متوجه نمیشم");
                return;
            }

            var isAdmin = StoreAdminRepo.StoreAdmins.Any(x => x.ChatId == subscriberRecord.ChatId);
            //if (!isAdmin)
            //{
            //    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "شرمنده شما مدیر بات نیستید. فعلا فقط بخش مدیریت بات رو توسعه دادیم.");
            //    return;
            //}

            switch (update.Message.Text)
            {
                case StateManager.Keyboards.StartCommand:
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "به فروشگاه ما خوش آمدید", replyMarkup: StateManager.Keyboards.StartAdmin);
                    break;
                case StateManager.Keyboards.NewProductCommand:
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

                            Task.Delay(i * 10).ContinueWith(task =>
                            {
                                TelegramClient.SendPhotoAsync(subscriberRecord.ChatId, product.ImageFileId, detail, parseMode: ParseMode.Markdown, replyMarkup: StateManager.Keyboards.StartAdmin);
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
                        HandleNewProductMessage(update, subscriberRecord);
                    else
                        TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "متوجه پیام نشدم..");
                    break;
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
                    NewProductStates[subscriberRecord.ChatId].ProductRecord.Name = update.Message.Text;
                    NewProductStates[subscriberRecord.ChatId].NewProductStep = NewProductSteps.Code;
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "کد:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Code:
                    NewProductStates[subscriberRecord.ChatId].ProductRecord.Code = update.Message.Text;
                    NewProductStates[subscriberRecord.ChatId].NewProductStep = NewProductSteps.Price;
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "قیمت:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Price:
                    if (!int.TryParse(update.Message.Text, out var price))
                    {
                        TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "قیمت وارد شده صحیح نیست. یک مقدار عددی وارد کنید.", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                        return;
                    }

                    NewProductStates[subscriberRecord.ChatId].ProductRecord.Price = price;
                    NewProductStates[subscriberRecord.ChatId].NewProductStep = NewProductSteps.Desciption;
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "توضیحات:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Desciption:
                    NewProductStates[subscriberRecord.ChatId].ProductRecord.Description = update.Message.Text;
                    NewProductStates[subscriberRecord.ChatId].NewProductStep = NewProductSteps.Image;
                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "تصویر:", replyMarkup: StateManager.Keyboards.AddingProductAdmin);
                    break;
                case NewProductSteps.Image:
                    NewProductStates[subscriberRecord.ChatId].ProductRecord.ImageFileId = update.Message.Photo.Last().FileId;

                    using (var db = new Db())
                    {
                        db.StoreProductRecords.Add(NewProductStates[subscriberRecord.ChatId].ProductRecord);
                        db.SaveChanges();
                    }

                    TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "محصول شما با موفقیت ثبت گردید", replyMarkup: StateManager.Keyboards.StartAdmin);
                    NewProductStates.Remove(subscriberRecord.ChatId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}