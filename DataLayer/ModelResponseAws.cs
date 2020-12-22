using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataLayer
{
    public class ModelResponseAws
    {
        [JsonPropertyName("connectString")]
        public string ConnectionString { get; set; }

        [JsonPropertyName("azureUrl")]
        public string AzureUrl { get;  set; }

        [JsonPropertyName("awsUrl")]
        public string AwsUrl { get; set; }

        [JsonPropertyName("connectString2")]
        public string ConnectionString2 { get;  set; }

        //[System.Text.Json.Serialization.JsonIgnore]
        //public string UserName { get; set; }
    }
}
