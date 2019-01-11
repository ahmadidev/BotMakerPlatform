using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace BotMakerPlatform.Web.Repo
{
    public class BotDefinitionRepo
    {
        public static IEnumerable<IBotDefinition> BotDefinitions;

        static BotDefinitionRepo()
        {
            using (var scope = IocConfig.Container.BeginLifetimeScope())
            {
                var botDefinitions = IocConfig.Container.ComponentRegistry
                    .Registrations
                    .Where(x => x.Activator.LimitType.IsAssignableTo<IBotDefinition>())
                    .Select(x => (IBotDefinition)scope.Resolve(x.Activator.LimitType))
                    .ToList();

                BotDefinitions = botDefinitions;
            }
        }
    }
}