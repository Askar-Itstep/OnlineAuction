using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataLayer
{
    public class MyConfig: DbConfiguration
    {
        public  static string connectionString = ""; //AWSRDS.db.UpdAuction->.Image.Url ="https://...azureContainer...
        public  static string azureUrl = "";
        public  static string awsUrl = "";
        public static string connectionString2 = ""; //AWSRDS.db.Auction->.Image.Url ="https://...azureContainer...
        public static string tempConnectString = "";
        //GoogleFirebase
        public static string authSecretFirebase="";
        public static string basePathFirebase = "";

        public MyConfig()
        {

            GetSecret();
            SqlConnectionFactory defaultFactory =  new SqlConnectionFactory(connectionString);

            this.SetDefaultConnectionFactory(defaultFactory);
        }
        public static void GetSecret()
        {
            string secretName = "conn/MSSQL";
            string region = "us-west-2";
            string secret = "";

            //tutorial  code 
            MemoryStream memoryStream = new MemoryStream();
            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest();
            request.SecretId = secretName;
            request.VersionStage = "AWSCURRENT"; 

            GetSecretValueResponse response = null;

            try
            {
                response = client.GetSecretValueAsync(request).Result;
            }
            catch (Exception e)
            {
                throw;
            }

            if (response.SecretString != null)
            {
                secret = response.SecretString;
            }
            else
            {
                memoryStream = response.SecretBinary;
                StreamReader reader = new StreamReader(memoryStream);
                string decodedBinarySecret = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
            }

            // Your code goes here.

            var json = JsonSerializer.Deserialize<ModelResponseAws>(secret);
            connectionString = json.ConnectionString;
            azureUrl = json.AzureUrl;
            awsUrl = json.AwsUrl;
            connectionString2 = json.ConnectionString2;

            //GoogleFirebase
            authSecretFirebase = json.AuthSecretFirebase;
            basePathFirebase = json.BasePathbase;
        }

    }
    
}
