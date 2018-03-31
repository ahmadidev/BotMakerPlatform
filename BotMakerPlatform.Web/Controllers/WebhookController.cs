using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Autofac;
using BotMakerPlatform.Web.Areas.SupportBot;
using BotMakerPlatform.Web.CriticalDtos;
using BotMakerPlatform.Web.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Controllers
{
    public class WebhookController : Controller
    {
        private SubscriberRepo SubscriberRepo { get; set; }

        // Webhook/Update/?BotInstanceId=Ahmadbot&Secret=somesecretrandom
        [HttpPost]
        public ActionResult Update(Update update, int botInstanceId, string secret)//(WebhookUpdateDto webhookUpdateDto) ToDo: make binder bind Update from body
        {
            var webhookUpdateDto = new WebhookUpdateDto { Update = update, BotInstanceId = botInstanceId, Secret = secret };
            HomeController.LogRecords.Add($"Telegram hit webhook BotInstanceId: {webhookUpdateDto.BotInstanceId} Secret: {webhookUpdateDto.Secret} UpdateType: {webhookUpdateDto.Update.Type}.");

            SubscriberRepo = new SubscriberRepo(webhookUpdateDto.BotInstanceId);

            var botInstance = MakeBotInstance(webhookUpdateDto);

            //TODO: We assume update is always a message
            var subscriber = GetSubscriber(webhookUpdateDto.Update.Message.Chat.Id) ?? AddSubscriber(webhookUpdateDto.Update);
            botInstance.Update(webhookUpdateDto.Update, subscriber);

            return Content("");
        }

        private Subscriber GetSubscriber(long chatId)
        {
            return SubscriberRepo.GetAll().SingleOrDefault(x => x.ChatId == chatId);
        }

        private Subscriber AddSubscriber(Update update)
        {
            var message = update.Message;

            //TODO: First and Last are not provided
            SubscriberRepo.Add(message.Chat.Id,
                               message.Chat.Username,
                               message.From.FirstName,
                               message.From.LastName);

            return GetSubscriber(message.Chat.Id);
        }

        private IBotInstance MakeBotInstance(WebhookUpdateDto webhookUpdateDto)
        {
            var botInstanceRecord = BotInstanceRepo.BotInstanceRecords.SingleOrDefault(x => x.Id == webhookUpdateDto.BotInstanceId && x.WebhookSecret == webhookUpdateDto.Secret);

            if (botInstanceRecord == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "BotUniqueName or Secret is invalid.");

            //Start initializing BotInstance props
            ITelegramBotClient telegramClient;
            //TODO: Make sure don't leak
            using (var scope = IocConfig.Container.BeginLifetimeScope())
                telegramClient = scope.Resolve<ITelegramBotClient>(new NamedParameter("token", botInstanceRecord.Token));

            //BotInstance actual work.
            //BotMakerPlatform.Web.Areas.SupportBot.SupportBotInstance
            var typeName = typeof(SupportBotInstance).FullName.Replace("SupportBot", botInstanceRecord.BotUniqueName);

            var botInstance = (IBotInstance)Activator.CreateInstance(Type.GetType(typeName));
            botInstance.Id = botInstanceRecord.Id;
            botInstance.TelegramClient = telegramClient;
            botInstance.Subscribers = SubscriberRepo.GetAll();

            return botInstance;
        }
    }
}