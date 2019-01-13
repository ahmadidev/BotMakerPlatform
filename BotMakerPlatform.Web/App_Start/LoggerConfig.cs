using BotMakerPlatform.Web;
using Microsoft.Owin;
using Serilog;

[assembly: OwinStartup(typeof(Startup))]

namespace BotMakerPlatform.Web
{
    public partial class Startup
    {
        public class LoggerConfig
        {
            public static void Config()
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.MSSqlServer(
                        connectionString: "DefaultConnectionString",
                        tableName: "Logs",
                        autoCreateSqlTable: true
                    ).CreateLogger();
            }
        }
    }
}