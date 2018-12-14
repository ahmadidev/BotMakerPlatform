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
            return Supporters.Where(x => x.BotInstanceId == BotInstanceId);
        }

        public void Add(long chatId)
        {
            Remove(chatId);
            Supporters.Add(new SubscriberRecord { BotInstanceId = BotInstanceId, ChatId = chatId });
        }

        public void Remove(long chatId)
        {
            Supporters.RemoveAll(x => x.BotInstanceId == BotInstanceId && x.ChatId == chatId);
        }

        public bool IsSupporter(SubscriberRecord subscriberRecord)
        {
            return GetAll().Any(x => x.ChatId == subscriberRecord.ChatId);
        }
    }
}