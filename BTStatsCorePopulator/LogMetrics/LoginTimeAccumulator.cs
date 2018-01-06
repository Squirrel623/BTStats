using System;
using System.Collections.Generic;
using System.Text;

namespace BTStatsCorePopulator
{
    public class LoginTimeAccumulator : BaseLoggedInMetric, IMetric
    {
        public IDictionary<string, TimeSpan> UserTimeSpans { get; } = new Dictionary<string, TimeSpan>();

        private Dictionary<string, DateTime> lastLogin = new Dictionary<string, DateTime>();

        public LoginTimeAccumulator()
        {
            foreach(string user in Users.InitialLoggedInUsers)
            {
                lastLogin[user] = new DateTime(2014, 1, 10, 22, 0, 0);
                UserTimeSpans[user] = new TimeSpan();
            }
        }


        public void HandleLogMessage(TimestampMessage message)
        {
            var userMessage = message as UserTimestampMessage;
            if (userMessage?.Type != UserTimestampMessage.MessageType.Join &&
                userMessage?.Type != UserTimestampMessage.MessageType.Leave)
            {
                return;
            }

            if (userMessage.Type == UserTimestampMessage.MessageType.Join)
            {
                UserJoin(userMessage);
            }
            else if (userMessage.Type == UserTimestampMessage.MessageType.Leave)
            {
                UserLeave(userMessage);
            }
        }

        private void UserJoin(UserTimestampMessage message)
        {
            if (loggedInUsers.Contains(message.Username))
            {
                //User logged in, they are already logged in - ERROR
                return;
            }

            if (!UserTimeSpans.ContainsKey(message.Username))
            {
                UserTimeSpans[message.Username] = new TimeSpan();
            }

            loggedInUsers.Add(message.Username);
            lastLogin[message.Username] = message.Timestamp.DateTime;
        }

        private void UserLeave(UserTimestampMessage message)
        {
            if (!loggedInUsers.Contains(message.Username))
            {
                //User logged out that wasn't logged in
                return;
            }

            loggedInUsers.Remove(message.Username);

            var loginDateTime = lastLogin[message.Username];
            var logoutDateTime = message.Timestamp.DateTime;
            var currentTimespan = UserTimeSpans[message.Username];

            var newSpan = currentTimespan.Add(logoutDateTime.Subtract(loginDateTime));

            if (newSpan.Ticks < currentTimespan.Ticks)
            {
                return;
            }

            UserTimeSpans[message.Username] = newSpan;
        }
    }
}
