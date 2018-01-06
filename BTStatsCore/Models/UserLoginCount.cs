using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BTStatsCore.Models
{
    public class UserLoginCount
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public int Count { get; set; }
    }
}
