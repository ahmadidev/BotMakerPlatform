using System;
using System.Linq;
using Autofac;
using Autofac.Core.Lifetime;
using BotMakerPlatform.Web.Areas.SupportBot;
using BotMakerPlatform.Web.CriticalDtos;
using BotMakerPlatform.Web.Repo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace BotMakerPlatform.Web.Controllers
{
    public class WebhookController : Controller
    {
        private SubscriberRepo SubscriberRepo { get; set; }
        private Db Db { get; }
        private IHttpContextAccessor HttpContextAccessor { get; }

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
            {
                ModelState.AddModelError("", "BotUniqueName or Secret is invalid.");
                return BadRequest(ModelState);
            }

            HttpContextAccessor.HttpContext.Items.Add("BotClientToken", botInstanceRecord.Token);
            HttpContextAccessor.HttpContext.Items.Add("BotInstanceId", webhookUpdateDto.BotInstanceId);

            using (var scope = IocConfig.Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                var botInstance = GetBotInstance(botInstanceRecord, scope);
                var subscriber = GetOrAddSubscriber(webhookUpdateDto, scope);

                Dump(botInstance, webhookUpdateDto.Update);

                botInstance.Update(webhookUpdateDto.Update, subscriber);
            }

            return Ok();
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

                messageHeader += $"\n{update.Message.Type}";

                if (update.Message.Caption.HasText())
                {
                    messageHeader += $"\n{update.Message.Caption}";
                }

                if (update.Message.Text.HasText())
                {
                    messageHeader += $"\n{update.Message.Text}";
                }

                //TODO: remove  when phot fixed
                Dumper.Instance().TelegramClient.SendTextMessageAsync(Dumper.ChatId, messageHeader, disableNotification: true);

                string fileId;
                switch (update.Message.Type)
                {
                    case MessageType.Text:
                        if (update.Message.Text != null)
                            Dumper.Instance().TelegramClient.SendTextMessageAsync(Dumper.ChatId, messageHeader, disableNotification: true);
                        break;
                    case MessageType.Photo:
                        fileId = update.Message.Photo.Last().FileId;
                        Dumper.Instance().TelegramClient.SendPhotoAsync(Dumper.ChatId, new InputOnlineFile(fileId), caption: messageHeader, disableNotification: true);
                        break;
                    case MessageType.Audio:
                        fileId = update.Message.Audio.FileId;
                        Dumper.Instance().TelegramClient.SendAudioAsync(Dumper.ChatId, new InputOnlineFile(fileId), messageHeader,
                            ParseMode.Default,
                            update.Message.Audio.Duration, update.Message.Audio.Performer ?? "", update.Message.Audio.Title ?? "", disableNotification: true);
                        break;
                    case MessageType.Video:
                        fileId = update.Message.Video.FileId;
                        Dumper.Instance().TelegramClient.SendVideoAsync(Dumper.ChatId, new InputOnlineFile(fileId),
                            caption: messageHeader, disableNotification: true);
                        break;
                    case MessageType.Voice:
                        fileId = update.Message.Voice.FileId;
                        Dumper.Instance().TelegramClient.SendVoiceAsync(Dumper.ChatId, new InputOnlineFile(fileId), caption: messageHeader, disableNotification: true);
                        break;
                    case MessageType.Document:
                        fileId = update.Message.Document.FileId;
                        Dumper.Instance().TelegramClient.SendDocumentAsync(Dumper.ChatId, new InputOnlineFile(fileId), caption: messageHeader, disableNotification: true);
                        break;
                    case MessageType.Sticker:
                        fileId = update.Message.Sticker.FileId;
                        Dumper.Instance().TelegramClient.SendStickerAsync(Dumper.ChatId, new InputOnlineFile(fileId), disableNotification: true);
                        Dumper.Instance().TelegramClient.SendTextMessageAsync(Dumper.ChatId, messageHeader, disableNotification: true);
                        break;
                    case MessageType.Location:
                        Dumper.Instance().TelegramClient.SendLocationAsync(Dumper.ChatId, update.Message.Location.Latitude,
                            update.Message.Location.Longitude, disableNotification: true);
                        Dumper.Instance().TelegramClient.SendTextMessageAsync(Dumper.ChatId, messageHeader, disableNotification: true);
                        break;
                    case MessageType.VideoNote:
                        fileId = update.Message.VideoNote.FileId;
                        fileId = update.Message.VideoNote.FileId;
                        Dumper.Instance().TelegramClient.SendVideoNoteAsync(Dumper.ChatId, new InputOnlineFile(fileId),
                            update.Message.VideoNote.Duration, update.Message.VideoNote.Length);
                        Dumper.Instance().TelegramClient.SendTextMessageAsync(Dumper.ChatId, messageHeader, disableNotification: true);
                        break;
                    case MessageType.Contact:
                    case MessageType.Venue:
                    case MessageType.Game:
                    case MessageType.Invoice:
                    case MessageType.SuccessfulPayment:
                    case MessageType.Unknown:
                    case MessageType.WebsiteConnected:
                    case MessageType.ChatMembersAdded:
                    case MessageType.ChatMemberLeft:
                    case MessageType.ChatTitleChanged:
                    case MessageType.ChatPhotoChanged:
                    case MessageType.MessagePinned:
                    case MessageType.ChatPhotoDeleted:
                    case MessageType.GroupCreated:
                    case MessageType.SupergroupCreated:
                    case MessageType.ChannelCreated:
                    case MessageType.MigratedToSupergroup:
                    case MessageType.MigratedFromGroup:
                    default:
                        Dumper.Instance().TelegramClient.SendTextMessageAsync(Dumper.ChatId,
                            $"{messageHeader} + Message type {update.Message.Type} is not supported.");
                        break;
                }
            }
            else
            {
                Dumper.Instance().TelegramClient.SendTextMessageAsync(Dumper.ChatId, $"{messageHeader}\nUpdate received ({update.Type}): {JsonConvert.SerializeObject(update)}", disableWebPagePreview: true);
            }
        }
    }
}