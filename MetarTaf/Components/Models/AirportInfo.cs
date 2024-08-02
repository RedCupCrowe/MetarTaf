using System;
using System.Collections.Generic;

namespace MetarTaf.Components.Models
{
    public class AirportInfo
    {
        public string City { get; set; }
        public string Country { get; set; }
        public int ElevationFt { get; set; }
        public int ElevationM { get; set; }
        public string Gps { get; set; }
        public string Iata { get; set; }
        public string Icao { get; set; }
        public double Latitude { get; set; }
        public string Local { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool Reporting { get; set; }
        public List<Runway> Runways { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public string Website { get; set; }
        public string Wiki { get; set; }
    }

    public class Runway
    {
        public int LengthFt { get; set; }
        public int WidthFt { get; set; }
        public string Ident1 { get; set; }
        public string Ident2 { get; set; }
    }
}
