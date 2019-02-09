using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BotMakerPlatform.Web.Repo
{
    public class SettingRepo
    {
        private int BotInstanceId { get; }
        private Db Db { get; }

        public SettingRepo(int botInstanceId, Db db)
        {
            BotInstanceId = botInstanceId;
            Db = db;
        }

        public T Load<T>() where T : ISettingable, new()
        {
            var settingRecord = Db.Settings.AsNoTracking().SingleOrDefault(x => x.BotInstanceRecordId == BotInstanceId);

            if (settingRecord == null)
                return (T)new T().Default();

            try
            {
                return JsonConvert.DeserializeObject<T>(settingRecord.Value);
            }
            catch (JsonException)
            {
                return (T)new T().Default();
            }
        }

        public void Save<T>(T setting)
        {
            var settingRecord = Db.Settings.SingleOrDefault(x => x.BotInstanceRecordId == BotInstanceId);

            if (settingRecord == null)
            {
                settingRecord = new SettingRecord { BotInstanceRecordId = BotInstanceId };
                Db.Settings.Add(settingRecord);
            }

            settingRecord.Value = JsonConvert.SerializeObject(setting);

            Db.SaveChanges();
        }
    }
}