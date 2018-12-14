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
            return GetAll().SingleOrDefault(x => x.ChatId == chatId);
        }

        public void Add(long chatId, string username, string firstName, string lastName)
        {
            Db.Subscribers.Add(new SubscriberRecord
            {
                ChatId = chatId,
                BotInstanceRecordId = BotInstanceId,
                Username = username,
                FirstName = firstName,
                LastName = lastName
            });
            Db.SaveChanges();
        }
    }
}