using System.Collections.Generic;
using System.Linq;

namespace BotMakerPlatform.Web.Areas.SupportBot.Repo
{
    public class ConnectionRepo
    {
        private static readonly List<Connection> connections = new List<Connection>();

        private int BotInstanceId { get; }

        public ConnectionRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public IEnumerable<Connection> GetAll()
        {
            return connections.Where(x => x.BotInstanceId == BotInstanceId);
        }

        public void Add(Connection connection)
        {
            Remove(connection);
            connections.Add(new Connection { BotInstanceId = BotInstanceId, UserChatId = connection.UserChatId, SupporterChatId = connection.SupporterChatId });
        }

        public void Remove(Connection connection)
        {
            connections.RemoveAll(x => x.BotInstanceId == BotInstanceId && x.SupporterChatId == connection.SupporterChatId && x.UserChatId == connection.UserChatId);
        }
    }
}