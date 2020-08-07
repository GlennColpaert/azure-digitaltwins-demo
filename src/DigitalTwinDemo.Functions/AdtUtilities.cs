﻿using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DigitalTwinDemo.Functions
{
    static class AdtUtilities
    {
        public static async Task<string> FindParent(DigitalTwinsClient client, string child, string relname, ILogger log)
        {
            // Find parent using incoming relationships
            try
            {
                AsyncPageable<IncomingRelationship> rels = client.GetIncomingRelationshipsAsync(child);

                await foreach (IncomingRelationship ie in rels)
                {
                    if (ie.RelationshipName == relname)
                        return (ie.SourceId);
                }
            }
            catch (RequestFailedException exc)
            {
                log.LogInformation($"*** Error in retrieving parent:{exc.Status}:{exc.Message}");
            }
            return null;
        }
        public static async Task<string> FindParentByQuery(DigitalTwinsClient client, string childId, ILogger log)
        {
            string query = $"SELECT Parent " +
                            $"FROM digitaltwins Parent " +
                            $"JOIN Child RELATED Parent.contains " +
                            $"WHERE Child.$dtId = '" + childId + "'";
            log.LogInformation($"Query: {query}");

            try
            {
                AsyncPageable<string> res = client.QueryAsync(query);

                await foreach (string s in res)
                {
                    JObject parentTwin = (JObject)JsonConvert.DeserializeObject(s);
                    return (string)parentTwin["Parent"]["$dtId"];
                }
                log.LogInformation($"*** No parent found");
            }
            catch (RequestFailedException exc)
            {
                log.LogInformation($"*** Error in retrieving parent:{exc.Status}/{exc.Message}");
            }
            return null;
        }

        public static async Task UpdateTwinProperty(DigitalTwinsClient client, string twinId, string operation, string propertyPath, string schema, string value, ILogger log)
        {
            // Update twin property
            try
            {
                List<object> twinData = new List<object>();
                twinData.Add(new Dictionary<string, object>() {
                    { "op", operation},
                    { "path", propertyPath},
                    { "value", ConvertStringToType(schema, value)}
                });

                await client.UpdateDigitalTwinAsync(twinId, JsonConvert.SerializeObject(twinData));
            }
            catch (RequestFailedException exc)
            {
                log.LogInformation($"*** Error:{exc.Status}/{exc.Message}");
            }
        }

        public static async Task UpdateTwinProperty(DigitalTwinsClient client, string twinId, string propertyPath, object value, ILogger log)
        {
            // If the twin does not exist, this will log an error
            try
            {
                // Update twin property
                List<Dictionary<string, object>> ops = new List<Dictionary<string, object>>();
                ops.Add(new Dictionary<string, object>()
                {
                    { "op", "add"},
                    //{ "op", "replace"},
                    { "path", propertyPath},
                    { "value", value}
                });
                await client.UpdateDigitalTwinAsync(twinId, JsonConvert.SerializeObject(ops));
            }
            catch (RequestFailedException exc)
            {
                log.LogInformation($"*** Error:{exc.Status}/{exc.Message}");
            }
        }

        private static object ConvertStringToType(string schema, string val)
        {
            switch (schema)
            {
                case "bool":
                    return bool.Parse(val);
                case "double":
                    return double.Parse(val);
                case "integer":
                    return Int32.Parse(val);
                case "datetime":
                    return DateTime.Parse(val);
                case "duration":
                    return Int32.Parse(val);
                case "string":
                default:
                    return val;
            }
        }
    }
}
