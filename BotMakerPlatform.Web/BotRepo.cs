using System;
using System.Collections.Generic;
using System.Linq;

namespace BotMakerPlatform.Web
{
    public class BotRepo
    {
        public static IEnumerable<IBot> Bots;

        static BotRepo()
        {
            var bots = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(x => typeof(IBot).IsAssignableFrom(x) && !x.IsInterface)
                .Select(x => (IBot)Activator.CreateInstance(x));

            Bots = bots;
        }
    }
}