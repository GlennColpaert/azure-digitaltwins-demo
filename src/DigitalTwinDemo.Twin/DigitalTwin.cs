using Azure;
using Azure.Core;
using Azure.DigitalTwins.Core;
using Azure.DigitalTwins.Core.Serialization;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.IO;
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

        private CommandLoop _commandLoop;

        const string adtAppId = "https://digitaltwins.azure.net";
        public DigitalTwin()
        {
            var credential = new InteractiveBrowserCredential(tenantId, clientId);
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);
            Utils.IgnoreErrors(() => client.GetDigitalTwin(""));

            _client = client;

            _commandLoop = new CommandLoop(client);
        }

        public async Task CreateHouseTwin()
        {         
            try
            {
                List<string> dtdlList = ParseDTDLModels();
                Response<ModelData[]> res = await _client.CreateModelsAsync(dtdlList);
                Console.WriteLine($"Model(s) created successfully!");
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"Response {e.Status}: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            var metaData = new Dictionary<string, object>()
            {
                { "$model", "dtmi:demo:House;2"},
                { "$kind", "DigitalTwin" }
            };

            var twinData = new Dictionary<string, object>()
            {
                 { "$metadata", metaData},
                 { "ConstructionYear", "1985" },
                 { "Owner", "Glenn Colpaert" }
            };

            await _client.CreateDigitalTwinAsync("127.0.0.1", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"Localhost Twin created successfully!");

            metaData = new Dictionary<string, object>()
            {
                { "$model", "dtmi:demo:Floor;1"},
                { "$kind", "DigitalTwin" }
            };

            twinData = new Dictionary<string, object>()
            {
                 { "$metadata", metaData}
            };

            await _client.CreateDigitalTwinAsync("Floor1", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"Floor1 Twin created successfully!");

            await _client.CreateDigitalTwinAsync("Floor2", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"Floor2 Twin created successfully!");

            await _client.CreateDigitalTwinAsync("Floor3", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"Floor3 Twin created successfully!");

            metaData = new Dictionary<string, object>()
            {
                { "$model", "dtmi:demo:Room;1"},
                { "$kind", "DigitalTwin" }
            };

            twinData = new Dictionary<string, object>()
            {
                 { "$metadata", metaData}
            };

            await _client.CreateDigitalTwinAsync("Kitchen", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"Kitchen Twin created successfully!");

            await _client.CreateDigitalTwinAsync("LivingRoom", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"LivingRoom Twin created successfully!");

            await _client.CreateDigitalTwinAsync("Bedroom1", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"Bedroom1 Twin created successfully!");

            await _client.CreateDigitalTwinAsync("Bedroom2", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"Bedroom2 Twin created successfully!");

            await _client.CreateDigitalTwinAsync("Bathroom", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"Bathroom Twin created successfully!");

            await _client.CreateDigitalTwinAsync("MasterBedroom", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"MasterBedroom Twin created successfully!");


            var body = new Dictionary<string, object>()
            {
                { "$targetId", "Floor1"},
                { "$relationshipName", "floors"}
            };
            await _client.CreateRelationshipAsync("127.0.0.1", "localhost_to_Floor1", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship localhost_to_Floor1 created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "Floor2"},
                { "$relationshipName", "floors"}
            };
            await _client.CreateRelationshipAsync("127.0.0.1", "localhost_to_floor2", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship localhost_to_floor2 created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "Floor3"},
                { "$relationshipName", "floors"}
            };
            await _client.CreateRelationshipAsync("127.0.0.1", "localhost_to_floor3", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship localhost_to_floor3 created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "Kitchen"},
                { "$relationshipName", "rooms"}
            };
            await _client.CreateRelationshipAsync("Floor1", "Floor1_to_kitchen", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship Floor1_to_kitchen created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "LivingRoom"},
                { "$relationshipName", "rooms"}
            };
            await _client.CreateRelationshipAsync("Floor1", "Floor1_to_livingroom", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship Floor1_to_livingroom created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "Bathroom"},
                { "$relationshipName", "rooms"}
            };
            await _client.CreateRelationshipAsync("Floor2", "floor2_to_bathroom", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship floor2_to_bathroom created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "Bedroom1"},
                { "$relationshipName", "rooms"}
            };
            await _client.CreateRelationshipAsync("Floor2", "floor2_to_bedroom1", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship floor2_to_bedroom1 created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "Bedroom2"},
                { "$relationshipName", "rooms"}
            };
            await _client.CreateRelationshipAsync("Floor2", "floor2_to_bedroom2", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship floor2_to_bedroom2 created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "MasterBedroom"},
                { "$relationshipName", "rooms"}
            };
            await _client.CreateRelationshipAsync("Floor3", "floor3_to_masterbedroom", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship floor3_to_masterbedroom created successfully!");

        }

        private List<string> ParseDTDLModels()
        {
            string[] models = new string[3] { "floor", "house", "room" };
            string consoleAppDir = Path.Combine(Directory.GetCurrentDirectory(), @"Models");

            List<string> dtdlList = new List<string>();
            for (int i = 0; i < models.Length; i++)
            {
                string filename = Path.Combine(consoleAppDir, models[i] + ".json");
                StreamReader r = new StreamReader(filename);
                string dtdl = r.ReadToEnd();
                r.Close();
                dtdlList.Add(dtdl);
            }

            return dtdlList;
        }

        /// <summary>
        /// Cleans the existing environment by deleting all the Twins and their relationships
        /// </summary>
        /// <returns></returns>
        public async Task CleanupEnvironment()
        {
            await _commandLoop.DeleteAllTwinsAsync();
        }


    }
}
