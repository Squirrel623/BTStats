using System;
using System.Collections.Generic;
using System.Text;

namespace BTStatsCorePopulator
{
    public class LoginCounter : IMetric
    {
        public Dictionary<string, int> LoginCount { get; }

        public LoginCounter()
        {
            LoginCount = new Dictionary<string, int>();
        }

        public void HandleLogMessage(TimestampMessage message)
        {
            var userMessage = message as UserTimestampMessage;
            if (userMessage?.Type != UserTimestampMessage.MessageType.Join)
            {
                return;
            }

            if (!LoginCount.ContainsKey(userMessage.Username))
            {
                LoginCount[userMessage.Username] = 0;
            }

            LoginCount[userMessage.Username]++;
        }
    }
}
