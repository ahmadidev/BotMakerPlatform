using System;
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
}