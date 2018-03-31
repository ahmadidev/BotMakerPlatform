using System.Collections.Generic;
using System.Linq;

namespace BotMakerPlatform.Web.Repo
{
    public class SubscriberRepo
    {
        private static readonly List<Subscriber> Subscribers = new List<Subscriber>();

        private int BotInstanceId { get; }

        public SubscriberRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public IEnumerable<Subscriber> GetAll()
        {
            return Subscribers.Where(x => x.BotInstanceId == BotInstanceId);
        }

        public Subscriber GetByChatId(long chatId)
        {
            return GetAll().SingleOrDefault(x => x.ChatId == chatId);
        }

        public void Add(long chatId, string username, string firstName, string lastName)
        {
            Subscribers.Add(new Subscriber
            {
                BotInstanceId = BotInstanceId,
                ChatId = chatId,
                Username = username,
                FirstName = firstName,
                LastName = lastName
            });
        }
    }
}