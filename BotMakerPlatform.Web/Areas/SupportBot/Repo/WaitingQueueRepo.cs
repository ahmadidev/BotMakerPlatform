using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Record;

namespace BotMakerPlatform.Web.Areas.SupportBot.Repo
{
    public class WaitingQueueRepo
    {
        private static readonly List<WaiterRecord> WaiterRecords = new List<WaiterRecord>();

        private int BotInstanceId { get; }

        public WaitingQueueRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public IEnumerable<long> GetAll()
        {
            return WaiterRecords.Where(x => x.BotInstanceId == BotInstanceId).Select(x => x.WaiterChatId);
        }

        public bool Any()
        {
            return GetAll().Any();
        }

        public void Enqueue(Subscriber customer)
        {
            WaiterRecords.Add(new WaiterRecord
            {
                BotInstanceId = BotInstanceId,
                WaiterChatId = customer.ChatId
            });
        }

        public long Dequeue()
        {
            var firstWaiterChatId = GetAll().FirstOrDefault();

            if (firstWaiterChatId == default(long))
                return default(long);

            WaiterRecords.RemoveAll(x => x.BotInstanceId == BotInstanceId && x.WaiterChatId == firstWaiterChatId);

            return firstWaiterChatId;
        }

        public int GetPosition(Subscriber customer)
        {
            return GetAll().ToList().IndexOf(customer.ChatId) + 1;
        }

        public bool HasWaiter(Subscriber customer)
        {
            return GetAll().Any(x => x == customer.ChatId);
        }
    }
}