using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;

namespace WebApi
{
    public class Startup
    {
        private IClusterClient _client;
        private ILogger<Startup> _logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(CreateClueterClient);
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            _logger = logger;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private IClusterClient CreateClueterClient(IServiceProvider service)
        {
            _client = new ClientBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansSample";
                })
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = "Microsoft.Data.SqlClient";
                    options.ConnectionString = Configuration.GetConnectionString("SampleConnection");
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            StartClientWithRetries(_client).Wait();
            return _client;
        }

        private async Task StartClientWithRetries(IClusterClient client)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    await _client.Connect();
                    _logger.LogInformation("Connected to the Orleans client.");
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error starting Orleans client.");
                }
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
