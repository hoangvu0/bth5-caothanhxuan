using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PricingService.Api.Commands;
using PricingService.Behaviors;
using PricingService.DataAccess;
using PricingService.Domain;
using Steeltoe.Discovery.Client;

namespace PricingService;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDiscoveryClient(Configuration);
        services.AddControllers().AddNewtonsoftJson();
        services.AddSwaggerGen();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Startup>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddTransient<IValidator<CalculatePriceCommand>, CalculatePriceCommandValidator>();
        services.AddSingleton<IDataStore, InMemoryDataStore>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
