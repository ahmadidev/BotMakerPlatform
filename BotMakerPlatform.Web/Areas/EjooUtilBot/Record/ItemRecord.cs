using System;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot.Record
{
    public class ItemRecord
    {
        public enum ItemTypes
        {
            IMAGE,
            PDF_FILE,
            IMAGE_FILE,
            WEB_PAGE,
            TEXT,
        }
        
        public ItemTypes ItemType { get; set; }

        public String FileId { get; set; }

        public String Text { get; set; }

        public long ChatId { get; set; }
        public int BotInstanceId { get; set; }


        public int MessageId { get; set; }


    }
}