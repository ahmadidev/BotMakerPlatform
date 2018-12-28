using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using BotMakerPlatform.Web.Repo;

namespace BotMakerPlatform.Web.Areas.StoreBot.Repo
{
    public class StoreAdminRepo
    {
        private int BotInstanceId { get; }
        private Db Db { get; }

        public StoreAdminRepo(int botInstanceId, Db db)
        {
            BotInstanceId = botInstanceId;
            Db = db;
        }

        public StoreAdminRecord GetAdmin(long chatId)
        {
            return Db.StoreAdminRecords.AsNoTracking().SingleOrDefault(x => x.BotInstanceRecordId == BotInstanceId && x.ChatId == chatId);
        }

        public IEnumerable<StoreAdminRecord> GetAllAdmins()
        {
            return Db.StoreAdminRecords.AsNoTracking();
        }

        public void AddAdmin(long chatId)
        {
            Db.StoreAdminRecords.Add(new StoreAdminRecord { ChatId = chatId, BotInstanceRecordId = BotInstanceId });
            Db.SaveChanges();
        }

        public void RemoveAdmin(long chatId)
        {
            Db.Entry(new StoreAdminRecord { ChatId = chatId, BotInstanceRecordId = BotInstanceId }).State = EntityState.Deleted;
            Db.SaveChanges();
        }
    }
}