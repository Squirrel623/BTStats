using System;
using System.Collections.Generic;
using System.Text;

namespace BTStatsCorePopulator
{
    class TotalMessages : IMetric
    {
        public IDictionary<string, int> NumberMessagesDict { get; } = new Dictionary<string, int>();

        public TotalMessages()
        {
            foreach(var user in Users.InitialLoggedInUsers)
            {
                NumberMessagesDict[user] = 0;
            }
        }

        public void HandleLogMessage(TimestampMessage message)
        {
            var userMessage = message as UserTimestampMessage;
            if (userMessage?.Type != UserTimestampMessage.MessageType.Regular)
            {
                return;
            }

            if (!NumberMessagesDict.ContainsKey(userMessage.Username))
            {
                NumberMessagesDict[userMessage.Username] = 0;
            }

            NumberMessagesDict[userMessage.Username]++;
        }
    }
}
