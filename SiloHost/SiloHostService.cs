using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiloHost
{
    public class SiloHostService : IHostedService
    {
        private IApplicationLifetime _applicationLifetime;
        private ILogger<SiloHostService> _logger;
        private IConfiguration _configuration;
        private ISiloHost _siloHost;

        public SiloHostService(IApplicationLifetime applicationLifetime,
            ILogger<SiloHostService> logger,
            IConfiguration configuration)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
            _configuration = configuration;
        }

        private void OnStarted()
        {
            try
            {
                int portAdd = _configuration.GetValue<int>("portAdd");
                StartSilo(portAdd).Wait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while instantiating silo");
            }
        }

        private void OnStopping()
        {

        }

        private void OnStopped()
        {

        }

        private async Task StartSilo(int portAdd)
        {
            var builder = new SiloHostBuilder()
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "OrleansSample";
            })
            .Configure<GrainCollectionOptions>(options =>
            {
                options.CollectionAge = TimeSpan.FromMinutes(5);
                options.CollectionQuantum = TimeSpan.FromMinutes(3);
            })
            .UseAdoNetClustering(options =>
            {
                options.Invariant = "Microsoft.Data.SqlClient";
                options.ConnectionString = _configuration.GetConnectionString("SampleConnection");
            })
            .AddAdoNetGrainStorage("Devices", options =>
            {
                options.Invariant = "Microsoft.Data.SqlClient";
                options.ConnectionString = _configuration.GetConnectionString("SampleConnection");
                options.UseJsonFormat = true;
            })
            .ConfigureEndpoints(siloPort: 11111 + portAdd, gatewayPort: 30000 + portAdd)
            .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory())
            .UseDashboard(options => options.Port = 8080 + portAdd)
            .UsePerfCounterEnvironmentStatistics()
            //.UseLinuxEnvironmentStatistics()
            .ConfigureLogging(logging => logging.AddConsole());

            _siloHost = builder.Build();
            await _siloHost.StartAsync();
            _logger.LogInformation("Host Started.");
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopping.Register(OnStopping);
            _applicationLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
