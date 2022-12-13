using Contracts;
using Orleans;
using Orleans.Providers;
using System.Threading.Tasks;

namespace Grains
{
    [StorageProvider(ProviderName = "Devices")]
    public class DeviceGrain : Grain<Device>, IDevice
    {
        public Task<double> GetTemperature()
        {
            return Task.FromResult(State.LastTemperature);
        }

        public async Task SetTemperature(double temperature)
        {
            State.LastTemperature = temperature;
            await WriteStateAsync();
        }
    }
}
