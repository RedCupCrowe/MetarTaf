using System;
using System.Collections.Generic;
using MetarTaf.Components.Models;
using MetarTaf.Components.Services;

namespace MetarTaf.Components.Factories
{
    public static class AirportFactory
    {
        private static readonly object lockObject = new object();
        public static readonly Dictionary<string, Airport> airports = new Dictionary<string, Airport>();
        private static MetarService metarService;
        private static TAFService tafService;
        private static AirportInfoService airportInfoService;

        public static void Initialize(MetarService metarSvc, TAFService tafSvc, AirportInfoService airportInfoSvc)
        {
            metarService = metarSvc;
            tafService = tafSvc;
            airportInfoService = airportInfoSvc;
        }

        public static Airport GetAirport(string icao)
        {
            lock (lockObject)
            {
                if (!airports.ContainsKey(icao))
                {
                    var airport = new Airport(icao, metarService, tafService, airportInfoService);
                    airport.IncrementReferenceCount();
                    airports[icao] = airport;
                    Console.WriteLine("[AirportFactory] Created new airport: " + icao);
                }
                else
                {
                    airports[icao].IncrementReferenceCount();
                    Console.WriteLine("[AirportFactory] Reused existing airport: " + icao);
                }

                

                return airports[icao];
            }
        }

        public static void ReleaseAirport(string icao)
        {
            lock (lockObject)
            {
                if (airports.ContainsKey(icao))
                {
                    airports[icao].DecrementReferenceCount();

                    if (!airports[icao].IsInUse())
                    {
                        airports[icao].Dispose();
                        airports.Remove(icao);
                        Console.WriteLine("[AirportFactory] Removed airport: " + icao);
                    }
                }

                
            }
        }
    }
}
