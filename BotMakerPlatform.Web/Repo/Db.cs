using BotMakerPlatform.Web.Areas.StoreBot.Record;
using BotMakerPlatform.Web.Areas.StoreBot.Repo;
using Microsoft.EntityFrameworkCore;

namespace BotMakerPlatform.Web.Repo
{
    public class Db : DbContext
    {
        //public Db() : base("DefaultConnectionString")
        //{
        //    Database.SetInitializer(new MigrateDatabaseToLatestVersion<Db, Migrations.Configuration>(useSuppliedContext: true));
        //}

        public Db(DbContextOptions<Db> contextOptions) : base(contextOptions)
        {
        }

        public DbSet<BotInstanceRecord> BotInstanceRecords { get; set; }
        public DbSet<SubscriberRecord> Subscribers { get; set; }
        public DbSet<SettingRecord> Settings { get; set; }

        //Bot Modules
        public DbSet<StoreProductRecord> StoreProductRecords { get; set; }
        public DbSet<StoreAdminRecord> StoreAdminRecords { get; set; }
        public DbSet<ImageFileRecord> ImageFileRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<BotInstanceRecord>()
                .Property(x => x.Id)
                .ValueGeneratedNever();



            modelBuilder
                .Entity<SubscriberRecord>()
                .HasKey(x => new { x.BotInstanceRecordId, x.ChatId });

            modelBuilder
                .Entity<SubscriberRecord>()
                .Property(x => x.ChatId)
                .ValueGeneratedNever();



            modelBuilder
                .Entity<SettingRecord>()
                .HasKey(x => x.BotInstanceRecordId);

            modelBuilder
                .Entity<SettingRecord>()
                .Property(x => x.BotInstanceRecordId)
                .ValueGeneratedNever();



            //Bots Data
            modelBuilder
                .Entity<StoreAdminRecord>()
                .HasKey(x => new { x.BotInstanceRecordId, x.ChatId });

            modelBuilder
                .Entity<StoreAdminRecord>()
                .Property(x => x.ChatId)
                .ValueGeneratedNever();
        }
    }
}