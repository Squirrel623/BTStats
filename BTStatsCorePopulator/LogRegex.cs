using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace BTStatsCorePopulator
{
    public class LogRegex
    {
        public static readonly Regex TimestampMessage = new Regex(@"^(\d+):(\d+):(\d+)\+(\d+)(.*)", RegexOptions.Compiled);
        public static readonly Regex LogOpened = new Regex(@"^--- Log opened (\w+) (\w+) (\d+) (\d\d:\d\d:\d\d) (\d+).*$", RegexOptions.Compiled);
        public static readonly Regex DayChanged = new Regex(@"^--- Day changed (\w+) (\w+) (\w+) (\w+).*$", RegexOptions.Compiled);
        public static readonly Regex UserMessage = new Regex(@"<[+@%](\w+)>", RegexOptions.Compiled);
        public static readonly Regex UserJoin = new Regex(@"-!- (\w+) .* has joined .*$", RegexOptions.Compiled);
        public static readonly Regex UserLeave = new Regex(@"-!- (\w+) .* has left .*$", RegexOptions.Compiled);
        public static readonly Regex Emote = new Regex(@"\[]\(/([\w-\d]+)\)", RegexOptions.Compiled);
    }
}
