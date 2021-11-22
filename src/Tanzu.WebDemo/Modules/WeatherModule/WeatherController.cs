using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tanzu.Common.Messaging;
using Tanzu.WebDemo.Modules.WeatherModule.Api;

using Tanzu.Common.Security;

namespace Tanzu.WebDemo.Modules.WeatherModule
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IMessageBus _messageBus;

        public WeatherController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        [HttpGet]
        [Authorize(KnownAuthorizationPolicy.WeatherRead)]
        public IAsyncEnumerable<WeatherForecast> Get([FromQuery] WeatherForecastQuery query) => _messageBus.Send(query);

        [HttpGet("{airportId}", Name = "Weather_GetById")]
        [Authorize(KnownAuthorizationPolicy.WeatherRead)]
        public async Task<ActionResult<WeatherForecast>> Get(string airportId)
        {
            var forecast = await Get(new WeatherForecastQuery {AirportId = airportId}).FirstOrDefaultAsync();
            if (forecast == null)
            {
                return NotFound(airportId);
            }
            return forecast;
        }

        [HttpPost]
        [HttpPut(Name = "Weather_Put")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(KnownAuthorizationPolicy.WeatherWrite)]
        public async Task<ActionResult<WeatherForecast>> Post(WeatherForecast forecast)
        {
            return await _messageBus.Send(forecast);
        }
    }
}