using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Record;
using BotMakerPlatform.Web.Repo;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot.Repo
{
    public class ImagesCountRepo
    {
        private static readonly List<ImageCountRecord> ImagesCount = new List<ImageCountRecord>();

        private int BotInstanceId { get; }

        public ImagesCountRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public IEnumerable<ImageCountRecord> GetAll()
        {
            return ImagesCount.Where(x => x.BotInstanceId == BotInstanceId);
        }

        public void Add(SubscriberRecord subscriberRecord, int messageId)
        {
            var imageCountRecord = new ImageCountRecord
            {
                BotInstanceId = BotInstanceId,
                EditingMessageId = messageId,
                ChatId = subscriberRecord.ChatId,
            };

            if (GetAll().Any(x => x.ChatId == subscriberRecord.ChatId))
            {
                GetAll().SingleOrDefault(x => x.ChatId == subscriberRecord.ChatId).EditingMessageId = messageId;
            }
            else
            {
                ImagesCount.Add(imageCountRecord);
            }
        }

        public ImageCountRecord GetCurrentSessionImagesCountRecord(SubscriberRecord subscriberRecord)
        {
            return GetAll().SingleOrDefault(x => x.ChatId == subscriberRecord.ChatId);
        }
    }
}