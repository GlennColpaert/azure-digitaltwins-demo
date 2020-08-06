using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace DigitalTwinDemo.DeviceSimulator
{
    public class Program
    {
        static readonly double avgTemperature = 21;
        static readonly double avgHumidity = 50;
        static readonly Random rand = new Random();

        static async Task Main(string[] args)
        {
            string sDevice01 = ConfigurationManager.AppSettings["TStat001"];
            string sDevice02 = ConfigurationManager.AppSettings["TStat001"];

            Task task1 = Task.Factory.StartNew(() => SimulateDeviceAsync("Device001", sDevice01));
            Task task2 = Task.Factory.StartNew(() => SimulateDeviceAsync("Device002", sDevice02));

            await Task.WhenAll(task1, task2);
            Console.ReadLine();
        }

        static async Task SimulateDeviceAsync(string deviceName, string connectionString)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(connectionString);

            while (true)
            {
                double currentTemperature = avgTemperature + rand.NextDouble() * 4 - 3;
                double currentHumidity = avgHumidity + rand.NextDouble() * 4 - 3;

                var telemetryMessage = new
                {
                    Temperature = currentTemperature,
                    Humidity = currentHumidity
                };

                var messageString = JsonConvert.SerializeObject(telemetryMessage);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message for Device {1}: {2}", DateTime.Now, deviceName, messageString);
                await Task.Delay(5000);
            }
        }
    }
}
