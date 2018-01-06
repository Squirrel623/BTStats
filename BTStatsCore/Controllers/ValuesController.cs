﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using BTStatsCorePopulator;
using System.Globalization;

namespace BTStatsCore.Controllers
{
    public struct UserLoginTimePerDay
    {
        public string Date { get; }
        public ulong Milliseconds { get; }

        public UserLoginTimePerDay(DateTimeOffset offset, TimeSpan span)
        {
            Date = $"{offset.Year}-{offset.Month.ToString("D2")}-{offset.Day.ToString("D2")}";
            Milliseconds = (uint)span.TotalMilliseconds;
        }
    }

    public struct CalendarEntry
    {
        private static GregorianCalendar calendar = new GregorianCalendar();

        //public int Year { get; }
        //public int Month { get; }
        public DayOfWeek DayOfWeekStart { get; }
        public int DaysInMonth { get; }

        public CalendarEntry(int year, int month)
        {
            //Year = year;
            //Month = month;

            DayOfWeekStart = calendar.GetDayOfWeek(new DateTime(year, month, 1));
            DaysInMonth = calendar.GetDaysInMonth(year, month);
        }
    }

    public class ValuesController : Controller
    {
        private readonly StatsProvider statsProvider;

        private readonly IDictionary<int, IDictionary<int, CalendarEntry>> calendarDictionary;

        public ValuesController(StatsProvider provider)
        {
            statsProvider = provider;

            calendarDictionary = new Dictionary<int, IDictionary<int, CalendarEntry>>();

            var entries = new List<CalendarEntry>();
            foreach(var year in Enumerable.Range(2014, 5))
            {
                var yearDict = calendarDictionary[year] = new Dictionary<int, CalendarEntry>();

                foreach(var month in Enumerable.Range(1, 12))
                {
                    yearDict[month] = new CalendarEntry(year: year, month: month);
                }
            }
        }

        [HttpGet("/calendar")]
        public IDictionary<int, IDictionary<int, CalendarEntry>> GetCalendar()
        {
            return calendarDictionary;
        }

        [HttpGet("/users")]
        public async Task<IEnumerable<string>> GetUsers()
        {
            return await statsProvider.GetUsernames();
        }

        // GET api/values/loginCount/user
        [HttpGet("/loginCount/{user}")]
        public async Task<int> GetLoginCount(string user)
        {
            return await statsProvider.GetUserLoginCount(user);
        }

        // GET api/values/loggedInTime/user
        [HttpGet("/loggedInTime/{user}")]
        public async Task<double> GetLoggedInTime(string user)
        {
            TimeSpan loggedInTime = await statsProvider.GetUserLoggedInTime(user);
            return loggedInTime.TotalSeconds;
        }

        [HttpGet("/messageCount/{user}")]
        public async Task<int> GetMessageCount(string user)
        {
            return await statsProvider.GetTotalMessages(user);
        }

        [HttpGet("/emotes/{user}/{count}")]
        public async Task<IReadOnlyDictionary<string, int>> GetUserEmotes(string user, int count)
        {
            return await statsProvider.GetUserEmotes(user, count);
        }

        [HttpGet("/emotes/{user}/emote/{emote}")]
        public async Task<IReadOnlyDictionary<string, int>> GetUserEmoteEffects(string user, string emote)
        {
            return await statsProvider.GetEffectsForEmote(user, emote);
        }

        // GET api/values/loggedInTimePerDay/user
        [HttpGet("/loggedInTimePerDay/{offset}/{user}")]
        public async Task<IEnumerable<UserLoginTimePerDay>> GetLoggedInTimePerDay(int offset, string user)
        {
            TimeSpan tsOffset;
            try
            {
                tsOffset = new TimeSpan(offset, 0, 0);
            }
            catch
            {
                return Enumerable.Empty<UserLoginTimePerDay>();
            }

            var dict = await statsProvider.GetUserLoggedInTimePerDay(tsOffset, user) ?? new Dictionary<DateTimeOffset, TimeSpan>();

            return dict
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => new UserLoginTimePerDay(kvp.Key, kvp.Value));
        }
    }
}
