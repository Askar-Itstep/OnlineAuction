using DataLayer.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Configuration;

namespace DataLayer
{
    public class MyContextInitializer : CreateDatabaseIfNotExists<Model1>
    //DropCreateDatabaseIfModelChanges<Model1>
    {
        private string connectName = "blobContainer";
        private string uripath = MyConfig.azureUrl;  // WebConfigurationManager.AppSettings["awsUrl"];//["azureUrl"];
        private string blobContainerName = "blobcontainer";    //именя контейнеров в Azure


        public static bool UploadFile(FileStream fileStream, string connectName, string boxName)
        {
            try
            {
                string filename = fileStream.Name;
                string storagekey = ConfigurationManager.ConnectionStrings[connectName].ConnectionString;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storagekey);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(boxName);

                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);

                blockBlob.UploadFromStreamAsync(fileStream);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Upload Error: " + ex.Message);
                return false;
            }
        }
        protected override async void Seed(Model1 context)
        {
            //1) Roles
            Role adminRole = new Role { RoleName = "admin" };
            Role moderRole = new Role { RoleName = "moder" };
            Role memberRole = new Role { RoleName = "member" };
            Role clientRole = new Role { RoleName = "client" };

            context.Roles.Add(adminRole);
            context.Roles.Add(memberRole);
            context.Roles.Add(clientRole);

            //2) Images            
            //----------нужно загрузить из папки File: 
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");
            DirectoryInfo dir = new DirectoryInfo(path);
            List<string> filenames = new List<string>();
            foreach (var item in dir.GetFiles())
            {
                if (item.Name.Contains("default") || item.Name.Contains("men"))
                {
                    filenames.Add(item.Name);
                    using (var filestream = File.Open(item.FullName, FileMode.Open))
                    {
                        UploadFile(filestream, connectName, blobContainerName);
                    }
                }
            }
            //сохр. в БД дефолт. изобр. юзера и товара
            List<Image> images = new List<Image>();
            foreach (var filename in filenames)
            {
                string uriStr = Path.Combine(uripath, filename);
                Image image = new Image { FileName = filename, URI = uriStr };
                context.Images.Add(image);
                images.Add(image);
            }

            //3)Account
            Address address = new Address { Region = "Akmola", City = "Nur-Sultan", Street = "Imanova", House = "22" };
            context.Addresses.Add(address);

            //var defaultImageUser = context.Images.FirstOrDefaultAsync(i => i.FileName.Contains("men"));

            Account account = new Account
            {
                FullName = "admin",
                Email = "admin@mail.ru",
                Password = "admin",
                Address = address,
                Image = images.FirstOrDefault(i=>i.FileName=="men"),
                Age = 0,
                CreateAt = DateTime.Parse("2017-01-01 12:00:00"),
                RemoveAt = DateTime.Parse("3000-01-01 12:00:00"),
            };
            context.Account.Add(account);

            //4)Categories
            List<Category> categories = new List<Category> {
                new Category {
                    Title = "electronic" }, new Category { Title = "book" }, new Category { Title = "DVD" }, new Category { Title = "Digital product" }
            };
            context.Categories.AddRange(categories);

            //5)Role-Account
            RoleAccountLink roleAccount = new RoleAccountLink { Account = account, Role = adminRole };
            context.RoleAccountLinks.Add(roleAccount);

            await context.SaveChangesAsync();
            base.Seed(context);
        }
    }
}
