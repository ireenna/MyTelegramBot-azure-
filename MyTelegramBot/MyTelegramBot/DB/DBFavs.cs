using System;
using System.Collections.Generic;
using System.Text;
using MyTelegramBot.GetTrack;

namespace MyTelegramBot
{
    public class DBFavs
    {
        public string id { get; set; }
        public List<SearchTrackMain> jsresults = new List<SearchTrackMain>();
    }
}
