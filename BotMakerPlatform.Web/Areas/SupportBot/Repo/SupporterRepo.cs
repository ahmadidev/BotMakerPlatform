using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Repo;

namespace BotMakerPlatform.Web.Areas.SupportBot.Repo
{
    public class SupporterRepo
    {
        private static readonly List<SubscriberRecord> Supporters = new List<SubscriberRecord>();

        private int BotInstanceId { get; }

        public SupporterRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public IEnumerable<SubscriberRecord> GetAll()
        {
            return Supporters.Where(x => x.BotInstanceRecordId == BotInstanceId);
        }

        public void Add(long chatId)
        {
            Remove(chatId);
            Supporters.Add(new SubscriberRecord { BotInstanceRecordId = BotInstanceId, ChatId = chatId });
        }

        public void Remove(long chatId)
        {
            Supporters.RemoveAll(x => x.BotInstanceRecordId == BotInstanceId && x.ChatId == chatId);
        }

        public bool IsSupporter(SubscriberRecord subscriberRecord)
        {
            return GetAll().Any(x => x.ChatId == subscriberRecord.ChatId);
        }
    }
}