namespace BotMakerPlatform.Web.Areas.StoreBot.Models
{
    public class Setting : ISettingable
    {
        public string WelcomeMessage { get; set; }

        public string ProductDetailTemplate { get; set; }

        public ISettingable Default()
        {
            return new Setting
            {
                WelcomeMessage = "به فروشگاه ما خوش آمدید",
                ProductDetailTemplate = "[Index]- **[Name]** ([Code]) [Price]\n" +
                                        "[Description]"
            };
        }
    }
}