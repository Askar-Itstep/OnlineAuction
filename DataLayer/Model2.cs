using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using DataLayer.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;

namespace DataLayer
{
    public partial class Model2 : DbContext
    {
        public Model2() : base("name=Model2")
        {
        }

        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<ImageProductLink> ImageProductLinks { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Auction> Auctions { get; set; }
        public virtual DbSet<AuctionClientsLink> AuctionClientsLinks { get; set; }
        public virtual DbSet<BetAuction> BetAuction { get; set; }

        public virtual DbSet<RoleAccountLink> RoleAccountLinks { get; set; }


        //--------SignalR-----------------
        public virtual DbSet<UserHub> UserHubs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }

    public class MyContextInitializer : DropCreateDatabaseIfModelChanges<Model2>
    {
        private string uripath = "https://for-dotnet.s3-us-west-2.amazonaws.com/Files/";
        public static bool UploadFile(string bucket, string key, string filepath)
        {
            try
            {
                var options = new CredentialProfileOptions
                {
                    AccessKey = "AKIA4LDGGJIMCE3BJE3C",
                    SecretKey = "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"
                };
                var profile = new CredentialProfile("dotnet-tutorials", options);
                profile.Region = RegionEndpoint.USWest2;

                var sharedFile = new SharedCredentialsFile(); //тоже самое
                sharedFile.RegisterProfile(profile);

                AWSCredentials awsCredentials;
                if (sharedFile.TryGetProfile("dotnet-tutorials", out profile) &&
                    AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out awsCredentials))
                {
                    using (var client = new AmazonS3Client(awsCredentials, profile.Region))
                    {
                        PutObjectRequest request = new PutObjectRequest
                        {
                            BucketName = bucket,    //"for-dotnet",
                            Key = key,           //"podarok.png",
                            FilePath = filepath //@"C:\Users\Askar\OneDrive\Pictures\ASP.Net\podarok.png"
                        };

                        // Put object
                        PutObjectResponse response2 = client.PutObject(request);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Upload Error: " + ex.Message);
                return false;
            }
        }
        protected override void Seed(Model2 context)
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
            //----------нужно загрузить из папки File: 1-ый файл для лиц; 2-ой для фона---------------
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files"); //../Sportclub/Sportclub/Files
            DirectoryInfo dir = new DirectoryInfo(path);
            List<string> filenames = new List<string>();
            foreach (var item in dir.GetFiles())
            {
                if (item.Name.Contains("default") || item.Name.Contains("men"))
                {
                    filenames.Add(item.Name);
                }
            }
            //сохр. в БД дефолт. изобр. юзера и товара
            foreach (var filename in filenames)
            {
                string uriStr = Path.Combine(uripath, filename);
                UploadFile("for-dotnet", filename, uriStr);
                Image image = new Image { FileName = filename, URI = uriStr };
                context.Images.Add(image);
            }

            //3)Account
            Address address = new Address { Region = "Akmola", City = "Nur-Sultan", Street = "Imanova", House = "22" };
            var defaultImageUser = context.Images.FirstOrDefaultAsync(i => i.FileName.Contains("men"));
            Account account = new Account { FullName = "admin", Email = "admin@mail.ru", Password = "admin", Address = address, ImageId = defaultImageUser.Id };
            context.Account.Add(account);

            //4)Categories
            List<Category> categories = new List<Category> { new Category { Title = "electronic" }, new Category { Title = "book" }, new Category { Title = "DVD" }, new Category { Title = "Digital product" } };
            context.Categories.AddRange(categories);

            context.SaveChanges();
            base.Seed(context);
        }
    }
}
