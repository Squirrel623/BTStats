using System;
using NodaTime;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BTStatsCorePopulator
{
    public class TimestampMessage
    {
        public static TimestampMessage Create(LocalDate baseDate, string rawMessage)
        {
            if (!LogRegex.TimestampMessage.IsMatch(rawMessage))
            {
                return null;
            }

            if (LogRegex.UserMessage.IsMatch(rawMessage) ||
                LogRegex.UserJoin.IsMatch(rawMessage) ||
                LogRegex.UserLeave.IsMatch(rawMessage))
            {
                return new UserTimestampMessage(baseDate, rawMessage);
            }

            return new TimestampMessage(baseDate, rawMessage);
        }

        public OffsetDateTime Timestamp { get; protected set; }

        public ZonedDateTime? ZonedTimestamp { get; protected set; }

        public string RawContent { get; protected set; }

        public TimestampMessage(LocalDate baseDate, string rawMessage)
        {
            var match = LogRegex.TimestampMessage.Match(rawMessage);

            if (!match.Success)
            {
                throw new Exception("Not a timestamp message");
            }

            if (match.Groups.Count < 6)
            {
                throw new Exception("Unexpected number of captures");
            }

            int hour, minute, second, offset;
            if (!int.TryParse(match.Groups[1].Value, out hour) ||
                !int.TryParse(match.Groups[2].Value, out minute) ||
                !int.TryParse(match.Groups[3].Value, out second) ||
                !int.TryParse(match.Groups[4].Value, out offset))
            {
                throw new Exception("Error extracting time values");
            }

            int hours = offset / 100;
            int minutes = offset % 100;
            Offset tsOffset = Offset.FromHoursAndMinutes(hours, minutes);

            var localDateTime = new LocalDateTime(baseDate.Year, baseDate.Month, baseDate.Day, hour, minute, second);
            Timestamp = new OffsetDateTime(localDateTime, tsOffset);
            RawContent = match.Groups[5].Value.Trim();
        }

        public TimestampMessage(TimestampMessage message)
        {
            this.Timestamp = message.Timestamp;
            this.RawContent = message.RawContent;
            this.ZonedTimestamp = message.ZonedTimestamp;
        }

        public TimestampMessage ToZone(DateTimeZone zone)
        {
            return new TimestampMessage(this)
            {
                ZonedTimestamp = new ZonedDateTime(this.Timestamp.ToInstant(), zone)
            };
        }
    }

    public class UserTimestampMessage : TimestampMessage
    {
        public enum MessageType
        {
            Join,
            Leave,
            Regular
        }

        public string Username { get; private set; }

        public MessageType Type { get; private set; }

        public UserTimestampMessage(LocalDate baseDate, string rawMessage) : base(baseDate, rawMessage)
        {
            if (LogRegex.UserMessage.IsMatch(rawMessage))
            {
                InitializeUserMessage(rawMessage);
            }
            else if (LogRegex.UserJoin.IsMatch(rawMessage))
            {
                InitializeUserJoin(rawMessage);
            }
            else if (LogRegex.UserLeave.IsMatch(rawMessage))
            {
                InitializeUserLeave(rawMessage);
            }
            else
            {
                throw new Exception("Not valid user message");
            }
        }

        public UserTimestampMessage(UserTimestampMessage message) : base(message)
        {
            this.Username = message.Username;
            this.Type = message.Type;
        }

        public new UserTimestampMessage ToZone(DateTimeZone zone)
        {
            return new UserTimestampMessage(this)
            {
                ZonedTimestamp = new ZonedDateTime(this.Timestamp.ToInstant(), zone)
            };
        }

        private void InitializeUserMessage(string message)
        {
            var match = LogRegex.UserMessage.Match(message);

            if (!match.Success || match.Groups.Count < 2)
            {
                throw new Exception("Bad Match");
            }

            Username = match.Groups[1].Value;
            Type = MessageType.Regular;
        }

        private void InitializeUserJoin(string message)
        {
            var match = LogRegex.UserJoin.Match(message);

            if (!match.Success || match.Groups.Count < 2)
            {
                throw new Exception("Bad Match");
            }

            Username = match.Groups[1].Value;
            Type = MessageType.Join;
        }

        private void InitializeUserLeave(string message)
        {
            var match = LogRegex.UserLeave.Match(message);

            if (!match.Success || match.Groups.Count < 2)
            {
                throw new Exception("Bad Match");
            }

            Username = match.Groups[1].Value;
            Type = MessageType.Leave;
        }
    }
}
