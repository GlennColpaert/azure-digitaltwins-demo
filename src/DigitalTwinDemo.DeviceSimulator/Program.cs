using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace DigitalTwinDemo.DeviceSimulator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string sDevice01 = ConfigurationManager.AppSettings["DEVICE001"];
            string sDevice02 = ConfigurationManager.AppSettings["DEVICE002"];

            Task task1 = Task.Factory.StartNew(() => doStuffAsync("Device001",sDevice01));
            Task task2 = Task.Factory.StartNew(() => doStuffAsync("Device002", sDevice02));

            await Task.WhenAll(task1, task2);
            Console.ReadLine();
        }

        static async Task doStuffAsync(string deviceName, string connectionString)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(connectionString);

            double avgTemperature = 21;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = avgTemperature + rand.NextDouble() * 4 - 3;

                var telemetryDataPoint = new
                {
                    Temperature = currentTemperature
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message for Device {1}: {2}", DateTime.Now, deviceName, messageString);
                await Task.Delay(5000);
            }
        }
    }
}
