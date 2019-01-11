using System.Collections.Generic;
using System.Linq;

namespace BotMakerPlatform.Web.Repo
{
    public class SubscriberRepo
    {
        private int BotInstanceId { get; }
        private Db Db { get; }

        public SubscriberRepo(int botInstanceId, Db db)
        {
            BotInstanceId = botInstanceId;
            Db = db;
        }

        public IEnumerable<SubscriberRecord> GetAll()
        {
            return Db.Subscribers.Where(x => x.BotInstanceRecordId == BotInstanceId);
        }

        public SubscriberRecord GetByChatId(long chatId)
        {
            return GetAll().SingleOrDefault(x => x.BotInstanceRecordId == BotInstanceId && x.ChatId == chatId);
        }

        public void Add(SubscriberRecord subscriber)
        {
            Db.Subscribers.Add(subscriber);
            Db.SaveChanges();
        }
    }
}