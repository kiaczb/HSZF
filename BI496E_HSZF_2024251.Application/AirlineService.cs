using BI496E_HSZF_2024251.Model;
using BI496E_HSZF_2024251.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI496E_HSZF_2024251.Application
{
    public interface IAirlineService
    {
        Airline GetAirlineById(int id);
        List<Airline> GetAllAirlines();
        List<string> GetAllDistinctAirlineNames();
        List<Airline> GetAirlineByName(string name);
        bool ReadFlightsFromJson(string path);
        bool hasSameAirline(Airline airline);
        void RemoveAirline(Airline airline);
        void AddAirline(Airline airline);
        void AddDestination(Destination destination);
        void RemoveDestination(Airline airline, Destination destination);
        void RemoveDiscount(Airline airline, Destination destination);

        void AddRangeDestination(Airline airline, ICollection<Destination> destinations);

    }
    public class AirlineService : IAirlineService
    {
        private readonly IAirlineDataProvider airlineDataProvider;

        public AirlineService(IAirlineDataProvider airlineDataProvider)
        {
            this.airlineDataProvider = airlineDataProvider;
        }

        public void AddAirline(Airline airline)
        {
            airlineDataProvider.AddAirline(airline);
        }

        public void AddDestination(Destination destination)
        {
            airlineDataProvider.AddDestination(destination);
        }

        public void AddRangeDestination(Airline airline, ICollection<Destination> destinations)
        {
            airlineDataProvider.AddRangeDestination(airline, destinations);
        }

        public Airline GetAirlineById(int id)
        {
            return airlineDataProvider.GetAirlineById(id);
        }

        public List<Airline> GetAirlineByName(string name)
        {
            return airlineDataProvider.GetAirlineByName(name);
        }
        public List<Airline> GetAllAirlines()
        {
            return airlineDataProvider.GetAllAirlines();
        }

        public List<string> GetAllDistinctAirlineNames()
        {
            return airlineDataProvider.GetAllDistinctAirlineNames();
        }

        public bool hasSameAirline(Airline airline)
        {
            return airlineDataProvider.hasSameAirline(airline);
        }

        public bool ReadFlightsFromJson(string path)
        {
            return airlineDataProvider.ReadFlightsFromJson(path);
        }

        public void RemoveAirline(Airline airline)
        {
            airlineDataProvider.RemoveAirline(airline);
        }

        public void RemoveDestination(Airline airline, Destination destination)
        {
            airlineDataProvider.RemoveDestination(airline, destination);
        }

        public void RemoveDiscount(Airline airline, Destination destination)
        {
            airlineDataProvider.RemoveDiscount(airline, destination);
        }
    }
}
