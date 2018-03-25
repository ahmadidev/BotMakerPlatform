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

            var bot = UserBotRepo.UserBots.SingleOrDefault(x => x.BotId == botId && x.WebhookSecret == secret);

            if (bot == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "BotUniqueName or Secret is invalid.");

            var subscriber = SubscriberRepo.Subscribers.SingleOrDefault(x => x.ChatId == update.Message.Chat.Id);
            var botClient = new TelegramBotClient(bot.Token);

            if (subscriber == null)
            {
                //TODO: First and Last
                subscriber = new Subscriber
                {
                    BotId = botId,
                    ChatId = update.Message.Chat.Id,
                    Username = update.Message.Chat.Username,
                    FirstName = update.Message.From.FirstName,
                    LastName = update.Message.From.LastName
                };
                SubscriberRepo.Subscribers.Add(subscriber);
                HomeController.LogRecords.Add($"Bot user added - botId: {botId} username: {update.Message.Chat.Username}");
            }

            BotRepo.Bots.First(x => x.UniqueName == bot.BotUniqueName).Update(botClient, update, botId, subscriber);

            return Content("");
        }
    }
}