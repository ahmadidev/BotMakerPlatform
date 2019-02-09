using Serilog;

namespace BotMakerPlatform.Web
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