using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alba;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace PricingService.IntegrationTest;

public class IntegrationTestsFixture : IAsyncLifetime
{
    public IAlbaHost SystemUnderTest { get; private set; }

    public Task InitializeAsync()
    {
        var hostBuilder = Program.CreateWebHostBuilder(Array.Empty<string>())
            .ConfigureServices((Action<HostBuilderContext, IServiceCollection>)SetupServices)
            .ConfigureAppConfiguration((_, configBuilder) => configBuilder.AddInMemoryCollection(new Dictionary<string, string>()));

        SystemUnderTest = new AlbaHost(hostBuilder);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await SystemUnderTest.DisposeAsync();
    }

    protected virtual void SetupServices(HostBuilderContext ctx, IServiceCollection services)
    {
    }

    protected Task SetupData()
    {
        return Task.CompletedTask;
    }
}
