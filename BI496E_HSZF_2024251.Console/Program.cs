using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BI496E_HSZF_2024251.Persistence.MsSql;
using BI496E_HSZF_2024251.Application;
using BI496E_HSZF_2024251.Model;
using ConsoleTools;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics;
using System;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddScoped<FlightDbContext>();
        services.AddSingleton<IAirlineDataProvider, AirlineDataProvider>();
        services.AddSingleton<IAirlineService, AirlineService>();
    })
    .Build();
host.Start();

using IServiceScope serviceScope = host.Services.CreateScope();
var airlineService = host.Services.GetRequiredService<IAirlineService>();



var menu = new ConsoleMenu(args, 1)
    .Add("Read airlines from json", () => ReadFlights())
    .Add("Add airline", () => AddAirline())
    .Add("Modify airlines", ()=>ShowAirlinesasd())
    .Add("Exit", () => Environment.Exit(0))
    .Configure((config) =>
    {
        config.Title = "First";
        config.EnableBreadcrumb = true;
        config.WriteBreadcrumbAction = titles => Console.WriteLine("Breadcrumb: " + string.Join(" > ", titles));
    });

menu.Show();

void ModifyMenu(string airlineName)
{
    var modifyAirlineMenu = new ConsoleMenu(args, 3)
    .Add("Modify", () => ModifyAirline(airlineName))
    .Add("Add Destination", () => AddDestination(airlineName))
    .Add("Remove Airline", () => RemoveAirline(airlineName))
    .Add("Remove Destination or Discount", () => RemoveDestinationOrDiscount(airlineName))
    .Add("Back", ConsoleMenu.Close)
    .Configure((config) =>
    {
        config.Title = "Third";
        config.EnableBreadcrumb = true;
        config.WriteBreadcrumbAction = titles => Console.WriteLine("Breadcrumb: " + string.Join(" > ", titles));
    });

    modifyAirlineMenu.Show();
}
void ShowAirlinesasd()
{
    var airlines = new ConsoleMenu(args, 2);
    foreach (var item in airlineService.GetAllDistinctAirlineNames())
    {
        airlines
            .Add($"{item}", () =>
            {
                ModifyMenu(item);
                airlines.CloseMenu();
                ShowAirlinesasd();
            });
            
    }
    airlines.Add("Back", ConsoleMenu.Close);
    airlines.Configure((config) =>
    {
        config.Title = "Second";
        config.EnableBreadcrumb = true;
        config.WriteBreadcrumbAction = titles => Console.WriteLine("Breadcrumb: " + string.Join(" > ", titles));
    });
    airlines.Show();
}
void EmptyAction() { }
void ModifyAirline(string airlineName)
{
    var airline = airlineService.GetAirlineByName(airlineName);

    var asd = new ConsoleMenu(args, 3);
    foreach (var item in airline)
    {
        asd.Add($"{item.name} - {item.departure_from}", EmptyAction);
        asd.Configure(config =>
        {
            config.WriteItemAction = menuItem =>
            {
                Console.Write($"{menuItem.Name}");

            };
            config.Selector = "";
        });
        foreach (var dest in item.Destinations)
        {
            foreach (PropertyInfo prop in dest.GetType().GetProperties())
            {
                string start = "\t  ";
                var displayName = (DisplayNameAttribute)Attribute.GetCustomAttribute(prop, typeof(DisplayNameAttribute));
                if (Attribute.IsDefined(prop, typeof(DisplayPropertyAttribute)) && prop.GetValue(dest) != null && displayName.DisplayName != null)
                {
                    string propValue = prop.GetValue(dest).ToString();
                    if (prop.Name == "city")
                    {
                        start = "\t";
                    }
                    if (prop.Name == "discount")
                    {
                        propValue = "";
                    }
                    asd.Add($"{start}-{displayName.DisplayName}: {propValue}", (a) =>
                    {
                        if (TryParseValue(prop.PropertyType,Console.ReadLine(), out var result) && prop.Name != "discount")
                        {
                            prop?.SetValue(dest, result);
                            propValue = prop.GetValue(dest).ToString();
                        }
                        a.CurrentItem.Name = $"{start}-{displayName.DisplayName}: {propValue}";
                    });
                    if (prop.Name == "discount" && dest.discount != null)
                    {
                        start = "\t\t";
                        foreach (PropertyInfo disProp in dest.discount.GetType().GetProperties())
                        {
                            var displayDiscountName = (DisplayNameAttribute)Attribute.GetCustomAttribute(disProp, typeof(DisplayNameAttribute));
                            if (Attribute.IsDefined(disProp, typeof(DisplayPropertyAttribute)) && displayDiscountName != null)
                            {
                                asd.Add($"{start}-{displayDiscountName.DisplayName}: {disProp.GetValue(dest.discount)}", (a) =>
                                {
                                    if (TryParseValue(disProp.PropertyType, Console.ReadLine(), out var result))
                                    {
                                        disProp?.SetValue(dest.discount, result);

                                    }
                                    a.CurrentItem.Name = $"{start}-{displayDiscountName.DisplayName}: {disProp.GetValue(dest.discount)}";
                                });
                            }
                            
                        }
                    }
                    
                }
                
            }

        }
    }
    asd.Add("Exit", ConsoleMenu.Close);
    asd.Show();
}
static bool TryParseValue(Type targetType, string input, out object result)
{
    result = null;

    if (input == null || input =="")
    {
        throw new ArgumentNullException(nameof(input));
    }

    if (targetType == typeof(string))
    {
        result = input; 
        return true;
    }

    var tryParseMethod = targetType.GetMethod("TryParse", new[] { typeof(string), targetType.MakeByRefType() });

    if (tryParseMethod != null)
    {

        var parameters = new object[] { input, null };
        bool success = (bool)tryParseMethod.Invoke(null, parameters);
        result = parameters[1];
        return success;
    }

    return false;
}
void RemoveAirline(string airlineName)
{
    var airline = airlineService.GetAirlineByName(airlineName);

    var asd = new ConsoleMenu(args, 3);
    foreach (var item in airline)
    {
        string destinations = "";
        foreach (var dest in item.Destinations)
        {
            foreach (PropertyInfo prop in dest.GetType().GetProperties())
            {
                string start = "\t  ";
                var displayName = (DisplayNameAttribute)Attribute.GetCustomAttribute(prop, typeof(DisplayNameAttribute));
                if (Attribute.IsDefined(prop, typeof(DisplayPropertyAttribute)) && prop.GetValue(dest) != null && displayName.DisplayName != null)
                {
                    string propValue = prop.GetValue(dest).ToString();
                    if (prop.Name == "city")
                    {
                        start = "\t";
                    }
                    if (prop.Name == "discount")
                    {
                        propValue = "";
                    }
                    destinations += $"{start}-{displayName.DisplayName}: {propValue}\n";
                    if (prop.Name == "discount" && dest.discount != null)
                    {
                        start = "\t\t";
                        foreach (PropertyInfo disProp in dest.discount.GetType().GetProperties())
                        {
                            var displayDiscountName = (DisplayNameAttribute)Attribute.GetCustomAttribute(disProp, typeof(DisplayNameAttribute));
                            if (Attribute.IsDefined(disProp, typeof(DisplayPropertyAttribute)) && displayDiscountName != null)
                            {
                                destinations += $"{start}-{displayDiscountName.DisplayName}: {disProp.GetValue(dest.discount)}\n";
                            }

                        }
                    }

                }

            }

        }
        asd.Add($"{item.name} - {item.departure_from}\n"+destinations,() =>
        {
            airlineService.RemoveAirline(item);
            asd.CloseMenu();
            RemoveAirline(item.name);
        });
        asd.Configure(config =>
        {
            config.WriteItemAction = menuItem =>
            {
                Console.Write($"{menuItem.Name}");

            };
            config.Selector = "";
        });
        
    }
    asd.Add("Exit", ConsoleMenu.Close);
    asd.Show();
}
void RemoveDestinationOrDiscount(string airlineName)
{
    var airline = airlineService.GetAirlineByName(airlineName);

    var asd = new ConsoleMenu(args, 3);
    foreach (var item in airline)
    {
        asd.Add($"{item.name} - {item.departure_from}", EmptyAction);
        asd.Configure(config =>
        {
            config.WriteItemAction = menuItem =>
            {
                Console.Write($"{menuItem.Name}");

            };
            config.Selector = "";
        });

        foreach (var dest in item.Destinations)
        {
            string destinations = "";
            string discount = "";
            foreach (PropertyInfo prop in dest.GetType().GetProperties())
            {
                string start = "\t  ";
                var displayName = (DisplayNameAttribute)Attribute.GetCustomAttribute(prop, typeof(DisplayNameAttribute));
                if (Attribute.IsDefined(prop, typeof(DisplayPropertyAttribute)) && prop.GetValue(dest) != null && displayName.DisplayName != null)
                {
                    string propValue = prop.GetValue(dest).ToString();
                    if (prop.Name == "city")
                    {
                        start = "\t";
                    }
                    if (prop.Name == "discount")
                    {
                        propValue = "";
                    }
                    destinations += $"{start}-{displayName.DisplayName}: {propValue}\n";
                    if (prop.Name == "discount" && dest.discount != null)
                    {
                        destinations += $"{start}-{displayName.DisplayName}: {propValue}";
                        start = "\t\t";
                        foreach (PropertyInfo disProp in dest.discount.GetType().GetProperties())
                        {
                            var displayDiscountName = (DisplayNameAttribute)Attribute.GetCustomAttribute(disProp, typeof(DisplayNameAttribute));
                            if (Attribute.IsDefined(disProp, typeof(DisplayPropertyAttribute)) && displayDiscountName != null)
                            {
                                discount += $"{start}-{displayDiscountName.DisplayName}: {disProp.GetValue(dest.discount)}\n";
                            }

                        }
                    }

                }

            }
            asd.Add(destinations, () =>
            {
                airlineService.RemoveDestination(item, dest);
                asd.CloseMenu();
                RemoveDestinationOrDiscount(item.name);
            });
            if (discount != "")
            {
                asd.Add(discount, ()=>
                {
                    airlineService.RemoveDiscount(item, dest);
                    asd.CloseMenu();
                    RemoveDestinationOrDiscount(item.name);
                });

            }

        }
    }
    asd.Add("Exit", ConsoleMenu.Close);
    asd.Show();
}
void AddAirline()
{
    Airline airline = new Airline();
    foreach (PropertyInfo prop in airline.GetType().GetProperties())
    {
        
        var displayName = (DisplayNameAttribute)Attribute.GetCustomAttribute(prop, typeof(DisplayNameAttribute));
        if (displayName!= null)
        {
            Console.WriteLine($"Enter {displayName.DisplayName}:");
            if (displayName.DisplayName == "Destinations")
            {
                AddDestinationForm(ref airline);

            }
        }
        
        if (prop.Name != "Destinations" && Attribute.IsDefined(prop, typeof(DisplayPropertyAttribute)) && TryParseValue(prop.PropertyType, Console.ReadLine(), out var result))
        {

            prop?.SetValue(airline, result);
        }
    }
    if (!airlineService.hasSameAirline(airline))
    {
        airlineService.AddAirline(airline);
    }
    else
    {
        airlineService.AddRangeDestination(airline, airline.Destinations);
    }
}
void AddDestination(string airlineName)
{
    var airline = airlineService.GetAirlineByName(airlineName);

    var asd = new ConsoleMenu(args, 3);
    foreach (var item in airline)
    {
        string destinations = "";
        foreach (var dest in item.Destinations)
        {
            foreach (PropertyInfo prop in dest.GetType().GetProperties())
            {
                string start = "\t  ";
                var displayName = (DisplayNameAttribute)Attribute.GetCustomAttribute(prop, typeof(DisplayNameAttribute));
                if (Attribute.IsDefined(prop, typeof(DisplayPropertyAttribute)) && prop.GetValue(dest) != null && displayName.DisplayName != null)
                {
                    string propValue = prop.GetValue(dest).ToString();
                    if (prop.Name == "city")
                    {
                        start = "\t";
                    }
                    if (prop.Name == "discount")
                    {
                        propValue = "";
                    }
                    destinations += $"{start}-{displayName.DisplayName}: {propValue}\n";
                    if (prop.Name == "discount" && dest.discount != null)
                    {
                        start = "\t\t";
                        foreach (PropertyInfo disProp in dest.discount.GetType().GetProperties())
                        {
                            var displayDiscountName = (DisplayNameAttribute)Attribute.GetCustomAttribute(disProp, typeof(DisplayNameAttribute));
                            if (Attribute.IsDefined(disProp, typeof(DisplayPropertyAttribute)) && displayDiscountName != null)
                            {
                                destinations += $"{start}-{displayDiscountName.DisplayName}: {disProp.GetValue(dest.discount)}\n";
                            }

                        }
                    }

                }

            }

        }
        asd.Add($"{item.name} - {item.departure_from}\n" + destinations, () =>
        {
            var myAirline = item;
            AddDestinationForm(ref myAirline);
            asd.CloseMenu();
            AddDestination(item.name);
        });
        asd.Configure(config =>
        {
            config.WriteItemAction = menuItem =>
            {
                Console.Write($"{menuItem.Name}");

            };
            config.Selector = "";
        });

    }
    asd.Add("Exit", ConsoleMenu.Close);
    asd.Show();
}
void AddDestinationForm(ref Airline airline)
{
        ConsoleKeyInfo keyInfo;
    do
    {
        Console.WriteLine();
        Destination destination = new Destination();
        foreach (var dest in destination.GetType().GetProperties())
        {
            var displayDestName = (DisplayNameAttribute)Attribute.GetCustomAttribute(dest, typeof(DisplayNameAttribute));
            if (displayDestName != null)
            {
                Console.Write($"{displayDestName.DisplayName}: ");

            }
            if (displayDestName != null && displayDestName.DisplayName == "Discount")
            {
                do
                {
                    Console.WriteLine("Would you like to add discount? [y/n]");
                    keyInfo = Console.ReadKey();
                } while (keyInfo.Key != ConsoleKey.N && keyInfo.Key != ConsoleKey.Y);
                if (keyInfo.Key == ConsoleKey.Y)
                {
                    Discount discount = new Discount();
                    Console.WriteLine();
                    foreach (var disc in discount.GetType().GetProperties())
                    {
                        var displayDiscName = (DisplayNameAttribute)Attribute.GetCustomAttribute(disc, typeof(DisplayNameAttribute));
                        if (displayDiscName != null)
                        {
                            Console.Write($"{displayDiscName.DisplayName}: ");
                            if (Attribute.IsDefined(disc, typeof(DisplayPropertyAttribute)) && TryParseValue(disc.PropertyType, Console.ReadLine(), out var discResult))
                            {

                                disc?.SetValue(discount, discResult);
                            }
                        }
                    }
                    destination.discount = discount;
                }

            }
            if (dest.Name != "discount" && Attribute.IsDefined(dest, typeof(DisplayPropertyAttribute)) && TryParseValue(dest.PropertyType, Console.ReadLine(), out var destResult))
            {

                dest?.SetValue(destination, destResult);
            }
        }
        airline.Destinations.Add(destination);
        do
        {
            Console.WriteLine("\nWould you like to add more destinations? [y/n]");
            keyInfo = Console.ReadKey();
        } while (keyInfo.Key != ConsoleKey.N && keyInfo.Key != ConsoleKey.Y);
    } while (keyInfo.Key == ConsoleKey.Y);
}
void ReadFlights()
{
    //airlineService.ReadFlightsFromJson("airlines.json");
    Console.WriteLine("Please enter the json file destination:");
    string path = Console.ReadLine();
    
    if (airlineService.ReadFlightsFromJson(path))
    {
        Console.Clear();
        Console.WriteLine("Json imported sucessfully!");
    }
    else
    {
        Console.Clear();
        Console.WriteLine("An error occured durnig reading the file.");
    }
    
    Thread.Sleep(2000);
}
