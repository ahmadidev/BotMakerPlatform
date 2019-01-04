namespace BotMakerPlatform.Web.Areas.StoreBot.Record
{
    public class ImageFileRecord
    {
        public int Id { get; set; }

        public string ImageFileId { get; set; }

        public int StoreProductRecordId { get; set; }
        public virtual StoreProductRecord StoreProductRecord { get; set; }
    }
}