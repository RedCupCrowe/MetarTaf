using MetarTaf.Components.Models;
using MetarTaf.Components.Services;
using Microsoft.AspNetCore.Components;

namespace MetarTaf.Components.Factories
{
    public class AirportFactory
    {
        private readonly MetarService metarService;
        private readonly TAFService tafService;
        private readonly AirportInfoService airportInfoService;

        private readonly Dictionary<string, Airport> airports = new Dictionary<string, Airport>();
        private readonly object lockObject = new object();

        public AirportFactory(MetarService metarService, TAFService tafService, AirportInfoService airportInfoService)
        {
            this.metarService = metarService;
            this.tafService = tafService;
            this.airportInfoService = airportInfoService;
        }

        public Airport GetAirport(string icao)
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

                Console.WriteLine("Factory stored airports: " + airports.Keys.ToString());

                return airports[icao];
            }
        }

        public void ReleaseAirport(string icao)
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
                    }
                }

                Console.WriteLine("Factory stored airports: " + airports.Keys.ToString());
            }
        }
    }
}
