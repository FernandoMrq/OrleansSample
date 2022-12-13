using Orleans;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IDevice : IGrainWithIntegerKey
    {
        Task SetTemperature(double temperature);
        Task<double> GetTemperature();
    }
}
