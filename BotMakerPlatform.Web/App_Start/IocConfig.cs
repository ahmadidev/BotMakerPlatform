using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Integration.Mvc;
using Hangfire;
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
                    .InstancePerRequest()
                    .InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            else
                builder.RegisterType<TelegramBotClient>()
                    .WithParameter(tokenParameter)
                    .As<ITelegramBotClient>()
                    .InstancePerRequest()
                    .InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .AssignableTo<IBotInstance>()
                .InstancePerRequest()
                .InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(x => x.Name.EndsWith("Manager"))
                .InstancePerRequest()
                .InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(x => x.Name.EndsWith("Notifier"))
                .InstancePerRequest()
                .InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(x => x.Name.EndsWith("Repo"))
                .WithParameter(new ResolvedParameter(
                    (info, context) => info.Name == "botInstanceId",
                    (info, context) => HttpContext.Current.Request.GetOwinContext().Get<int>("BotInstanceId")))
                .InstancePerRequest()
                .InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            Container = builder.Build();

            GlobalConfiguration.Configuration.UseAutofacActivator(Container);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(Container));
        }
    }
}