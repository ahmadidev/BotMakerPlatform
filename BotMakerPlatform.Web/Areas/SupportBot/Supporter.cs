using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class Supporter
    {
        public Subscriber Subscriber { get; private set; }

        public List<Subscriber> WaitingList;

        public Supporter(Subscriber subscriber)
        {
            Subscriber = subscriber;
            WaitingList = new List<Subscriber>();
        }
    }
}