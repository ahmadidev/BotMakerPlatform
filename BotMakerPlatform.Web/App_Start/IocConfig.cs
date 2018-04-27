using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Core;
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
            var currentAssembly = Assembly.GetExecutingAssembly();

            builder.RegisterControllers(currentAssembly);

            var tokenParameter = new ResolvedParameter(
                (info, context) => info.Name == "token",
                (info, context) => HttpContext.Current.Request.GetOwinContext().Get<string>("BotClientToken"));

            if (Configuration.IsDebug)
                builder
                    .RegisterType<SimulatorBotClient>()
                    .WithParameter(tokenParameter)
                    .As<ITelegramBotClient>()
                    .InstancePerRequest();
            else
                builder.RegisterType<TelegramBotClient>()
                    .WithParameter(tokenParameter)
                    .As<ITelegramBotClient>()
                    .InstancePerRequest();

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .AssignableTo<IBotInstance>()
                .InstancePerRequest();

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(x => x.Name.EndsWith("Manager"))
                .InstancePerRequest();

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(x => x.Name.EndsWith("Notifier"))
                .InstancePerRequest();

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(x => x.Name.EndsWith("Repo"))
                .WithParameter(new ResolvedParameter(
                    (info, context) => info.Name == "botInstanceId",
                    (info, context) => HttpContext.Current.Request.GetOwinContext().Get<int>("BotInstanceId")))
                .InstancePerRequest();

            Container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(Container));
        }
    }
}