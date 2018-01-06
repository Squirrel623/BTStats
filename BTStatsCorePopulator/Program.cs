using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace BTStatsCorePopulator
{
    class Program
    {
        static string LogDirectory = @"C:\Users\Brady\Documents\BTLogs2";

        static void Main(string[] args)
        {
            var files = (Directory.EnumerateFiles(LogDirectory) as IEnumerable<string>);//.Take(20);

            LoginCounter metric1 = new LoginCounter();
            //LoginTimeAccumulator metric2 = new LoginTimeAccumulator(offset: TimeSpan.Zero);
            LoginTimePerDay metric3 = new LoginTimePerDay(offset: new TimeSpan(-4, 0, 0));

            //var metrics = new IMetric[] { metric1, metric2, metric3 };

            foreach (var file in files)
            {
                //using (FileReader reader = new FileReader(file, metrics))
                //{
                //    reader.ReadLines();
                //}
            }

           // var ordered = metric2.UserTimeSpans.OrderByDescending(kvp => kvp.Value.TotalMilliseconds).ToArray();

            //TimeSpan offset = new TimeSpan(0, 0, 0);
            //LoginTimePerDay metric = new LoginTimePerDay(offset);

            //string loginString = "01:30:00+0200 -!- Squirrel623 [Squirrel623@berrytube.tv] has joined #berrytube";
            //string logoutString = "01:00:00+0100 -!- Squirrel623 [Squirrel623@berrytube.tv] has left #berrytube";

            //DateTimeOffset baseDt = new DateTimeOffset(2017, 11, 5, 0, 0, 0, TimeSpan.Zero);

            //var firstMessage = TimestampMessage.Create(baseDt, loginString);
            //var lastMessage = TimestampMessage.Create(baseDt, logoutString);

            //metric.HandleLogMessage(firstMessage);
            //metric.HandleLogMessage(lastMessage);

            Console.ReadKey();
        }
    }
}
