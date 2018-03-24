using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BotMakerPlatform.Web.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Controllers
{
    public class WebhookController : Controller
    {
        //Webhook/Update/?botid=Ahmadbot&secret=somesecretrandom
        public ActionResult Update(Update update, string botId, string secret)
        {
            HomeController.LogRecords.Add($"Telegram hit webhook botId:{botId} secret: {secret}.");

            if (update.Type != UpdateType.MessageUpdate)
                return Content("");

            //TODO: Handle bot message

            var bot = HomeController.Bots.SingleOrDefault(x => x.Id == botId && x.WebhookSecret == secret);

            if (bot == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "BotId or Secret is invalid.");

            var botUser = HomeController.Users.SingleOrDefault(x => x.ChatId == update.Message.Chat.Id);
            if (botUser == null)
            {
                HomeController.Users.Add(new BotUserDto
                {
                    ChatId = update.Message.Chat.Id,
                    Username = update.Message.Chat.Username,
                    FirstName = update.Message.Chat.FirstName,
                    LastName = update.Message.Chat.LastName
                });
            }

            HomeController.LogRecords.Add($"Bot user added - botId: {botId} username: {update.Message.Chat.Username}");

            new TelegramBotClient(bot.Token).SendTextMessageAsync(update.Message.Chat.Id,
                "Bot is hosted on Bot Maker Platform.\n" +
                "Unfortunately its currently on development...");

            return Content("");
        }
    }
}