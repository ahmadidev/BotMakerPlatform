using System;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using BotMakerPlatform.Web.Repo;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace BotMakerPlatform.Web
{
    public class IocConfig
    {
        public static IContainer Container;

        public static IServiceProvider Config(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);

            var currentAssembly = Assembly.GetExecutingAssembly();

            //builder.RegisterControllers(currentAssembly);

            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();

            var tokenParameter = new ResolvedParameter(
                (info, context) => info.Name == "token",
                (info, context) => context.Resolve<IHttpContextAccessor>().HttpContext.Items["BotClientToken"].ToString());

            if (Configuration.IsDebug)
                builder
                    .RegisterType<SimulatorBotClient>()
                    .WithParameter(tokenParameter)
                    .As<ITelegramBotClient>()
                    .InstancePerLifetimeScope();
            //.InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            else
                builder.RegisterType<TelegramBotClient>()
                    .WithParameter(tokenParameter)
                    .As<ITelegramBotClient>()
                    .InstancePerLifetimeScope();
            //.InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .AssignableTo<IBotDefinition>()
                .SingleInstance();

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .AssignableTo<IBotInstance>()
                .InstancePerLifetimeScope();
            //.InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(x => x.Name.EndsWith("Manager"))
                .InstancePerLifetimeScope();
            //.InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(x => x.Name.EndsWith("Notifier"))
                .InstancePerLifetimeScope();
            //.InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(x => x.Name.EndsWith("Repo"))
                .WithParameter(new ResolvedParameter(
                    (info, context) => info.Name == "botInstanceId",
                    (info, context) => context.Resolve<IHttpContextAccessor>().HttpContext.Items["BotInstanceId"].ToString()))
                .InstancePerLifetimeScope();
            //.InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            builder
                .RegisterType<Db>()
                .AsSelf()
                .InstancePerLifetimeScope();
            //.InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            Container = builder.Build();

            //GlobalConfiguration.Configuration.UseAutofacActivator(Container);
            //DependencyResolver.SetResolver(new AutofacDependencyResolver(Container));
            return new AutofacServiceProvider(Container);
        }
    }
}