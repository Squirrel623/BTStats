using Microsoft.Extensions.Hosting;
using NodaTime;
using NodaTime.TimeZones;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTStatsCorePopulator
{
    public class StatsProvider : BackgroundService
    {
        static string LogDirectory = Environment.GetEnvironmentVariable("LogDir");

        public Task InitializeTask { get; private set; }

        private readonly IEnumerable<string> logFiles;

        private LoginUsernames usernameMetric;
        private LoginCounter loginCountMetric;
        private LoginTimeAccumulator loginTimeMetric;
        private TotalMessages messageCountMetric;
        private UserEmotes emoteCountMetric;

        private List<LoginTimePerDay> dailyLoginTimeMetrics;
        private List<IMetric> metrics;

        public StatsProvider()
        {
            if (string.IsNullOrEmpty(LogDirectory))
            {
                LogDirectory = @"C:\Users\Brady\Documents\BTLogs2";
            }

            logFiles = Directory.EnumerateFiles(LogDirectory) as IEnumerable<string>;

            usernameMetric = new LoginUsernames();
            loginCountMetric = new LoginCounter();
            loginTimeMetric = new LoginTimeAccumulator();
            messageCountMetric = new TotalMessages();
            emoteCountMetric = new UserEmotes();

            TzdbDateTimeZoneSource tzSource = TzdbDateTimeZoneSource.Default;
            dailyLoginTimeMetrics = new List<LoginTimePerDay>()
            {
                new LoginTimePerDay(timezone: tzSource.ForId("US/Pacific")),
                new LoginTimePerDay(timezone: tzSource.ForId("US/Mountain")),
                new LoginTimePerDay(timezone: tzSource.ForId("US/Central")),
                new LoginTimePerDay(timezone: tzSource.ForId("US/Eastern")),
                new LoginTimePerDay(timezone: tzSource.ForId("Europe/London")),
                new LoginTimePerDay(timezone: tzSource.ForId("Europe/Berlin")),
                new LoginTimePerDay(timezone: tzSource.ForId("Europe/Athens")),
                new LoginTimePerDay(timezone: tzSource.ForId("Asia/Hong_Kong")),
                new LoginTimePerDay(timezone: tzSource.ForId("Asia/Tokyo")),
                //new LoginTimePerDay(timezone: tzSource.ForId("Etc/GMT+10")),
            };

            metrics = new List<IMetric>()
            {
                usernameMetric,
                loginCountMetric,
                loginTimeMetric,
                messageCountMetric,
                emoteCountMetric
            };
            metrics.AddRange(dailyLoginTimeMetrics);

            CancellationTokenSource cts = new CancellationTokenSource();
            InitializeTask = Task.Factory.StartNew(() =>
            {
                return this.CheckForNewFiles(cts.Token);
            })
            .Unwrap()
            .ContinueWith(task =>
            {
                foreach (var file in Directory.EnumerateFiles(LogDirectory))
                {
                    using (FileReader reader = new FileReader(file, metrics))
                    {
                        reader.ReadLines();
                    }
                }
            });
        }

        #region API

        public async Task<IEnumerable<string>> GetUsernames()
        {
            await InitializeTask;

            return usernameMetric.Usernames;
        }

        public async Task<int> GetUserLoginCount(string user)
        {
            await InitializeTask;
            var userLoginCount = loginCountMetric.LoginCount;

            if (!userLoginCount.ContainsKey(user))
            {
                return -1;
            }

            return userLoginCount[user];
        }

        public async Task<Duration> GetUserLoggedInTime(string user)
        {
            await InitializeTask;
            var userLoggedInTime = loginTimeMetric.UserTimeSpans;

            if (!userLoggedInTime.ContainsKey(user))
            {
                return Duration.Zero;
            }

            return userLoggedInTime[user];
        }

        public async Task<IDictionary<LocalDate, Duration>> GetUserLoggedInTimePerDay(string timezoneId, string user)
        {
            await InitializeTask;

            var dict = dailyLoginTimeMetrics.Find(loginTimePerDayMetric => loginTimePerDayMetric.TimeZone.Id == timezoneId)?.UserDailyLoginTimeDictionary;
            if (dict == null)
            {
                return null;
            }

            if (!dict.ContainsKey(user))
            {
                return null;
            }

            return new Dictionary<LocalDate, Duration>(dict[user]);
        }

        public async Task<int> GetTotalMessages(string user)
        {
            await InitializeTask;
            var userMessageCount = messageCountMetric.NumberMessagesDict;

            return userMessageCount.ContainsKey(user) ? userMessageCount[user] : 0;
        }

        public async Task<IReadOnlyDictionary<string, int>> GetUserEmotes(string username, int num)
        {
            await InitializeTask;
            var userEmotes = emoteCountMetric.UserEmoteDictionary;

            if (!userEmotes.ContainsKey(username))
            {
                return new Dictionary<string, int>();
            }

            return userEmotes[username].OrderByDescending(kv => kv.Value.Count).Take(num).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        }

        public async Task<IReadOnlyDictionary<string, int>> GetEffectsForEmote(string username, string emote)
        {
            await InitializeTask;
            var userEmotes = emoteCountMetric.UserEmoteDictionary;

            if (!userEmotes.ContainsKey(username) || !userEmotes[username].ContainsKey(emote))
            {
                return new Dictionary<string, int>();
            }

            return userEmotes[username][emote].Effects;
        }

        #endregion

        #region IHostedService

        private string GetFilePath(DateTime date)
        {
            string year, month, day;
            year = date.Year.ToString("D4");
            month = date.Month.ToString("D2");
            day = date.Day.ToString("D2");

            return Path.Combine(LogDirectory, $"#berrytube-{year}-{month}-{day}.log");
        }

        private string GetFileUrl(DateTime date)
        {
            string year, month, day;
            year = date.Year.ToString("D4");
            month = date.Month.ToString("D2");
            day = date.Day.ToString("D2");

            return $"https://logs.multihoofdrinking.com/raw/%23berrytube-{year}-{month}-{day}.log";
        }

        private async Task<string> DownloadFile(DateTime date, CancellationToken stoppingToken)
        {
            string downloadPath = GetFilePath(date);

            try
            {
                using (HttpClient client = new HttpClient())
                using (FileStream file = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    HttpResponseMessage response = await client.GetAsync(GetFileUrl(date), stoppingToken);
                    await response.Content.CopyToAsync(file);

                    return downloadPath;
                }
            }
            catch
            {
                return null;
            }
        }

        private bool FileExists(DateTime date)
        {
            return File.Exists(GetFilePath(date));
        }

        private static readonly DateTime BeginDate = new DateTime(2014, 1, 10);
        private DateTime LatestDate = BeginDate;

        private IEnumerable<DateTime> GetDatesFromLatestToCurrentDate(DateTime currentDate)
        {
            return Enumerable.Range(0, currentDate.Subtract(LatestDate).Days)
                             .Select(offset => LatestDate.AddDays(offset+1));
        }

        private async Task<IEnumerable<string>> CheckForNewFiles(CancellationToken stoppingToken)
        {
            var retList = new List<string>();

            var dates = GetDatesFromLatestToCurrentDate(DateTime.Now).ToList();

            foreach (var date in dates)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return Enumerable.Empty<string>();
                }

                if (FileExists(date))
                {
                    LatestDate = date;
                    continue;
                }

                string expectedPath = GetFilePath(date);
                Console.WriteLine($"Downloading File: {expectedPath}");
                string path = await DownloadFile(date, stoppingToken);

                if (path != expectedPath)
                {
                    Console.WriteLine($"ERROR: File Download Failed for {expectedPath}");
                    //Bail out so we don't have any empty holes
                    return retList;
                }
                else
                {
                    LatestDate = date;
                    retList.Add(path);
                }

                await Task.Delay(millisecondsDelay: 2000, cancellationToken: stoppingToken);
            }

            return retList;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeTask;

            Console.WriteLine("Starting StatsProvider New File Watcher");
            stoppingToken.Register(() => Console.WriteLine("Stopping StatsProvider New File Watcher"));

            while(!stoppingToken.IsCancellationRequested)
            {
                IEnumerable<string> newFilePaths = await CheckForNewFiles(stoppingToken);

                foreach (string filePath in newFilePaths)
                {
                    using (FileReader reader = new FileReader(filePath, metrics))
                    {
                        reader.ReadLines();
                    }
                }

                await Task.Delay(new TimeSpan(hours: 3, minutes: 0, seconds: 0), stoppingToken);
            }
        }

        #endregion
    }
}
