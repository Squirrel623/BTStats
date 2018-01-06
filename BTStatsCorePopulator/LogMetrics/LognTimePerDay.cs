using System;
using System.Collections.Generic;
using System.Text;

namespace BTStatsCorePopulator
{
    public class LoginTimePerDay : BaseLoggedInMetric, IMetric
    {
        public TimeSpan Offset { get; private set; }

        private Dictionary<string, DateTimeOffset> lastLogin = new Dictionary<string, DateTimeOffset>();

        public IDictionary<string, IDictionary<DateTimeOffset, TimeSpan>> UserDailyLoginTimeDictionary = new Dictionary<string, IDictionary<DateTimeOffset, TimeSpan>>();

        public LoginTimePerDay(TimeSpan offset)
        {
            this.Offset = offset;

            foreach (string user in Users.InitialLoggedInUsers)
            {
                lastLogin[user] = new DateTimeOffset(2014, 1, 10, 0, 0, 0, offset);
                UserDailyLoginTimeDictionary[user] = new Dictionary<DateTimeOffset, TimeSpan>();
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

            var messageInOffset = userMessage.ToOffset(this.Offset);

            if (messageInOffset.Type == UserTimestampMessage.MessageType.Join)
            {
                UserJoin(messageInOffset);
            }
            else if (messageInOffset.Type == UserTimestampMessage.MessageType.Leave)
            {
                UserLeave(messageInOffset);
            }
        }

        private void UserJoin(UserTimestampMessage message)
        {
            if (loggedInUsers.Contains(message.Username))
            {
                //User logged in, they are already logged in - ERROR
                return;
            }

            if (!UserDailyLoginTimeDictionary.ContainsKey(message.Username))
            {
                UserDailyLoginTimeDictionary[message.Username] = new Dictionary<DateTimeOffset, TimeSpan>();
            }

            loggedInUsers.Add(message.Username);
            lastLogin[message.Username] = message.Timestamp;
        }

        private TimeSpan NiceTimespan(TimeSpan span)
        {
            return new TimeSpan(span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        public IEnumerable<Tuple<DateTimeOffset, TimeSpan>> GetTimespanForEachDate(DateTimeOffset first, DateTimeOffset last)
        {
            var returnList = new List<Tuple<DateTimeOffset, TimeSpan>>();
            var firstKey = new DateTimeOffset(first.Year, first.Month, first.Day, 0, 0, 0, this.Offset);
            var lastKey = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, this.Offset);

            DateTimeOffset endOfFirst = new DateTimeOffset(first.Year, first.Month, first.Day, 23, 59, 59, 999, this.Offset);
            DateTimeOffset beginningOfLast = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, this.Offset);

            returnList.Add(new Tuple<DateTimeOffset, TimeSpan>(firstKey, NiceTimespan(endOfFirst.Subtract(first))));

            var dateIterator = endOfFirst.AddMilliseconds(1);
            while(dateIterator < beginningOfLast)
            {
                returnList.Add(new Tuple<DateTimeOffset, TimeSpan>(dateIterator, new TimeSpan(1, 0, 0, 0)));
                dateIterator = dateIterator.AddDays(1);
            }

            returnList.Add(new Tuple<DateTimeOffset, TimeSpan>(lastKey, NiceTimespan(last.Subtract(beginningOfLast))));

            return returnList;
        }

        private TimeSpan AssignAndReturnSpanForDate(string username, DateTimeOffset date)
        {
            var userDictionary = UserDailyLoginTimeDictionary[username];

            if (userDictionary.ContainsKey(date))
            {
                return userDictionary[date];
            }

            var ts = new TimeSpan();
            userDictionary[date] = ts;

            return ts;
        }

        private void UserLeave(UserTimestampMessage message)
        {
            if (!loggedInUsers.Contains(message.Username))
            {
                //User logged out that wasn't logged in
                return;
            }

            loggedInUsers.Remove(message.Username);

            var userDictionary = UserDailyLoginTimeDictionary[message.Username];
            var loginDateTime = lastLogin[message.Username];
            var logoutDateTime = message.Timestamp;

            //Case 1: User logged in and out on the same day
            if (loginDateTime.Day == logoutDateTime.Day)
            {
                var date = new DateTimeOffset(logoutDateTime.Year, logoutDateTime.Month, logoutDateTime.Day, 0, 0, 0, this.Offset);

                var timespan = AssignAndReturnSpanForDate(message.Username, date);

                userDictionary[date] = timespan.Add(logoutDateTime.Subtract(loginDateTime));
                return;
            }

            //Case 2: User logged in on one date and out on another
            foreach(var dateTimespanTuple in GetTimespanForEachDate(loginDateTime, logoutDateTime))
            {
                var currentTimespan = AssignAndReturnSpanForDate(message.Username, dateTimespanTuple.Item1);
                userDictionary[dateTimespanTuple.Item1] = currentTimespan.Add(dateTimespanTuple.Item2);
            }
        }

    }
}
