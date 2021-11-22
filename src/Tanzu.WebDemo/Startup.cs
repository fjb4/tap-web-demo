using System;
using System.Data;
using System.Linq;
using idunno.Authentication.Basic;
using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Tanzu.Common;
using Tanzu.Common.Messaging;
using Tanzu.Common.Modules;
using Tanzu.Common.Persistence;
using Tanzu.Common.Security;
using Tanzu.WebDemo.Persistence;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Connector;
using Steeltoe.Extensions.Logging;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Tracing;
using DbType = Tanzu.Common.Persistence.DbType;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Steeltoe.Extensions.Configuration.ConfigServer;

namespace Tanzu.WebDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
            BootstrapLoggerFactory.Update(configuration);
            Logger = BootstrapLoggerFactory.Instance.CreateLogger<Startup>();
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public ILogger Logger { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (Environment.IsDevelopment())
            {
                // remove zipkin trace ids from logs when running in local development
                services.AddSingleton<IDynamicMessageProcessor, NullLogProcessor>();
            }
            services.AddDistributedTracingAspNetCore();
            
            services.AddSecureActuators();
            services.AddConfigServerHealthContributor();
            if (Configuration.GetValue<string>("Spring:Boot:Admin:Client:Url") != null)
            {
                services.AddSpringBootAdmin();
            }
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(cfg =>
                {
                    cfg.ForwardDefaultSelector = httpContext => (httpContext.Request.Path.StartsWithSegments("/actuator")? BasicAuthenticationDefaults.AuthenticationScheme : null)!;
                    Configuration.GetSection($"Authentication:{JwtBearerDefaults.AuthenticationScheme}").Bind(cfg);
                });
            services.AddAuthorization(authz =>
            {
                authz.AddPolicy(KnownAuthorizationPolicy.AirportRead, policy => policy.RequireScope(KnownScope.Read));
                authz.AddPolicy(KnownAuthorizationPolicy.WeatherRead, policy => policy.RequireScope(KnownScope.Read));
                authz.AddPolicy(KnownAuthorizationPolicy.WeatherWrite, policy => policy.RequireScope(KnownScope.Write));
            });
            services.AddMediatR(cfg => cfg.Using<MessageBus>(), typeof(Startup));
            services.AddTransient(svc => (IMessageBus) svc.GetRequiredService<IMediator>());
            services.AddModules();
            services.AddDbContext<WebDemoContext>(opt =>
            {
                var connectionString = Configuration.GetConnectionString("database");
                var dbDriver = Configuration.GetValue<DbType>("DbType");
                switch (dbDriver)
                {
                    case DbType.SQLite:
                        if (connectionString.Contains(":memory") || connectionString.Contains("mode=memory"))
                        {
                            // in memory database needs to have its connection permanently open or it will get auto-deleted
                            var keepAliveConnection = new SqliteConnection(connectionString);
                            keepAliveConnection.Open();
                            opt.UseSqlite(keepAliveConnection);
                        }
                        else
                        {
                            opt.UseSqlite(connectionString);
                        }

                        break;
                    case DbType.PostgreSQL:
                        opt.UseNpgsql(connectionString);
                        break;

                }
            });
            services.AddScoped<IDbConnection>(ctx => ctx.GetRequiredService<WebDemoContext>().Database.GetDbConnection());
            services.AddScoped<IHealthContributor, RelationalDbHealthContributor>(); // allow db connection health to show up in actuator health endpoint
            services.AddControllers(cfg => cfg.Filters.Add<DomainExceptionFilter>()); // respond with HTTP400 if domain exception is thrown
            services.AddSwaggerGen(c =>
            {
                c.CustomOperationIds(api =>
                {
                    var actionDescriptor = (ControllerActionDescriptor) api.ActionDescriptor;
                    HttpMethodAttribute? methodAttribute = api.HttpMethod switch
                    {
                        "GET" => actionDescriptor.EndpointMetadata.OfType<HttpGetAttribute>().FirstOrDefault(),
                        "POST" =>  actionDescriptor.EndpointMetadata.OfType<HttpPostAttribute>().FirstOrDefault(),
                        "PUT" =>  actionDescriptor.EndpointMetadata.OfType<HttpPutAttribute>().FirstOrDefault(),
                        "DELETE" =>  actionDescriptor.EndpointMetadata.OfType<HttpDeleteAttribute>().FirstOrDefault(),
                        "PATCH" =>  actionDescriptor.EndpointMetadata.OfType<HttpPatchAttribute>().FirstOrDefault(),
                        "OPTIONS" =>  actionDescriptor.EndpointMetadata.OfType<HttpOptionsAttribute>().FirstOrDefault(),
                        "HEAD" =>  actionDescriptor.EndpointMetadata.OfType<HttpHeadAttribute>().FirstOrDefault(),
                        _ => null!
                    };
                    if (methodAttribute?.Name is not null)
                    {
                        return methodAttribute.Name;
                    }

                    return $"{actionDescriptor.ControllerName}_{actionDescriptor.ActionName}"; 

                    // return $"{((ControllerActionDescriptor) api.ActionDescriptor).ControllerName}_{api.HttpMethod}_{string.Join("_", api.ParameterDescriptions.Select(x => x.Name))}".ToLower();
                });
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Tanzu.WebDemo", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            BootstrapLoggerFactory.Update(loggerFactory);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.MigrateDatabase<WebDemoContext>();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tanzu.WebDemo v1");
            });
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}