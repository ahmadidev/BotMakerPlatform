using BotMakerPlatform.Web.Repo;

namespace BotMakerPlatform.Web.Areas.StoreBot
{
    public class StoreAdminRecord
    {
        public long ChatId { get; set; }

        public int BotInstanceRecordId { get; set; }
        public virtual BotInstanceRecord BotInstanceRecord { get; set; }
    }
}