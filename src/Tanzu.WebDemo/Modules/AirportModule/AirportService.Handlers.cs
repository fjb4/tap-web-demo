using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Tanzu.WebDemo.Modules.AirportModule.Api;

#pragma warning disable 1998

namespace Tanzu.WebDemo.Modules.AirportModule
{
    partial class AirportService : IRequestHandler<AirportQuery, IAsyncEnumerable<Airport>>
    {
        public async Task<IAsyncEnumerable<Airport>> Handle(AirportQuery request, CancellationToken cancellationToken) => GetAirports(request);
    }
}