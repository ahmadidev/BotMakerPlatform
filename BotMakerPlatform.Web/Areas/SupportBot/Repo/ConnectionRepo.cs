using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Record;

namespace BotMakerPlatform.Web.Areas.SupportBot.Repo
{
    public class ConnectionRepo
    {
        private static readonly List<ConnectionRecord> Connections = new List<ConnectionRecord>();

        private int BotInstanceId { get; }

        public ConnectionRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public IEnumerable<ConnectionRecord> GetAll()
        {
            return Connections.Where(x => x.BotInstanceId == BotInstanceId);
        }

        public void Add(Subscriber supporter, Subscriber customer)
        {
            Connections.Add(new ConnectionRecord
            {
                BotInstanceId = BotInstanceId,
                SupporterChatId = supporter.ChatId,
                CustomerChatId = customer.ChatId
            });
        }

        public long FindPartyChatId(Subscriber subscriber)
        {
            var chatId = subscriber.ChatId;

            var party = Connections.SingleOrDefault(x =>
                x.BotInstanceId == BotInstanceId &&
                (x.SupporterChatId == chatId || x.CustomerChatId == chatId)
            );

            if (party == null)
                return default(long);

            return party.SupporterChatId == chatId
                ? party.CustomerChatId
                : party.SupporterChatId;
        }

        public void RemoveByCustomer(Subscriber customer)
        {
            Connections.RemoveAll(x => x.CustomerChatId == customer.ChatId);
        }
    }
}