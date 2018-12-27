namespace BotMakerPlatform.Web
{
    public static class StringExtensions
    {
        public static bool HasText(this string text)
        {
            return !string.IsNullOrEmpty(text);
        }
    }
}