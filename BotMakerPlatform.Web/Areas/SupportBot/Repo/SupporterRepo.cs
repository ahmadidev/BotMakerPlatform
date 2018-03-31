using System.Collections.Generic;
using System.Linq;

namespace BotMakerPlatform.Web.Areas.SupportBot.Repo
{
    public class SupporterRepo
    {
        private static readonly List<Subscriber> Supporters = new List<Subscriber>();

        private int BotInstanceId { get; }

        public SupporterRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public IEnumerable<Subscriber> GetAll()
        {
            return Supporters.Where(x => x.BotInstanceId == BotInstanceId);
        }

        public void Add(long chatId)
        {
            Remove(chatId);
            Supporters.Add(new Subscriber { BotInstanceId = BotInstanceId, ChatId = chatId });
        }

        public void Remove(long chatId)
        {
            Supporters.RemoveAll(x => x.BotInstanceId == BotInstanceId && x.ChatId == chatId);
        }
    }
}