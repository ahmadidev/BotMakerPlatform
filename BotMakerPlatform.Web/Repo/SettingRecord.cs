namespace BotMakerPlatform.Web.Repo
{
    public class SettingRecord
    {
        public int BotInstanceRecordId { get; set; }
        public virtual BotInstanceRecord BotInstanceRecord { get; set; }

        public string Value { get; set; }
    }
}