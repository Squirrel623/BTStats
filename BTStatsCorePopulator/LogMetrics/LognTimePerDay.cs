using System;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BTStatsCorePopulator
{
    public class LoginTimePerDay : BaseLoggedInMetric, IMetric
    {
        public DateTimeZone TimeZone { get; private set; }

        private Dictionary<string, ZonedDateTime> lastLogin = new Dictionary<string, ZonedDateTime>();

        public IDictionary<string, IDictionary<LocalDate, Duration>> UserDailyLoginTimeDictionary = new Dictionary<string, IDictionary<LocalDate, Duration>>();

        public LoginTimePerDay(DateTimeZone timezone)
        {
            this.TimeZone = timezone;

            foreach (string user in Users.InitialLoggedInUsers)
            {
                lastLogin[user] = new ZonedDateTime(Users.FirstLoginInstant, timezone);
                UserDailyLoginTimeDictionary[user] = new Dictionary<LocalDate, Duration>();
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

            var messageInOffset = userMessage.ToZone(this.TimeZone);

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
                UserDailyLoginTimeDictionary[message.Username] = new Dictionary<LocalDate, Duration>();
            }

            loggedInUsers.Add(message.Username);
            lastLogin[message.Username] = message.ZonedTimestamp.Value;
        }

        private TimeSpan NiceTimespan(TimeSpan span)
        {
            return new TimeSpan(span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        public IEnumerable<Tuple<LocalDate, Duration>> GetDurationForEachDate(ZonedDateTime first, ZonedDateTime last)
        {
            var returnList = new List<Tuple<LocalDate, Duration>>();
            var firstKey =  new LocalDate(first.Year, first.Month, first.Day);
            var lastKey = new LocalDate(last.Year, last.Month, last.Day);

            var localFirst = first.LocalDateTime;
            var localLast = last.LocalDateTime;

            //ZonedDateTime endOfFirst = first.Zone.AtStrictly(new LocalDateTime(first.Year, first.Month, first.Day, 23, 59, 59));
            //ZonedDateTime beginningOfLast = last.Zone.AtStrictly(new LocalDateTime(last.Year, last.Month, last.Day, 0, 0, 0));
            LocalDateTime endOfFirst = new LocalDateTime(first.Year, first.Month, first.Day, 23, 59, 59);
            LocalDateTime beginningOfLast = new LocalDateTime(last.Year, last.Month, last.Day, 0, 0, 0);

            //returnList.Add(new Tuple<LocalDate, Duration>(firstKey, endOfFirst.Minus(first)));
            returnList.Add(new Tuple<LocalDate, Duration>(firstKey, endOfFirst.Minus(localFirst).ToDuration()));

            var dateIterator = endOfFirst.PlusSeconds(1);

            HashSet<LocalDate> datesSeen = new HashSet<LocalDate>();
            datesSeen.Add(firstKey);

            while(dateIterator < beginningOfLast)
            {
                var date = new LocalDate(dateIterator.Year, dateIterator.Month, dateIterator.Day);

                if (datesSeen.Contains(date))
                {
                    var i = 10;
                }

                datesSeen.Add(date);

                returnList.Add(new Tuple<LocalDate, Duration>(date, Duration.FromDays(1)));
                dateIterator = dateIterator.PlusHours(24);
            }

            returnList.Add(new Tuple<LocalDate, Duration>(lastKey, localLast.Minus(beginningOfLast).ToDuration()));

            return returnList;
        }

        private Duration AssignAndReturnDurationForDate(string username, LocalDate date)
        {
            var userDictionary = UserDailyLoginTimeDictionary[username];

            if (userDictionary.ContainsKey(date))
            {
                return userDictionary[date];
            }

            var duration = new Duration();
            userDictionary[date] = duration;

            return duration;
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
            var logoutDateTime = message.ZonedTimestamp.Value;

            var loginDate = new LocalDate(loginDateTime.Year, loginDateTime.Month, loginDateTime.Day);
            var logoutDate = new LocalDate(logoutDateTime.Year, logoutDateTime.Month, logoutDateTime.Day);

            //Case 1: User logged in and out on the same day

            if (loginDate == logoutDate)
            {
                var date = new LocalDate(logoutDateTime.Year, logoutDateTime.Month, logoutDateTime.Day);

                var duration = AssignAndReturnDurationForDate(message.Username, date);

                if (duration > Duration.FromDays(1))
                {
                    var i = 11;
                }

                var delta = logoutDateTime.Minus(loginDateTime);
                if (delta.TotalTicks < 0)
                {
                    var g = 19;
                }

                var newDuration = duration.Plus(delta);

                if (newDuration > Duration.FromDays(1))
                {
                    var u = 39;
                }


                userDictionary[date] = newDuration;
                return;
            }

            //Case 2: User logged in on one date and out on another
            var dates = GetDurationForEachDate(loginDateTime, logoutDateTime).ToList();
            foreach (var dateTimespanTuple in dates)
            {
                var currentTimespan = AssignAndReturnDurationForDate(message.Username, dateTimespanTuple.Item1);
                userDictionary[dateTimespanTuple.Item1] = currentTimespan.Plus(dateTimespanTuple.Item2);

                if (userDictionary[dateTimespanTuple.Item1] > Duration.FromDays(1))
                {
                    var g = 10;
                }
            }
        }

    }
}
