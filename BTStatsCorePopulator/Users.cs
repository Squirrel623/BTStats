using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace BTStatsCorePopulator
{
    public class Users
    {
        public static readonly ImmutableHashSet<string> InitialLoggedInUsers = new HashSet<string>
        {
            "rovendoug","Jasonbluefire","Kris321","Yuukon","clay_two","everclear","wut","draders",
            "MelloMaster","SolarFlare","ThoriumHooves","Blueshift","Redtoxin","Toastdeib","Chrono",
            "Haiku_knives","omnomtom","inks","Trellmor_PC","fuzzykittenears","electrashock45",
            "FlutterNickname","majorfresh","yaxim3","thewalkindude","Gonzales","MogwaiToboggan","Lavender",
            "fenrir","Q0Phone","miggyb","WeedWuff","unKaged","Kurtis","SugarGrape","ChocoScoots",
            "tiwake","Rainbowjack","Thunderstrike","DigitalVagrant","frenzyfivefour","ZetaRho",
            "PonisEnvy","Q0IRC","Jerick","Colgate","Marminator","Cades","Trahsi","Therii", "Twisted_Muffins"
        }.ToImmutableHashSet();
    }
}
