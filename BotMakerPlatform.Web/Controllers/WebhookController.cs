using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Controllers
{
    public class WebhookController : Controller
    {
        //Webhook/Update/?botid=Ahmadbot&secret=somesecretrandom
        public ActionResult Update(Update update, int botId, string secret)
        {
            HomeController.LogRecords.Add($"Telegram hit webhook botId: {botId} secret: {secret}.");

            if (update.Type != UpdateType.MessageUpdate)
                return Content("");

            //TODO: Handle bot message

            var bot = UserBotRepo.UserBots.SingleOrDefault(x => x.BotId == botId && x.WebhookSecret == secret);

            if (bot == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "BotUniqueName or Secret is invalid.");

            var botUser = SubscriberRepo.Subscribers.SingleOrDefault(x => x.ChatId == update.Message.Chat.Id);
            if (botUser == null)
            {
                //TODO: First and Last
                SubscriberRepo.Subscribers.Add(new Subscriber
                {
                    BotId = botId,
                    ChatId = update.Message.Chat.Id,
                    Username = update.Message.Chat.Username,
                    FirstName = update.Message.From.FirstName,
                    LastName = update.Message.From.LastName
                });
            }

            HomeController.LogRecords.Add($"Bot user added - botId: {botId} username: {update.Message.Chat.Username}");

            new TelegramBotClient(bot.Token).SendTextMessageAsync(update.Message.Chat.Id,
                "Bot is hosted on Bot Maker Platform.\n" +
                "Unfortunately its currently on development...");

            //TODO: Bot instance and call event

            return Content("");
        }
    }
}