using System.Collections.Generic;
using MediatR;

namespace Tanzu.WebDemo.Modules.AirportModule.Api
{
    partial class AirportQuery : IRequest<IAsyncEnumerable<Airport>>
    {
        
    }
}