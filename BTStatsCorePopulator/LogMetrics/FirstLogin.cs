using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace BTStatsCorePopulator
{
    public class FirstLogin : IMetric
    {
        public Dictionary<string, LocalDate> FirstLoginDict { get; }

        public FirstLogin()
        {
            FirstLoginDict = new Dictionary<string, LocalDate>();

            var firstLoginDateTime = Users.FirstLogin.LocalDateTime;
            var firstLoginDate = new LocalDate(firstLoginDateTime.Year, firstLoginDateTime.Month, firstLoginDateTime.Day);
            foreach(var user in Users.InitialLoggedInUsers)
            {
                FirstLoginDict[user] = firstLoginDate;
            }
        }

        public void HandleLogMessage(TimestampMessage message)
        {
            var userMessage = message as UserTimestampMessage;
            if (userMessage?.Type != UserTimestampMessage.MessageType.Join)
            {
                return;
            }

            if (!FirstLoginDict.ContainsKey(userMessage.Username))
            {
                FirstLoginDict[userMessage.Username] = new LocalDate(message.Timestamp.Year, message.Timestamp.Month, message.Timestamp.Day);
            }
        }
    }
}
