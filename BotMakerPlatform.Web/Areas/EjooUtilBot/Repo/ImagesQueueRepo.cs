using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Record;
using BotMakerPlatform.Web.Repo;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot.Repo
{
    public class ImagesQueueRepo
    {
        private static readonly List<PhotoRecord> Images = new List<PhotoRecord>();

        private int BotInstanceId { get; }

        public ImagesQueueRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public IEnumerable<PhotoRecord> GetAll()
        {
            return Images.Where(x => x.BotInstanceId == BotInstanceId);
        }

        public void Add(SubscriberRecord subscriberRecord, PhotoSize photoSize, int messageId)
        {
            Images.Add(new PhotoRecord
            {
                BotInstanceId = BotInstanceId,
                PhotoSize = photoSize,
                MessageId =  messageId,
                ChatId = subscriberRecord.ChatId,
            });
        }

        public IEnumerable<PhotoRecord> GetCurrentSessionImages(SubscriberRecord subscriberRecord)
        {
            return GetAll().Where(x => x.ChatId == subscriberRecord.ChatId).OrderBy(x => x.MessageId);
        }

        public void ClearCurrentSessionImages(SubscriberRecord subscriberRecord)
        {
            Images.RemoveAll(x => x.BotInstanceId == BotInstanceId && x.ChatId == subscriberRecord.ChatId);
        }
    }
}