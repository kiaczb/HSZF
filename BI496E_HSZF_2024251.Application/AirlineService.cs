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
        void RemoveAirline(Airline airline);
    }
    public class AirlineService : IAirlineService
    {
        private readonly IAirlineDataProvider airlineDataProvider;

        public AirlineService(IAirlineDataProvider airlineDataProvider)
        {
            this.airlineDataProvider = airlineDataProvider;
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

        public bool ReadFlightsFromJson(string path)
        {
            return airlineDataProvider.ReadFlightsFromJson(path);
        }

        public void RemoveAirline(Airline airline)
        {
            airlineDataProvider.RemoveAirline(airline);
        }
    }
}
