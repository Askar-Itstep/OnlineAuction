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
            //SqlConnectionFactory defaultFactory =  new SqlConnectionFactory(connectionString);
            //this.SetDefaultConnectionFactory(defaultFactory);
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
            catch (DecryptionFailureException e)
            {
                // Secrets Manager can't decrypt the protected secret text using the provided KMS key.
                // Deal with the exception here, and/or rethrow at your discretion.
                throw;
            }
            catch (InternalServiceErrorException e)
            {
                // An error occurred on the server side.
                // Deal with the exception here, and/or rethrow at your discretion.
                throw;
            }
            catch (InvalidParameterException e)
            {
                // You provided an invalid value for a parameter.
                // Deal with the exception here, and/or rethrow at your discretion
                throw;
            }
            catch (InvalidRequestException e)
            {
                // You provided a parameter value that is not valid for the current state of the resource.
                // Deal with the exception here, and/or rethrow at your discretion.
                throw;
            }
            catch (ResourceNotFoundException e)
            {
                // We can't find the resource that you asked for.
                // Deal with the exception here, and/or rethrow at your discretion.
                throw;
            }
            catch (System.AggregateException ae)
            {
                // More than one of the above exceptions were triggered.
                // Deal with the exception here, and/or rethrow at your discretion.
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
