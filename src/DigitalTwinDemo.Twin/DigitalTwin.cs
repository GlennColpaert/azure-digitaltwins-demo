using Azure;
using Azure.Core;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DigitalTwinDemo.Twin
{
    public class DigitalTwin
    {
        private DigitalTwinsClient _client;
        private static string clientId = "87ab6291-a6b7-40e3-8e59-5c9bf1847f12";
        private static string tenantId = "d4842539-1664-4a55-981e-a6a6643b6d02";
        private static string adtInstanceUrl = "https://adt-demo-glenn.api.neu.digitaltwins.azure.net";

        const string adtAppId = "https://digitaltwins.azure.net";
        public DigitalTwin()
        {
            var credential = new InteractiveBrowserCredential(tenantId, clientId);
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);
            Utils.IgnoreErrors(() => client.GetDigitalTwin(""));

            _client = client;
        }
        public async Task DeleteAllTwinsAsync()
        {
            Console.WriteLine($"\nDeleting all twins");
            Console.WriteLine($"Step 1: Find all twins", ConsoleColor.DarkYellow);
            List<string> twinlist = new List<string>();
            try
            {
                AsyncPageable<string> qresult = _client.QueryAsync("SELECT * FROM DIGITALTWINS");
                await foreach (string item in qresult)
                {
                    JsonDocument document = JsonDocument.Parse(item);
                    if (document.RootElement.TryGetProperty("$dtId", out JsonElement eDtdl))
                    {
                        try
                        {
                            string twinId = eDtdl.GetString();
                            twinlist.Add(twinId);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("No DTDL property in query result");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: Can't find twin id in query result:\n {item}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in query execution: {ex.Message}");
            }


            Console.WriteLine($"Step 2: Find and remove relationships for each twin...", ConsoleColor.DarkYellow);
            foreach (string twinId in twinlist)
            {
                // Remove any relationships for the twin
                // await FindAndDeleteOutgoingRelationshipsAsync(twinId).ConfigureAwait(false);
                //await FindAndDeleteIncomingRelationshipsAsync(twinId).ConfigureAwait(false);
            }

            Console.WriteLine($"Step 3: Delete all twins", ConsoleColor.DarkYellow);
            foreach (string twinId in twinlist)
            {
                try
                {
                    await _client.DeleteDigitalTwinAsync(twinId).ConfigureAwait(false);
                    Console.WriteLine($"Deleted twin {twinId}");
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"*** Error {ex.Status}/{ex.ErrorCode} deleting twin {twinId} due to {ex.Message}");
                }
            }
        }
    }
}
