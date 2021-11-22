using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tanzu.Common.Messaging;
using Tanzu.Common.Modules;
using Tanzu.WebDemo.Modules.AirportModule.Api;
using Tanzu.WebDemo.Modules.WeatherModule;
using Tanzu.WebDemo.Modules.WeatherModule.Api;
using Tanzu.WebDemo.Persistence;
using NSubstitute;
using Xunit;

namespace WebDemoTests
{
    public class WeatherTests : IUseDbContext<WebDemoContext>
    {
        public WebDemoContext CreateContext() => ((IUseDbContext<WebDemoContext>) this).GetDbContext();
        [Fact]
        public void CreateForecast_InvalidAirport_ThrowsDomainException()
        {
            var messageBus = Substitute.For<IMessageBus>();
            messageBus
                .Send(Arg.Any<AirportQuery>(), Arg.Any<CancellationToken>())
                .Returns(new[] {new Airport() {Id = "Test", Name = "Test Airport"}}.ToAsyncEnumerable());

            var sut = new WeatherService(CreateContext(), messageBus, NullLogger<WeatherService>.Instance);

            var forecast = new WeatherForecast()
            {
                AirportId = "Invalid",
                Summary = "Warm"
            };
            
            sut.Invoking(x => x.SaveForecast(forecast)).Should().ThrowAsync<DomainException>();
        }
        
        [Fact]
        public async Task CreateForecast_Valid_SavesToDatabase()
        {
            var messageBus = Substitute.For<IMessageBus>();
            messageBus
                .Send(Arg.Any<AirportQuery>(), Arg.Any<CancellationToken>())
                .Returns(new[] {new Airport() {Id = "Test", Name = "Test Airport"}}.ToAsyncEnumerable());

            var sut = new WeatherService(CreateContext(), messageBus, NullLogger<WeatherService>.Instance);

            var forecast = new WeatherForecast()
            {
                AirportId = "Invalid",
                Summary = "Warm"
            };
            
            var result = await sut.SaveForecast(forecast);
            result.Id.Should().NotBeEmpty("Id should have been set during save operation");
        }
    }
}