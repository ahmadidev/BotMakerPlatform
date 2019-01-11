using BotMakerPlatform.Web.Repo;

namespace BotMakerPlatform.Web.Areas.StoreBot.Repo
{
    public class StoreAdminRecord
    {
        public long ChatId { get; set; }

        public int BotInstanceRecordId { get; set; }
        public virtual BotInstanceRecord BotInstanceRecord { get; set; }
    }
}