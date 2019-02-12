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

            modelBuilder.Entity<BotInstanceRecord>(builder =>
            {
                builder.Property(x => x.Id).ValueGeneratedNever();
            });


            modelBuilder.Entity<SubscriberRecord>(entity =>
            {
                entity.HasKey(x => new { x.BotInstanceRecordId, x.ChatId });
                entity.Property(x => x.ChatId).ValueGeneratedNever();
            });


            modelBuilder.Entity<SettingRecord>(builder =>
            {
                builder.HasKey(x => x.BotInstanceRecordId);
                builder.Property(x => x.BotInstanceRecordId).ValueGeneratedNever();
            });


            //Bots Data
            modelBuilder.Entity<StoreAdminRecord>(builder =>
            {
                builder.HasKey(x => new { x.BotInstanceRecordId, x.ChatId });
                builder.Property(x => x.ChatId).ValueGeneratedNever();
            });
        }
    }
}