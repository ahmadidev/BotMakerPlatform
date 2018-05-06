using System;
using System.Collections.Generic;
using System.Linq;

namespace BotMakerPlatform.Web.Repo
{
    public class BotDefinitionRepo
    {
        public static IEnumerable<IBotDefinition> BotDefinitions;

        static BotDefinitionRepo()
        {
            var bots = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(x => typeof(IBotDefinition).IsAssignableFrom(x) && !x.IsInterface)
                .Select(x => (IBotDefinition)Activator.CreateInstance(x));

            BotDefinitions = bots;
        }
    }
}