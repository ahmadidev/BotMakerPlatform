using System.Collections.Generic;
using BotMakerPlatform.Web.Repo;

namespace BotMakerPlatform.Web.Areas.StoreBot.Record
{
    public class StoreProductRecord
    {
        public StoreProductRecord()
        {
            ImageFileRecords = new HashSet<ImageFileRecord>();
        }

        public int Id { get; set; }

        public string Name { get; set; }
        public string Code { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public virtual ICollection<ImageFileRecord> ImageFileRecords { get; set; }

        public int BotInstanceRecordId { get; set; }
        public virtual BotInstanceRecord BotInstanceRecord { get; set; }
    }
}