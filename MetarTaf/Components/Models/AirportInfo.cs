using System;
using System.Collections.Generic;

namespace MetarTaf.Components.Models
{
    public class AirportInfo
    {
        public string city { get; set; }
        public string country { get; set; }
        public int elevation_ft { get; set; }
        public int elevation_m { get; set; }
        public string gps { get; set; }
        public string iata { get; set; }
        public string icao { get; set; }
        public double latitude { get; set; }
        public string local { get; set; }
        public double longitude { get; set; }
        public string name { get; set; }
        public string note { get; set; }
        public bool reporting { get; set; }
        public List<Runway> runways { get; set; }
        public string state { get; set; }
        public string type { get; set; }
        public string website { get; set; }
        public string wiki { get; set; }
    }

    public class Runway
    {
        public int length_ft { get; set; }
        public int width_ft { get; set; }
        public string ident1 { get; set; }
        public string ident2 { get; set; }
    }
}
