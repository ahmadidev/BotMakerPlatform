using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Record;
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

        public void Add(Subscriber subscriber, PhotoSize photoSize)
        {
            Images.Add(new PhotoRecord
            {
                BotInstanceId = BotInstanceId,
                PhotoSize = photoSize,
                ChatId = subscriber.ChatId,
            });
        }

        public IEnumerable<PhotoRecord> GetCurrentSessionImages(Subscriber subscriber)
        {
            return GetAll().Where(x => x.ChatId == subscriber.ChatId);
        }

        public void ClearCurrentSessionImages(Subscriber subscriber)
        {
            Images.RemoveAll(x => x.BotInstanceId == BotInstanceId && x.ChatId == subscriber.ChatId);
        }
    }
}