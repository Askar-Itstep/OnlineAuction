using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using BusinessLayer.BusinessObject;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OnlineAuction.ServiceClasses
{
    //--------AZURE------------------------
    public class BlobHelper
    {
        const string blobContainerName = "blobcontainer";
        private const string uripath = "https://storageauction.blob.core.windows.net/blobcontainer";

        public static async Task<ImageBO> SetImageAsync(HttpPostedFileBase upload, ImageVM imageVM, ImageBO imageBase, IMapper mapper, AccountBO userBO = null)
        {
            //а)------запись в blobStorage ----------------------            
            string filename = Path.GetFileName(upload.FileName);
            bool resUpload = await UploadFile(upload);
            //б)-----запись в БД----------
            if (resUpload == true)
            {
                string uriStr = uripath + "/" + filename;
                imageVM.FileName = filename;
                imageVM.URI = new Uri(uriStr);
                var imgListBO = DependencyResolver.Current.GetService<ImageBO>().LoadAll().Where(i => i.FileName == imageVM.FileName).ToList();
                if (imgListBO == null || imgListBO.Count() == 0)                    //если такого в БД нет - сохранить
                {
                    var imageBO = mapper.Map<ImageBO>(imageVM);
                    imageBase.Save(imageBO);
                }
                List<ImageBO> imageBases = DependencyResolver.Current.GetService<ImageBO>().LoadAll().Where(i => i.FileName == imageVM.FileName).ToList();
                imageBase = imageBases[0];
                if (userBO != null)
                {
                    userBO.ImageId = imageBase.Id;
                }
            }
            return imageBase;
        }
        //----------------запись в AzureBlobContainer-------------------------------
        public static async Task<bool> UploadFile(HttpPostedFileBase upload)
        {
            try
            {
                string filename = Path.GetFileName(upload.FileName);
                string storagekey = ConfigurationManager.ConnectionStrings["blobContainer"].ConnectionString;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storagekey);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(blobContainerName);             //.. .windows.net/containerblob";
                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);

                using (var fileStream = upload.InputStream)
                {  //System.IO.File.OpenRead(@"path\myfile")
                    await blockBlob.UploadFromStreamAsync(fileStream);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Upload Error: " + ex.Message);
                return false;
            }
        }

        //--------------------------------------- AWS-------------------------------------------------------

        public static async Task<bool> UploadJsonAWSbucket(string filepath)
        {
            bool flagGoodRequest = true;
            try
            {
                var options = new CredentialProfileOptions
                {
                    AccessKey = "AKIA4LDGGJIMCE3BJE3C",
                    SecretKey = "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"
                };
                var profile = new CredentialProfile("dotnet-tutorials", options);
                profile.Region = RegionEndpoint.EUWest1;         //.USWest2;
                var sharedFile = new SharedCredentialsFile();
                //sharedFile.RegisterProfile(profile); //попробовать после 01.12.20

                if (sharedFile.TryGetProfile("dotnet-tutorials", out profile)
                    && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out AWSCredentials awsCredentials))
                {
                    using (var client = new AmazonS3Client(awsCredentials, profile.Region))
                    {
                        //a) Create a PutObject request
                        PutObjectRequest request = new PutObjectRequest
                        {
                            BucketName = "auction-predict-bucket",
                            Key = "json.json",
                            FilePath = filepath
                        };

                        //b) Put object
                        PutObjectResponse response = await client.PutObjectAsync(request);
                    }
                }
            }
            catch (Exception e)
            {
                flagGoodRequest = false;
                System.Diagnostics.Debug.WriteLine("Error: " + e.Message);
            }
            if (flagGoodRequest) return true;
            else return false;
        }

        internal static async Task<Tuple<bool, string>> PutFileToS3Async(string bucketname, string keyname, string json)
        {
            // Create file in memory
            UnicodeEncoding uniEncoding = new UnicodeEncoding();

            // Create the data to write to the stream.
            byte[] memstring = uniEncoding.GetBytes(json);
            string message = "";
            bool flag = false;
            #region
            //try
            //{
            //    var options = new CredentialProfileOptions
            //    {
            //        AccessKey = "AKIA4LDGGJIMCE3BJE3C",
            //        SecretKey = "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"
            //    };
            //    var profile = new CredentialProfile("dotnet-tutorials", options);
            //    profile.Region = RegionEndpoint.EUWest1;         //.USWest2;
            //    var sharedFile = new SharedCredentialsFile();
            //    sharedFile.RegisterProfile(profile);  //access denied!???????????????????????????

            //    if (AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out AWSCredentials awsCredentials))
            //    {
            //        using (MemoryStream memStream = new MemoryStream(1024))
            //        {
            //            memStream.Write(memstring, 0, memstring.Length);
            //            try
            //            {
            //                AmazonS3Client s3 = new AmazonS3Client(awsCredentials, profile.Region);
            //                using (Amazon.S3.Transfer.TransferUtility tranUtility = new Amazon.S3.Transfer.TransferUtility(s3))
            //                {
            //                    await tranUtility.UploadAsync(memStream, bucketname, keyname);
            //                    string message = "Good";
            //                    return new Tuple<bool, string>(true, message);
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                string message = "Error: " + ex.Message; ;
            //                return new Tuple<bool, string>(false, message);
            //            }
            //        }

            //    }
            //    else
            //    {
            //        return new Tuple<bool, string>(false, "Unknown error!");
            //    }
            //}
            //catch (Exception e)
            //{
            //    string message = "Error2: " + e.Message; ;
            //    return new Tuple<bool, string>(false, message);
            //}
            #endregion

            try { 
            AmazonS3Client client2 = new AmazonS3Client(RegionEndpoint.EUWest1);    //credentials, region Irland
                using (Stream stream = new MemoryStream(1024))
                {
                    stream.Write(memstring, 0, memstring.Length);
                    PutObjectRequest request = new PutObjectRequest
                    {
                        BucketName = bucketname, //"auction-predict-bucket",
                        Key = keyname,  //"json.json",
                        InputStream = stream
                    };

                    //b) Put object
                    PutObjectResponse response2 = await client2.PutObjectAsync(request);
                    message = "It's Good!";
                    flag = true;
                }                
            }
            catch(Exception e)
            {
                 message = "Error: " + e.Message; ;
                flag = false;
            }
            return new Tuple<bool, string>(flag, message);
        }
    }




}