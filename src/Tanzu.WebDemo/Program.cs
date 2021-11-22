using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tanzu.Common;
using Tanzu.Common.Configuration;
using Steeltoe.Extensions.Logging;
using Steeltoe.Management.Endpoint;

namespace Tanzu.WebDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddDynamicLogging()
                .UseYamlWithProfilesAppConfiguration<Program>(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        

    }
}