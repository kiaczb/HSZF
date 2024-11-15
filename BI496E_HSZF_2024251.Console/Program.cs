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


var airlines = new ConsoleMenu(args, 2);
var modifyFlightsMenu = new ConsoleMenu(args, 1)
    .Add("Add airline", () => AddAirline())
    .Add("Remove airline", () => ShowAirlines(RemoveAirline))
    .Add("Modify airline", (airline) => ShowAirlines(ModifyAirline))
    .Add("Back", ConsoleMenu.Close);


var menu = new ConsoleMenu(args, 0)
    .Add("Read flights", () => ReadFlights())
        .Add("Modify flights", modifyFlightsMenu.Show)
        .Add("Four", () => Environment.Exit(0));



menu.Show();


void ShowAirlines(Action<string> action)
{
    var airlines = new ConsoleMenu(args, 2);
    foreach (var item in airlineService.GetAllDistinctAirlineNames())
    {
        airlines.Add($"{item}",()=>
        {
            action(item);
            airlines.CloseMenu();
            ShowAirlines(action);
        });
    }
    airlines.Add("Exit", ConsoleMenu.Close);
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
        return false;
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

void AddAirline()
{
    Airline airline = new Airline();
    foreach (PropertyInfo prop in airline.GetType().GetProperties())
    {
        var displayName = (DisplayNameAttribute)Attribute.GetCustomAttribute(prop, typeof(DisplayNameAttribute));
        Console.WriteLine($"Enter {displayName.DisplayName}:");
        if (Attribute.IsDefined(prop, typeof(DisplayPropertyAttribute)) && TryParseValue(prop.PropertyType, Console.ReadLine(), out var result) && prop.Name != "discount")
        {
            prop?.SetValue(prop, result);
        }
    }
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
        airlines = null;
    }
    else
    {
        Console.Clear();
        Console.WriteLine("An error occured durnig reading the file.");
    }
    
    Thread.Sleep(2000);
}
