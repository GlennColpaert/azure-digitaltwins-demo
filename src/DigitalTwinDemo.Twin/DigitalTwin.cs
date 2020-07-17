using Azure;
using Azure.Core;
using Azure.DigitalTwins.Core;
using Azure.DigitalTwins.Core.Serialization;
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




        /// <summary>
        /// Delete all the Twins and their relationships
        /// </summary>
        /// <returns></returns>
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
                    document.RootElement.TryGetProperty("$dtId", out JsonElement eDtdl);
                    twinlist.Add(eDtdl.GetString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in query execution: {ex.Message}");
            }
            
            Console.WriteLine($"Step 2: Find and remove relationships for each twin...", ConsoleColor.DarkYellow);
            foreach (string twinId in twinlist)
            {
                await FindAndDeleteOutgoingRelationshipsAsync(twinId).ConfigureAwait(false);
                await FindAndDeleteIncomingRelationshipsAsync(twinId).ConfigureAwait(false);
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

        public async Task FindAndDeleteOutgoingRelationshipsAsync(string twinId)
        {
            try
            {
                await foreach (string relJson in _client.GetRelationshipsAsync(twinId))
                {
                    var rel = JsonSerializer.Deserialize<BasicRelationship>(relJson);
                    await _client.DeleteRelationshipAsync(twinId, rel.Id).ConfigureAwait(false);
                    Console.WriteLine($"Deleted relationship {rel.Id} from {twinId}");
                }
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"*** Error {ex.Status}/{ex.ErrorCode} retrieving or deleting relationships for {twinId} due to {ex.Message}");
            }
        }

        async Task FindAndDeleteIncomingRelationshipsAsync(string twinId)
        {
            try
            {
                await foreach (IncomingRelationship incomingRel in _client.GetIncomingRelationshipsAsync(twinId))
                {
                    await _client.DeleteRelationshipAsync(incomingRel.SourceId, incomingRel.RelationshipId).ConfigureAwait(false);
                    Console.WriteLine($"Deleted incoming relationship {incomingRel.RelationshipId} from {twinId}");
                }
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"*** Error {ex.Status}/{ex.ErrorCode} retrieving or deleting incoming relationships for {twinId} due to {ex.Message}");
            }
        }
    }
}
