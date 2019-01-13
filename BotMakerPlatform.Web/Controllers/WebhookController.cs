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
using Newtonsoft.Json;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Controllers
{
    public class WebhookController : Controller
    {
        private SubscriberRepo SubscriberRepo { get; set; }
        private Db Db { get; }

        public WebhookController(Db db)
        {
            Db = db;
        }

        // Webhook/Update/?BotInstanceId=[int]&Secret=somesecretrandom
        [HttpPost]
        public ActionResult Update([ModelBinder(typeof(UpdateModelBinder))]WebhookUpdateDto webhookUpdateDto)
        {
            Log.Information("Telegram hit webhook BotInstanceId: {BotInstanceId} Secret: {Secret} UpdateType: {UpdateType} MessageType: {MessageType}.",
                webhookUpdateDto.BotInstanceId, webhookUpdateDto.Secret, webhookUpdateDto.Update.Type, webhookUpdateDto.Update.Message?.Type);

            var botInstanceRecord = Db.BotInstanceRecords.SingleOrDefault(x => x.Id == webhookUpdateDto.BotInstanceId && x.WebhookSecret == webhookUpdateDto.Secret);

            if (botInstanceRecord == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "BotUniqueName or Secret is invalid.");

            Request.GetOwinContext().Set("BotClientToken", botInstanceRecord.Token);
            Request.GetOwinContext().Set("BotInstanceId", webhookUpdateDto.BotInstanceId);

            using (var scope = IocConfig.Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                var botInstance = GetBotInstance(botInstanceRecord, scope);
                var subscriber = GetOrAddSubscriber(webhookUpdateDto, scope);

                Dump(botInstance, webhookUpdateDto.Update);

                botInstance.Update(webhookUpdateDto.Update, subscriber);
            }

            return Content("");
        }

        private SubscriberRecord GetOrAddSubscriber(WebhookUpdateDto webhookUpdateDto, ILifetimeScope scope)
        {
            SubscriberRepo = scope.Resolve<SubscriberRepo>();

            var message = webhookUpdateDto.Update.Message;
            var subscriber = SubscriberRepo.GetByChatId(message.Chat.Id);

            if (subscriber != null)
                return subscriber;

            subscriber = new SubscriberRecord
            {
                ChatId = message.Chat.Id,
                BotInstanceRecordId = webhookUpdateDto.BotInstanceId,
                Username = message.Chat.Username,
                FirstName = message.From.FirstName,
                LastName = message.From.LastName
            };

            SubscriberRepo.Add(subscriber);

            return subscriber;
        }

        private static IBotInstance GetBotInstance(BotInstanceRecord botInstanceRecord, ILifetimeScope scope)
        {
            var typeName = typeof(SupportBotInstance).FullName.Replace("SupportBot", botInstanceRecord.BotUniqueName);

            var botInstance = (IBotInstance)scope.Resolve(Type.GetType(typeName));
            botInstance.BotInstanceId = botInstanceRecord.Id;
            botInstance.Username = botInstanceRecord.BotUsername;
            return botInstance;
        }

        private static void Dump(IBotInstance botInstance, Update update)
        {
            var messageHeader = $"@{botInstance.Username}";

            if (update.Type == UpdateType.Message)
            {
                messageHeader += $" - {update.Message.From.FirstName}";

                if (update.Message.From.LastName.HasText())
                    messageHeader += update.Message.From.LastName;

                if (update.Message.From.Username.HasText())
                    messageHeader += $"(@{update.Message.From.Username})";

                Dumper.Instance().TelegramClient.SendTextMessageAsync(Dumper.ChatId, $"{messageHeader}\n{update.Message.Type}");
            }
            else
            {
                Dumper.Instance().TelegramClient.SendTextMessageAsync(Dumper.ChatId, $"{messageHeader}\nUpdate received ({update.Type}): {JsonConvert.SerializeObject(update)}", disableWebPagePreview: true);
            }
        }
    }
}