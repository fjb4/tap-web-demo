using System.Collections.Generic;
using System.Linq;
using LinqKit;
using Microsoft.Extensions.Logging;
using Tanzu.Common.Modules;
using Tanzu.WebDemo.Modules.AirportModule.Api;
using Tanzu.WebDemo.Modules.WeatherModule;
using Tanzu.WebDemo.Persistence;

namespace Tanzu.WebDemo.Modules.AirportModule
{
    public partial class AirportService : IService
    {
        private readonly WebDemoContext _context;
        private readonly ILogger _logger;

        public AirportService(WebDemoContext context, ILogger<AirportService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IAsyncEnumerable<Airport> GetAirports(AirportQuery query)
        {
            var predicate = PredicateBuilder.New<Airport>(true);
            if (query.AirportId != null)
            {
                predicate = predicate.And(x => x.Id == query.AirportId);
            }

            return _context.Airports
                .AsQueryable()
                .Where(predicate)
                .ToAsyncEnumerable();
        }
    }
}