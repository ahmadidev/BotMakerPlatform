using System.Data.Entity;

namespace BotMakerPlatform.Web.Repo
{
    public class Db : DbContext
    {
        public Db() : base("DefaultConnectionString")
        {

        }

        public IDbSet<BotInstanceRecord> BotInstanceRecords { get; set; }
    }
}