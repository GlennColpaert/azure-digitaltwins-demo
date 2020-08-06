// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Identity;

using Azure.DigitalTwins.Core;
using System.Net.Http;
using Azure.Core.Pipeline;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DigitalTwinDemo.Functions
{
    public static class ProcessHubMessages
    {
        const string adtAppId = "https://digitaltwins.azure.net";
        private static string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("ProcessHubMessages")]
        public static async void Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            // After this is deployed, you need to turn the Managed Identity Status to "On", 
            // Grab Object Id of the function and assigned "Azure Digital Twins Owner (Preview)" role to this function identity
            // in order for this function to be authorized on ADT APIs.

            log.LogInformation(eventGridEvent.Data.ToString());
            DigitalTwinsClient client = null;

            try
            {
                // Authenticate on ADT APIs
                ManagedIdentityCredential cred = new ManagedIdentityCredential(adtAppId);
                client = new DigitalTwinsClient(new Uri(adtInstanceUrl), cred, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(httpClient) });
                log.LogInformation($"ADT service client connection created.");

                if (client != null)
                {
                    if (eventGridEvent != null && eventGridEvent.Data != null)
                    {
                        #region Open this region for message format information
                        // Telemetry message format
                        //{
                        //  "properties": { },
                        //  "systemProperties": 
                        // {
                        //    "iothub-connection-device-id": "thermostat1",
                        //    "iothub-connection-auth-method": "{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}",
                        //    "iothub-connection-auth-generation-id": "637199981642612179",
                        //    "iothub-enqueuedtime": "2020-03-18T18:35:08.269Z",
                        //    "iothub-message-source": "Telemetry"
                        //  },
                        //  "body": "eyJUZW1wZXJhdHVyZSI6NzAuOTI3MjM0MDg3MTA1NDg5fQ=="
                        //}
                        #endregion

                        // Reading deviceId from message headers
                        log.LogInformation(eventGridEvent.Data.ToString());
                        JObject job = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                        string deviceId = (string)job["systemProperties"]["iothub-connection-device-id"];
                        log.LogInformation($"Found device: {deviceId}");

                        // Extracting temperature from device telemetry
                        byte[] body = System.Convert.FromBase64String(job["body"].ToString());
                        var value = System.Text.ASCIIEncoding.ASCII.GetString(body);
                        var bodyProperty = (JObject)JsonConvert.DeserializeObject(value);
                        var temperature = bodyProperty["Temperature"];
                        var humidity = bodyProperty["Humidity"];
                        log.LogInformation($"Device Temperature & Humidity is:{temperature} - {humidity}");

                        // Update device Temperature property
                        await AdtUtilities.UpdateTwinProperty(client, deviceId, "/Temperature", temperature, log);
                        await AdtUtilities.UpdateTwinProperty(client, deviceId, "/Humidity", humidity, log);

                    }
                }
            }
            catch (Exception e)
            {
                log.LogError($"Error: {e.Message}");
            }

        }
    }
}
