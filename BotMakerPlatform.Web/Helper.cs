using System.Globalization;

namespace BotMakerPlatform.Web
{
    public static class StringExtensions
    {
        private static readonly CultureInfo FaCulture = new CultureInfo("fa-IR");

        public static bool HasText(this string text)
        {
            return !string.IsNullOrEmpty(text);
        }

        public static string ToCurrency(this int number)
        {
            FaCulture.NumberFormat.CurrencyPositivePattern = 3;

            var text = number.ToString("C0", FaCulture);

            for (var i = 0; i < FaCulture.NumberFormat.NativeDigits.Length; i++)
                text = text.Replace(i.ToString(), FaCulture.NumberFormat.NativeDigits[i]);

            return text;
        }

        public static string ToPersian(this int number)
        {
            var text = number.ToString();

            for (var i = 0; i < FaCulture.NumberFormat.NativeDigits.Length; i++)
                text = text.Replace(i.ToString(), FaCulture.NumberFormat.NativeDigits[i]);

            return text;
        }
    }
}