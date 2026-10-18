using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Converters;
using PricingService.Commands;
using PricingService.Data;
using Steeltoe.Discovery.Eureka;

namespace PricingService;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDataStore, InMemoryDataStore>();
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<CalculatePriceHandler>());
        services.AddEurekaDiscoveryClient();
        services.AddControllers()
            .AddNewtonsoftJson(settings => settings.SerializerSettings.Converters.Add(new StringEnumConverter()));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (ValidationException exception)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    errors = exception.Errors.Select(error => new { error.PropertyName, error.ErrorMessage })
                });
            }
        });

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
