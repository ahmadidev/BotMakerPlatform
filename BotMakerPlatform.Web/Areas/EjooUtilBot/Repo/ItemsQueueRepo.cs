using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Record;
using BotMakerPlatform.Web.Repo;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot.Repo
{
    public class ItemsQueueRepo
    {
        private static readonly List<ItemRecord> Images = new List<ItemRecord>();

        private int BotInstanceId { get; }

        public ItemsQueueRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public IEnumerable<ItemRecord> GetAll()
        {
            return Images.Where(x => x.BotInstanceId == BotInstanceId);
        }

        public void Add(SubscriberRecord subscriberRecord, ItemRecord itemRecord)
        {
            Images.Add(itemRecord);
        }

        public IEnumerable<ItemRecord> GetCurrentSessionImages(SubscriberRecord subscriberRecord)
        {
            return GetAll().Where(x => x.ChatId == subscriberRecord.ChatId).OrderBy(x => x.MessageId);
        }

        public void ClearCurrentSessionImages(SubscriberRecord subscriberRecord)
        {
            Images.RemoveAll(x => x.BotInstanceId == BotInstanceId && x.ChatId == subscriberRecord.ChatId);
        }
    }
}