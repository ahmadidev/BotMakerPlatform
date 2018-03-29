using System.Reflection;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Telegram.Bot;

namespace BotMakerPlatform.Web
{
    public class IocConfig
    {
        public static IContainer Container;

        public static void Config()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            if (Configuration.IsDebug)
                builder.RegisterType<SimulatorBotClient>().As<ITelegramBotClient>();
            else
                builder.RegisterType<TelegramBotClient>().As<ITelegramBotClient>();

            Container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(Container));
        }
    }
}