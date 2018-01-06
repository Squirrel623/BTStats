using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BTStatsCorePopulator
{
    public class EmoteStats
    {
        public int Count { get; set; }
        public Dictionary<string, int> Effects { get; } = new Dictionary<string, int>();
    }

    public class UserEmotes : IMetric
    {
        private Dictionary<string, Dictionary<string, EmoteStats>> _userEmoteDictionary = new Dictionary<string, Dictionary<string, EmoteStats>>();
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, EmoteStats>> UserEmoteDictionary => _userEmoteDictionary.ToDictionary(kv => kv.Key, kv => (IReadOnlyDictionary<string, EmoteStats>)new ReadOnlyDictionary<string, EmoteStats>(kv.Value));

        public UserEmotes()
        {
            foreach(var user in Users.InitialLoggedInUsers)
            {
                _userEmoteDictionary[user] = new Dictionary<string, EmoteStats>();
            }
        }

        public void HandleLogMessage(TimestampMessage message)
        {
            var userMessage = message as UserTimestampMessage;
            if (userMessage?.Type != UserTimestampMessage.MessageType.Regular)
            {
                return;
            }

            var match = LogRegex.Emote.Match(message.RawContent);
            if (!match.Success)
            {
                return;
            }

            string[] emote = match.Groups[1].Value.Split('-');
            string baseEmote = emote[0];

            if (!_userEmoteDictionary.ContainsKey(userMessage.Username))
            {
                _userEmoteDictionary[userMessage.Username] = new Dictionary<string, EmoteStats>();
            }


            var dict = _userEmoteDictionary[userMessage.Username];

            if (!dict.ContainsKey(baseEmote))
            {
                dict[baseEmote] = new EmoteStats();
            }

            dict[baseEmote].Count++;
            emote.Skip(1).ForEach(effect => dict[baseEmote].Effects.AddAndIncrement(effect));
        }
    }
}
