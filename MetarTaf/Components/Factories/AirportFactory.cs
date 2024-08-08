using MetarTaf.Components.Models;
using MetarTaf.Components.Services;
using Microsoft.AspNetCore.Components;

namespace MetarTaf.Components.Factories
{
    public class AirportFactory
    {
        private static readonly Dictionary<string, Airport> airports = new Dictionary<string, Airport>();
        [Inject] private static MetarService metarService { get; set; }
        [Inject] private static TAFService tafService { get; set; }
        [Inject] private static AirportInfoService airportInfoService { get; set; }


      
        // Lock object for thread safety
        private static readonly object lockObject = new object();

        // Method to get or create an Airport instance
        public static Airport GetAirport(string icao)
        {
            lock (lockObject)
            {
                if (!airports.ContainsKey(icao))
                {
                    var airport = new Airport(icao, metarService, tafService, airportInfoService);
                    airport.IncrementReferenceCount();
                    airports[icao] = airport;
                }
                else
                {
                    airports[icao].IncrementReferenceCount();
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
                        // Optionally, you can call Dispose or clean up resources here
                        airports[icao].Dispose();
                        airports.Remove(icao);
                    }
                }
            }
        }

    }
}
