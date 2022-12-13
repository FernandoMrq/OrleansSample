using Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly IClusterClient _client;

        public DeviceController(IClusterClient client)
        {
            _client = client;
        }

        //GET api/devices/5
        [HttpGet("{deviceId}")]
        public async Task<IActionResult> Get(int deviceId)
        {
            try
            {
                var device = _client.GetGrain<IDevice>(deviceId);
                double lastTemperature = await device.GetTemperature();
                return Ok(lastTemperature);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        //PUT api/devices/5
        [HttpPut("{deviceId}")]
        public async Task<IActionResult> Put(int deviceId, [FromForm] string value)
        {
            try
            {
                double.TryParse(value, out var temperature);
                var device = _client.GetGrain<IDevice>(deviceId);
                await device.SetTemperature(temperature);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
