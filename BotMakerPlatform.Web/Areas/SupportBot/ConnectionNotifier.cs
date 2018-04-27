using System;
using System.Collections.Generic;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class ConnectionNotifier
    {
        private readonly List<Action> _actions = new List<Action>();

        public void CustomerDisconnected()
        {
            _actions.ForEach(x => x.Invoke());
        }

        public void NotifyOnCustomerDisconnect(Action action)
        {
            _actions.Add(action);
        }
    }
}