using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BTStatsCorePopulator
{
    public class DateConvertUtil
    {
        public static int MonthStringToInt(string month)
        {
            switch(month) {
                case "Jan": return 1;
                case "Feb": return 2;
                case "Mar": return 3;
                case "Apr": return 4;
                case "May": return 5;
                case "Jun": return 6;
                case "Jul": return 7;
                case "Aug": return 8;
                case "Sep": return 9;
                case "Oct": return 10;
                case "Nov": return 11;
                case "Dec": return 12;
                default: throw new Exception("Unexpected month string");
            }
        }
    }

    public class FileReader : IDisposable
    {
        private StreamReader fileStreamReader;
        //string[] fileLines;
        private DateTimeOffset baseDate;
        private IEnumerable<IMetric> metrics;

        public FileReader(string filePath, IEnumerable<IMetric> metrics)
        {
            //if (fileLines == null || fileLines.Length < 1)
            //{
            //    throw new Exception();
            //}
            //this.fileLines = fileLines;

            fileStreamReader = new StreamReader(filePath);
            this.metrics = metrics;

            string firstLine = fileStreamReader.ReadLine();
            //string firstLine = fileLines[0];
            if (!LogRegex.LogOpened.IsMatch(firstLine))
            {
                throw new Exception("Unexpected first line");
            }

            var match = LogRegex.LogOpened.Match(firstLine);

            if (!match.Success || match.Groups.Count < 6)
            {
                throw new Exception("Matches don't match");
            }

            int day, month, year;
            if (!int.TryParse(match.Groups[3].Value, out day) ||
                !int.TryParse(match.Groups[5].Value, out year))
            {
                throw new Exception("Bad day or year");
            }

            month = DateConvertUtil.MonthStringToInt(match.Groups[2].Value);


            baseDate = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
        }

        public void ReadLines()
        {
            //for(int i = 1; i < this.fileLines.Length; i++)
            //{
            //    string line = this.fileLines[i];
            //    TimestampMessage message = TimestampMessage.Create(baseDate, line);
            //    if (message == null)
            //    {
            //        continue;
            //    }

            //    metric.HandleLogMessage(message);
            //}
            string line;
            while ((line = fileStreamReader.ReadLine()) != null)
            {
                TimestampMessage message = TimestampMessage.Create(baseDate, line);
                if (message == null)
                {
                    continue;
                }

                foreach (var metric in metrics)
                {
                    metric.HandleLogMessage(message);
                }
            }
        }

        public void Dispose()
        {
            fileStreamReader.Dispose();
        }
    }
}
