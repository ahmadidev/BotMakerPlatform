using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using BotMakerPlatform.Web.Areas.SupportBot;
using BotMakerPlatform.Web.CriticalDtos;
using BotMakerPlatform.Web.Repo;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Controllers
{
    public class WebhookController : Controller
    {
        private SubscriberRepo SubscriberRepo { get; set; }

        // Webhook/Update/?BotInstanceId=[int]&Secret=somesecretrandom
        [HttpPost]
        public ActionResult Update(Update update, int botInstanceId, string secret)//(WebhookUpdateDto webhookUpdateDto) ToDo: make binder bind Update from body
        {
            var webhookUpdateDto = new WebhookUpdateDto { Update = update, BotInstanceId = botInstanceId, Secret = secret };
            HomeController.LogRecords.Add($"Telegram hit webhook BotInstanceId: {webhookUpdateDto.BotInstanceId} Secret: {webhookUpdateDto.Secret} UpdateType: {webhookUpdateDto.Update.Type}.");

            var botInstanceRecord = BotInstanceRepo.BotInstanceRecords.SingleOrDefault(x => x.Id == webhookUpdateDto.BotInstanceId && x.WebhookSecret == webhookUpdateDto.Secret);
            if (botInstanceRecord == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "BotUniqueName or Secret is invalid.");

            Request.GetOwinContext().Set("BotClientToken", botInstanceRecord.Token);
            Request.GetOwinContext().Set("BotInstanceId", webhookUpdateDto.BotInstanceId);

            using (var scope = IocConfig.Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                var typeName = typeof(SupportBotInstance).FullName.Replace("SupportBot", botInstanceRecord.BotUniqueName);

                var botInstance = (IBotInstance)scope.Resolve(Type.GetType(typeName));
                botInstance.Id = botInstanceRecord.Id;

                SubscriberRepo = scope.Resolve<SubscriberRepo>();

                //TODO: We assume update is always a message
                var subscriber = GetSubscriber(webhookUpdateDto.Update.Message.Chat.Id) ?? AddSubscriber(webhookUpdateDto.Update);
                botInstance.Update(webhookUpdateDto.Update, subscriber);
            }

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
    }
}