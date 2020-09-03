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
        private static string clientId = "";
        private static string tenantId = "";
        private static string adtInstanceUrl = "";

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
                { "$model", "dtmi:demo:House;1"},
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

            metaData = new Dictionary<string, object>()
            {
                { "$model", "dtmi:demo:Sensor;1"},
                { "$kind", "DigitalTwin" }
            };

            //Yes you have to initialize Temp and Hum as default
            //Unless you do an add (instead of replace) as update operation to the query interface.
            twinData = new Dictionary<string, object>()
            {
                 { "$metadata", metaData},
                 { "FirmwareVersion", "2020.ADT.5782.3" }
                //,
                // { "Temperature", 0 },
                // { "Humidity", 0},
            };

            await _client.CreateDigitalTwinAsync("TStat001", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"TStat001 Twin created successfully!");

            await _client.CreateDigitalTwinAsync("TStat002", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"TStat002 Twin created successfully!");

            await _client.CreateDigitalTwinAsync("TStat003", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"TStat003 Twin created successfully!");

            await _client.CreateDigitalTwinAsync("TStat004", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"TStat004 Twin created successfully!");

            await _client.CreateDigitalTwinAsync("TStat005", JsonSerializer.Serialize(twinData));
            Console.WriteLine($"TStat005 Twin created successfully!");


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

            body = new Dictionary<string, object>()
            {
                { "$targetId", "TStat001"},
                { "$relationshipName", "sensors"}
            };
            await _client.CreateRelationshipAsync("Kitchen", "kitchen_to_tstat001", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship kitchen_to_tstat001 created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "TStat001"},
                { "$relationshipName", "sensors"}
            };
            await _client.CreateRelationshipAsync("LivingRoom", "livingroom_to_tstat001", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship LivingRoom_to_tstat001 created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "TStat002"},
                { "$relationshipName", "sensors"}
            };
            await _client.CreateRelationshipAsync("Bedroom1", "bedroom_to_tstat002", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship bedroom_to_tstat002 created successfully!");


            body = new Dictionary<string, object>()
            {
                { "$targetId", "TStat003"},
                { "$relationshipName", "sensors"}
            };
            await _client.CreateRelationshipAsync("Bedroom2", "bedroom2_to_tstat003", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship bedroom2_to_tstat003 created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "TStat004"},
                { "$relationshipName", "sensors"}
            };
            await _client.CreateRelationshipAsync("Bathroom", "bathroom_to_tstat004", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship bathroom_to_tstat004 created successfully!");

            body = new Dictionary<string, object>()
            {
                { "$targetId", "TStat005"},
                { "$relationshipName", "sensors"}
            };
            await _client.CreateRelationshipAsync("MasterBedroom", "masterbedroom_to_tstat005", JsonSerializer.Serialize(body));
            Console.WriteLine($"Relationship masterbedroom_to_tstat005 created successfully!");

        }

        private List<string> ParseDTDLModels()
        {
            string[] models = new string[4] { "floor", "house", "room", "thermostat" };
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
