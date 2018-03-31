using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class Connection
    {
        public int BotInstanceId { get; set; }
        public long UserChatId { get; set; }
        public long SupporterChatId { get; set; }
    }
}