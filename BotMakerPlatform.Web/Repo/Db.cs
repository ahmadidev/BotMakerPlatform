using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using BotMakerPlatform.Web.Areas.StoreBot;
using BotMakerPlatform.Web.Areas.StoreBot.Record;

namespace BotMakerPlatform.Web.Repo
{
    public class Db : DbContext
    {
        public Db() : base("DefaultConnectionString")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<Db, Migrations.Configuration>(useSuppliedContext: true));
        }

        public IDbSet<BotInstanceRecord> BotInstanceRecords { get; set; }
        public IDbSet<SubscriberRecord> Subscribers { get; set; }
        public IDbSet<SettingRecord> Settings { get; set; }

        //Bot Modules
        public IDbSet<StoreProductRecord> StoreProductRecords { get; set; }
        public IDbSet<StoreAdminRecord> StoreAdminRecords { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<BotInstanceRecord>()
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder
                .Entity<SubscriberRecord>()
                .HasKey(x => new { x.BotInstanceRecordId, x.ChatId })
                .Property(x => x.ChatId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder
                .Entity<SettingRecord>()
                .HasKey(x => x.BotInstanceRecordId)
                .Property(x => x.BotInstanceRecordId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            //Bots Data
            modelBuilder
                .Entity<StoreAdminRecord>()
                .HasKey(x => new { x.BotInstanceRecordId, x.ChatId })
                .Property(x => x.ChatId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }
}