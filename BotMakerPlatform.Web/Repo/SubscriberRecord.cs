namespace BotMakerPlatform.Web.Repo
{
    public class SubscriberRecord
    {
        /// <summary>
        /// Primary key. Its unique between all users and bots chats
        /// </summary>
        public long ChatId { get; set; }

        public int BotInstanceId { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}