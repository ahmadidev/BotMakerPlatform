using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Autofac;
using BotMakerPlatform.Web.Areas.SupportBot;
using BotMakerPlatform.Web.CriticalDtos;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Controllers
{
    public class WebhookController : Controller
    {
        // Webhook/Update/?BotInstanceId=Ahmadbot&Secret=somesecretrandom
        [HttpPost]
        public ActionResult Update(Update update, int botInstanceId, string secret)//(WebhookUpdateDto webhookUpdateDto) To make binder bind Update from body
        {
            var webhookUpdateDto = new WebhookUpdateDto { Update = update, BotInstanceId = botInstanceId, Secret = secret };

            HomeController.LogRecords.Add($"Telegram hit webhook BotInstanceId: {webhookUpdateDto.BotInstanceId} Secret: {webhookUpdateDto.Secret} UpdateType: {webhookUpdateDto.Update.Type}.");

            if (webhookUpdateDto.Update.Type != UpdateType.MessageUpdate)
                return Content("");

            var subscriber = GetSubscriber(webhookUpdateDto);
            if (subscriber == null)
                subscriber = AddSubscriber(webhookUpdateDto);

            var botInstance = MakeBotInstance(webhookUpdateDto);
            botInstance.Update(webhookUpdateDto.Update, subscriber);

            foreach (var sub in SubscriberRepo.Subscribers)
                HomeController.LogRecords.Add($"-> Bot user - BotInstanceId: {sub.BotInstanceId} username: {sub.Username} chatId : {sub.ChatId}");

            return Content("");
        }

        private static Subscriber GetSubscriber(WebhookUpdateDto webhookUpdateDto)
        {
            return SubscriberRepo.Subscribers.SingleOrDefault(x => x.BotInstanceId == webhookUpdateDto.BotInstanceId &&
                                                                   x.ChatId == webhookUpdateDto.Update.Message.Chat.Id);
        }

        private static Subscriber AddSubscriber(WebhookUpdateDto webhookUpdateDto)
        {
            HomeController.LogRecords.Add($"Adding Subscriber BotInstanceId: {webhookUpdateDto.BotInstanceId}.");

            //TODO: First and Last
            var subscriber = new Subscriber
            {
                BotInstanceId = webhookUpdateDto.BotInstanceId,
                ChatId = webhookUpdateDto.Update.Message.Chat.Id,
                Username = webhookUpdateDto.Update.Message.Chat.Username,
                FirstName = webhookUpdateDto.Update.Message.From.FirstName,
                LastName = webhookUpdateDto.Update.Message.From.LastName
            };

            SubscriberRepo.Subscribers.Add(subscriber);

            HomeController.LogRecords.Add($"Added Subscriber BotInstanceId: {subscriber.BotInstanceId}.");

            return subscriber;
        }

        private static IBotInstance MakeBotInstance(WebhookUpdateDto webhookUpdateDto)
        {
            var botInstanceRecord = BotInstanceRepo.BotInstanceRecords.SingleOrDefault(x => x.Id == webhookUpdateDto.BotInstanceId && x.WebhookSecret == webhookUpdateDto.Secret);

            if (botInstanceRecord == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "BotUniqueName or Secret is invalid.");

            //Start initializing BotInstance props
            ITelegramBotClient telegramClient;
            //TODO: Make sure don't leak
            using (var scope = IocConfig.Container.BeginLifetimeScope())
                telegramClient = scope.Resolve<ITelegramBotClient>(new NamedParameter("token", botInstanceRecord.Token));

            var subscribers = SubscriberRepo.Subscribers.Where(x => x.BotInstanceId == webhookUpdateDto.BotInstanceId);

            //BotInstance actual work.
            //BotMakerPlatform.Web.Areas.SupportBot.SupportBotInstance
            var typeName = typeof(SupportBotInstance).FullName.Replace("SupportBot", botInstanceRecord.BotUniqueName);

            var botInstance = (IBotInstance)Activator.CreateInstance(Type.GetType(typeName));
            botInstance.Id = botInstanceRecord.Id;
            botInstance.Subscribers = subscribers;
            botInstance.TelegramClient = telegramClient;

            return botInstance;
        }
    }
}