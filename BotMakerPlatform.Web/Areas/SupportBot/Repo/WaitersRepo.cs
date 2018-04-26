using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotMakerPlatform.Web.Areas.SupportBot.Repo
{
    public class WaitersRepo
    {
        private static readonly List<Subscriber> Waiters = new List<Subscriber>();

        private int BotInstanceId { get; }

        public WaitersRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public IEnumerable<Subscriber> GetAll()
        {
            return Waiters.Where(x => x.BotInstanceId == BotInstanceId);
        }

        public void Add(long chatId)
        {
            Remove(chatId);
            Waiters.Add(new Subscriber { BotInstanceId = BotInstanceId, ChatId = chatId });
        }

        public void Remove(long chatId)
        {
            Waiters.RemoveAll(x => x.BotInstanceId == BotInstanceId && x.ChatId == chatId);
        }
    }
}