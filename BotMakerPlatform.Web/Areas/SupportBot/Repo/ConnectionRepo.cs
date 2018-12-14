using System;
using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Record;
using BotMakerPlatform.Web.Repo;

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

        public void Add(SubscriberRecord supporter, SubscriberRecord customer)
        {
            Connections.Add(new ConnectionRecord
            {
                BotInstanceId = BotInstanceId,
                SupporterChatId = supporter.ChatId,
                CustomerChatId = customer.ChatId,
                CreatedAt = DateTime.UtcNow
            });
        }

        public long FindPartyChatId(SubscriberRecord subscriberRecord)
        {
            var chatId = subscriberRecord.ChatId;

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

        public void RemoveByCustomer(SubscriberRecord customer)
        {
            Connections.RemoveAll(x => x.CustomerChatId == customer.ChatId);
        }
    }
}