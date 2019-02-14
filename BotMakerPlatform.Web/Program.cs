using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Debugging;

namespace BotMakerPlatform.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) => builder
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .Build()
                )
                .UseSerilog((context, configuration) =>
                {
                    SelfLog.Enable(s =>
                    {
                        var env = context.HostingEnvironment;
                        var path = Path.Combine(env.ContentRootPath, $@"App_Data\{DateTime.Now:yyyy-MM-dd}.txt");
                        File.AppendAllLines(path, new[] { s });
                    });

                    configuration
                        .ReadFrom.Configuration(context.Configuration);
                })
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<Startup>();
    }
}