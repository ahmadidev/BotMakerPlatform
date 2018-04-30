﻿using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Record;

namespace BotMakerPlatform.Web.Areas.SupportBot.Repo
{
    public class SettingRepo
    {
        private static readonly List<SettingRecord> SettingRecords = new List<SettingRecord>();

        private int BotInstanceId { get; }
        public SettingRepo(int botInstanceId)
        {
            BotInstanceId = botInstanceId;
        }

        public void SetWelcomeMessage(string welcomeMessage)
        {
            var settingRecord = SettingRecords.SingleOrDefault(x => x.BotInstanceId == BotInstanceId);

            if (settingRecord == null)
            {
                settingRecord = new SettingRecord
                {
                    BotInstanceId = BotInstanceId,
                    WelcomeMessage = welcomeMessage
                };
                SettingRecords.Add(settingRecord);
            }
            else
                settingRecord.WelcomeMessage = welcomeMessage;
        }

        public string GetWelcomeMessage()
        {
            return SettingRecords.SingleOrDefault(x => x.BotInstanceId == BotInstanceId)?.WelcomeMessage;
        }
    }
}