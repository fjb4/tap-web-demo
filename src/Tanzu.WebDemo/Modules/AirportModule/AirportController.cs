using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tanzu.Common.Messaging;
using Tanzu.WebDemo.Modules.AirportModule.Api;
using Tanzu.Common.Security;

namespace Tanzu.WebDemo.Modules.AirportModule
{
    [ApiController]
    [Route("[controller]")]
    public class AirportController : ControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AirportController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        [HttpGet(Name = "Airport_GetAll")]
        [Authorize(KnownAuthorizationPolicy.AirportRead)]
        public IAsyncEnumerable<Airport> Get() => Get(null);
        [HttpGet("{airportId}", Name = "Airport_GetById")]
        [Authorize(KnownAuthorizationPolicy.AirportRead)]
        public IAsyncEnumerable<Airport> Get(string? airportId) => _messageBus.Send(new AirportQuery{ AirportId = airportId });
        
    }
}