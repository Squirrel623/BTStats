using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace BTStatsCorePopulator
{
    public class LoginUsernames : BaseLoggedInMetric, IMetric
    {
        public IImmutableSet<string> Usernames => loggedInUsers.ToImmutableHashSet();

        public void HandleLogMessage(TimestampMessage message)
        {
            var userMessage = message as UserTimestampMessage;
            if (userMessage?.Type != UserTimestampMessage.MessageType.Join)
            {
                return;
            }

            if (!loggedInUsers.Contains(userMessage.Username))
            {
                loggedInUsers.Add(userMessage.Username);
            }
        }
    }
}
