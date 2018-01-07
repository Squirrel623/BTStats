using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace BTStatsCorePopulator
{
    public class LoginTimeAccumulator : BaseLoggedInMetric, IMetric
    {
        public IDictionary<string, Duration> UserTimeSpans { get; } = new Dictionary<string, Duration>();

        private Dictionary<string, OffsetDateTime> lastLogin = new Dictionary<string, OffsetDateTime>();

        public LoginTimeAccumulator()
        {
            var localStartDate = new LocalDateTime(2014, 1, 10, 22, 0, 0);
            foreach(string user in Users.InitialLoggedInUsers)
            {

                lastLogin[user] = new OffsetDateTime(localStartDate, Offset.Zero);
                UserTimeSpans[user] = Duration.Zero;
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
                UserTimeSpans[message.Username] = Duration.Zero;
            }

            loggedInUsers.Add(message.Username);
            lastLogin[message.Username] = message.Timestamp;
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
            var logoutDateTime = message.Timestamp;
            var currentDuration = UserTimeSpans[message.Username];

            var newDuration = currentDuration.Plus(logoutDateTime.Minus(loginDateTime));

            if (newDuration.TotalTicks < currentDuration.TotalTicks)
            {
                return;
            }

            UserTimeSpans[message.Username] = newDuration;
        }
    }
}
