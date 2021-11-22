using System.Collections.Generic;
using MediatR;

namespace Tanzu.WebDemo.Modules.WeatherModule.Api
{
    partial class WeatherForecastQuery : IRequest<IAsyncEnumerable<WeatherForecast>>
    {
        
    }
}