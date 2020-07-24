using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Azure.DigitalTwins.Core;
using Azure.DigitalTwins.Core.Serialization;
using Azure;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.DigitalTwins.Parser;

/// <summary>
/// Taken from the Azure Digital Twin official Sample
/// https://github.com/Azure-Samples/digital-twins-samples/tree/master/
/// </summary>
namespace DigitalTwinDemo.Twin
{
    public class CommandLoop
    {
        private DigitalTwinsClient client;

        public CommandLoop(DigitalTwinsClient _client)
        {
            client = _client;
        }

        public async Task FindAndDeleteOutgoingRelationshipsAsync(string dtId)
        {
            // Find the relationships for the twin

            try
            {
                // GetRelationshipsAsync will throw if an error occurs
                AsyncPageable<string> relsJson = client.GetRelationshipsAsync(dtId);

                await foreach (string relJson in relsJson)
                {
                    var rel = System.Text.Json.JsonSerializer.Deserialize<BasicRelationship>(relJson);
                    await client.DeleteRelationshipAsync(dtId, rel.Id).ConfigureAwait(false);
                    Console.WriteLine($"Deleted relationship {rel.Id} from {dtId}");
                }
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"*** Error {ex.Status}/{ex.ErrorCode} retrieving or deleting relationships for {dtId} due to {ex.Message}");
            }
        }

        async Task FindAndDeleteIncomingRelationshipsAsync(string dtId)
        {
            // Find the relationships for the twin

            try
            {
                // GetRelationshipssAsync will throw if an error occurs
                AsyncPageable<IncomingRelationship> incomingRels = client.GetIncomingRelationshipsAsync(dtId);

                await foreach (IncomingRelationship incomingRel in incomingRels)
                {
                    await client.DeleteRelationshipAsync(incomingRel.SourceId, incomingRel.RelationshipId).ConfigureAwait(false);
                    Console.WriteLine($"Deleted incoming relationship {incomingRel.RelationshipId} from {dtId}");
                }
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"*** Error {ex.Status}/{ex.ErrorCode} retrieving or deleting incoming relationships for {dtId} due to {ex.Message}");
            }
        }

        public async Task DeleteAllTwinsAsync()
        {
            Console.WriteLine($"\nDeleting all twins");
            Console.WriteLine($"Step 1: Find all twins", ConsoleColor.DarkYellow);
            List<string> twinlist = new List<string>();
            try
            {
                AsyncPageable<string> qresult = client.QueryAsync("SELECT * FROM DIGITALTWINS");
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
                await FindAndDeleteOutgoingRelationshipsAsync(twinId).ConfigureAwait(false);
                await FindAndDeleteIncomingRelationshipsAsync(twinId).ConfigureAwait(false);
            }

            Console.WriteLine($"Step 3: Delete all twins", ConsoleColor.DarkYellow);
            foreach (string twinId in twinlist)
            {
                try
                {
                    await client.DeleteDigitalTwinAsync(twinId).ConfigureAwait(false);
                    Console.WriteLine($"Deleted twin {twinId}");
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"*** Error {ex.Status}/{ex.ErrorCode} deleting twin {twinId} due to {ex.Message}");
                }
            }
        }

        public void LogResponse(string res, string type = "")
        {
            if (type != "")
                Console.WriteLine($"{type}: \n");
            else
                Console.WriteLine("Response:");
            if (res == null)
                Console.WriteLine("Null response");
            else
            {
                Console.WriteLine(PrettifyJson(res));
            }
        }

        private string PrettifyJson(string json)
        {
            object jsonObj = System.Text.Json.JsonSerializer.Deserialize<object>(json);
            return System.Text.Json.JsonSerializer.Serialize(jsonObj, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
    }
}
