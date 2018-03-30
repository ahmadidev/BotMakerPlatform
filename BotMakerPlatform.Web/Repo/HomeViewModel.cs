using System.Collections.Generic;

namespace BotMakerPlatform.Web.Repo
{
    public class HomeViewModel
    {
        public List<HomeBotDto> Bots { get; set; }
    }

    public class HomeBotDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }

        public string WebhookSecret { get; set; }
    }

    public class BotUserDto
    {
        public long ChatId { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}