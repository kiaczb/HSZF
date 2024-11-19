using BI496E_HSZF_2024251.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
namespace BI496E_HSZF_2024251.Persistence.MsSql
{
    public interface IAirlineDataProvider
    {
        Airline GetAirlineById(int id);
        List<Airline> GetAirlineByName(string name);
        List<Airline> GetAllAirlines();
        bool hasSameAirline(Airline airline);
        bool ReadFlightsFromJson(string path);
        List<string> GetAllDistinctAirlineNames();
        void RemoveAirline(Airline airline);
        void AddAirline(Airline airline);
        void AddDestination(Destination destination);
        void RemoveDestination(Airline airline, Destination destination);
        void RemoveDiscount(Airline airline, Destination destination);
        void AddRangeDestination(Airline airline, ICollection<Destination> destinations);
    }
    public class AirlineDataProvider : IAirlineDataProvider
    {
        private readonly FlightDbContext FlightDbContext;
        public AirlineDataProvider(FlightDbContext flightDbContext) 
        {
            FlightDbContext = flightDbContext;
        }


        public Airline GetAirlineById(int id)
        {
            return FlightDbContext.Airlines.First(x => x.ID == id);
        }
        public void RemoveAirline(Airline airline)
        {
            FlightDbContext.Airlines.Remove(airline);
            FlightDbContext.SaveChanges();
        }
        public List<Airline> GetAllAirlines()
        {
            return FlightDbContext.Airlines.ToList();
        }
        public List<string> GetAllDistinctAirlineNames()
        {
            return FlightDbContext.Airlines.Select(x => x.name).Distinct().ToList();

        }
        public bool hasSameAirline(Airline airline)
        {
            foreach (var item in GetAllAirlines())
            {
                if (item.departure_from is not null && item.departure_from.Equals(airline.departure_from) && item.name.Equals(airline.name))
                {
                    return true;
                }
            }
            return false;
        }
        public Airline GetFirstPredAirline(Predicate<Airline> pred) { return FlightDbContext.Airlines.ToList().First(x => pred(x)); }
        private bool hasDest(Destination dest)
        {
            return FlightDbContext.Destinations.ToList().Where((x) => x.ToString().Equals(dest.ToString())).ToList().Count > 0;
        }
        public bool ReadFlightsFromJson(string path)
        {
            string json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<Flights>(json);
            foreach (var airlineData in data.Airlines)
            {
                if (hasSameAirline(airlineData))
                {
                    foreach (var destinationData in airlineData.Destinations)
                    {
                       
                        var destination = new Destination()
                        {
                            city = destinationData.city,
                            price = destinationData.price,
                            distance = destinationData.distance,
                            departure_date = DateTime.Parse(destinationData.departure_date.ToString()),
                            discount = destinationData.discount
                        };
                        var myAirline = GetFirstPredAirline(x => hasSameAirline(x));

                        if (hasDest(destination))
                        {
                            continue;
                        }
                        destination.Airline = myAirline;
                        myAirline.Destinations.Add(destination);
                        FlightDbContext.Destinations.Add(destination);
                        FlightDbContext.SaveChanges();

                    }

                }
                else
                {
                    
                    var airline = new Airline()
                    {
                        name = airlineData.name,
                        departure_from = airlineData.departure_from,
                        Destinations = new List<Destination>()
                    };
                    foreach (var destinationData in airlineData.Destinations)
                    {

                        var destination = new Destination()
                        {
                            city = destinationData.city,
                            price = destinationData.price,
                            distance = destinationData.distance,
                            departure_date = DateTime.Parse(destinationData.departure_date.ToString()),
                            discount = destinationData.discount
                        };
                        airline.Destinations.Add(destination);
                    }
                    FlightDbContext.Airlines.Add(airline);
                }
                
                FlightDbContext.SaveChanges();
            }

            FlightDbContext.SaveChanges();
            try
            {
                
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public List<Airline> GetAirlineByName(string name)
        {
            return FlightDbContext.Airlines.Where(x => x.name == name).ToList();

        }
        public void AddAirline(Airline airline)
        {
            FlightDbContext.Add(airline);
            FlightDbContext.SaveChanges();
        }

        public void AddDestination(Destination destination)
        {
            FlightDbContext.Destinations.Add(destination);
            FlightDbContext.SaveChanges();
        }

        public void AddRangeDestination(Airline airline, ICollection<Destination> destinations)
        {
            var dbAirlines = GetFirstPredAirline(x => x.name == airline.name && x.departure_from == airline.departure_from);
            foreach (var item in destinations)
            {
                dbAirlines.Destinations.Add(item);
                FlightDbContext.SaveChanges();
            }
            FlightDbContext.SaveChanges();
        }

        public void RemoveDestination(Airline airline, Destination destination)
        {
            var dbAirlines = GetFirstPredAirline(x => x.name == airline.name && x.departure_from == airline.departure_from);
            dbAirlines.Destinations.Remove(destination);
            FlightDbContext.SaveChanges();
        }

        public void RemoveDiscount(Airline airline, Destination destination)
        {
            var dbAirlines = GetFirstPredAirline(x => x.name == airline.name && x.departure_from == airline.departure_from);
            foreach (var item in dbAirlines.Destinations)
            {
                if (item.Equals(destination))
                {
                    item.discount = null;
                }
            }
        }
    }
}
