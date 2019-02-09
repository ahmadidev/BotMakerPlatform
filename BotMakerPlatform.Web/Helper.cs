using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;

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

        public static string ToEnglishDigits(this string text)
        {
            var faNativeDigits = FaCulture.NumberFormat.NativeDigits;
            var arNativeDigits = CultureInfo.GetCultureInfo("ar").NumberFormat.NativeDigits;

            for (var i = 0; i < faNativeDigits.Length; i++)
                text = text.Replace(faNativeDigits[i], i.ToString());

            for (var i = 0; i < arNativeDigits.Length; i++)
                text = text.Replace(faNativeDigits[i], i.ToString());

            return text;
        }

        public static string GetUserId(this IIdentity identity)
        {
            return (identity as ClaimsIdentity).FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}