using System;
using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using BotMakerPlatform.Web.Repo;
using Hangfire;
using Owin;

namespace BotMakerPlatform.Web
{
    public class SchedulerConfig
    {
        public static void Config(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnectionString");

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            RecurringJob.AddOrUpdate(() => Console.WriteLine("Daily Job"), Cron.Minutely);
        }
    }

    public class HandleExpiredConnectionsJob
    {
        private BotInstanceRepo BotInstanceRepo { get; }

        public HandleExpiredConnectionsJob(BotInstanceRepo botInstanceRepo)
        {
            BotInstanceRepo = botInstanceRepo;
        }

        public void Execute()
        {
            foreach (var botInstanceRecord in BotInstanceRepo.BotInstanceRecords)
            {
                var connections = new ConnectionRepo(botInstanceRecord.Id).GetAll();
                var expireMinutes = new SettingRepo(botInstanceRecord.Id).GetExpireMinutes();
                var expiredConnections = connections.Where(x => x.CreatedAt.AddMinutes(expireMinutes) > DateTime.UtcNow);

                //new WaitingManager().CustomerDisconnected();

                //TODO: How to resolve dependencies?
            }
        }
    }
}