using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace BTStatsCorePopulator
{
    public class FirstLastLogin : IMetric
    {
        public Dictionary<string, LocalDate> FirstLoginDict { get; }
        public Dictionary<string, LocalDate> LastLoginDict { get; }

        public FirstLastLogin()
        {
            FirstLoginDict = new Dictionary<string, LocalDate>();
            LastLoginDict = new Dictionary<string, LocalDate>();

            var firstLoginDateTime = Users.FirstLogin.LocalDateTime;
            var firstLoginDate = new LocalDate(firstLoginDateTime.Year, firstLoginDateTime.Month, firstLoginDateTime.Day);
            foreach(var user in Users.InitialLoggedInUsers)
            {
                FirstLoginDict[user] = firstLoginDate;
                LastLoginDict[user] = firstLoginDate;
            }
        }

        public void HandleLogMessage(TimestampMessage message)
        {
            var userMessage = message as UserTimestampMessage;
            if (userMessage?.Type != UserTimestampMessage.MessageType.Join)
            {
                return;
            }

            var date = new LocalDate(message.Timestamp.Year, message.Timestamp.Month, message.Timestamp.Day);
            LastLoginDict[userMessage.Username] = date;

            if (!FirstLoginDict.ContainsKey(userMessage.Username))
            {
                FirstLoginDict[userMessage.Username] = date;
            }
        }
    }
}
