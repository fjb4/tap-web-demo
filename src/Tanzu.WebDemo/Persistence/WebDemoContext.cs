using Microsoft.EntityFrameworkCore;
using Tanzu.WebDemo.Modules.AirportModule.Api;
using Tanzu.WebDemo.Modules.WeatherModule.Api;

namespace Tanzu.WebDemo.Persistence
{
    public class WebDemoContext : DbContext
    {
        protected WebDemoContext()
        {
        }

        public WebDemoContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();
        public DbSet<Airport> Airports => Set<Airport>();
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Airport>().HasData(new Airport {Id = "YYZ", Name = "Toronto"});
            modelBuilder.Entity<Airport>().HasData(new Airport {Id = "JFK", Name = "New York"});
        }
    }
}