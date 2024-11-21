using BI496E_HSZF_2024251.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI496E_HSZF_2024251.Persistence.MsSql
{
    public class FlightDbContext : DbContext
    {
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Discount> Discounts { get; set; }

        public FlightDbContext()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=FlightsDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Destination>()
            .HasOne(d => d.Airline)          // A Destination has one Airline
            .WithMany(a => a.Destinations)  // An Airline has many Destinations
            .HasForeignKey(d => d.AirlineId) // The foreign key for the relationship
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
