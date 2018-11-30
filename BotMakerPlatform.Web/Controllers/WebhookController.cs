using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using BotMakerPlatform.Web.Areas.SupportBot;
using BotMakerPlatform.Web.CriticalDtos;
using BotMakerPlatform.Web.Repo;
using Newtonsoft.Json;
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
            //TODO: Find out the reason of not mapping firstname and lastname of update
            Request.InputStream.Position = 0;
            var input = new StreamReader(Request.InputStream).ReadToEnd();
            update = JsonConvert.DeserializeObject<Update>(input);
            //HomeController.LogRecords.Add(input);
            //HomeController.LogRecords.Add(update.Message.From.FirstName);
            //HomeController.LogRecords.Add(update.Message.Chat.FirstName);

            var webhookUpdateDto = new WebhookUpdateDto { Update = update, BotInstanceId = botInstanceId, Secret = secret };
            HomeController.LogRecords.Add($"Telegram hit webhook BotInstanceId: {webhookUpdateDto.BotInstanceId} Secret: {webhookUpdateDto.Secret} UpdateType: {update.Type}.");

            //var botInstanceRecord = BotInstanceRepo.BotInstanceRecords.SingleOrDefault(x => x.Id == webhookUpdateDto.BotInstanceId && x.WebhookSecret == webhookUpdateDto.Secret);
            BotInstanceRecord botInstanceRecord;
            using (var db = new Db())
                botInstanceRecord = db.BotInstanceRecords.SingleOrDefault(x => x.Id == botInstanceId);

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