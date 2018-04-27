namespace BotMakerPlatform.Web.Areas.SupportBot.Record
{
    public class ConnectionRecord
    {
        public int BotInstanceId { get; set; }

        public long CustomerChatId { get; set; }

        public long SupporterChatId { get; set; }
    }
}