namespace OnlineAuction.Entities
{
    using DataLayer.Entities;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity;
    using System.IO;

    public partial class Model1 : DbContext
    {
        public Model1() : base("name=Model1")
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

    public class MyContextInitializer : DropCreateDatabaseIfModelChanges<Model1>
    {
        private string connectName = "blobContainer";
        private string uripath = "https://storageauction.blob.core.windows.net/blobcontainer";
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
        protected override void Seed(Model1 context)
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
                    using (var filestream = File.Open(item.FullName, FileMode.Open))
                    {
                        UploadFile(filestream, connectName, blobContainerName);
                    }
                }
            }
            //сохр. в БД дефолт. изобр. юзера и товара
            foreach (var filename in filenames)
            {
                string uriStr = Path.Combine(uripath, filename);
                Image image = new Image { FileName = filename, URI = uriStr };
                context.Images.Add(image);
            }

            //3)Account
            Address address = new Address { Region = "Akmola", City = "Nur-Sultan", Street = "Imanova", House = "22" };
            context.Addresses.Add(address);

            var defaultImageUser = context.Images.FirstOrDefaultAsync(i => i.FileName.Contains("men"));

            Account account = new Account
            {
                FullName = "admin",
                Email = "admin@mail.ru",
                Password = "admin",
                Address = address,
                ImageId = defaultImageUser.Id,
                Age = 0,
                CreateAt = DateTime.Parse("2017-01-01 12:00:00")
            };
            context.Account.Add(account);

            //4)Categories
            List<Category> categories = new List<Category> {
                new Category {
                    Title = "electronic" }, new Category { Title = "book" }, new Category { Title = "DVD" }, new Category { Title = "Digital product" }
            };
            context.Categories.AddRange(categories);

            context.SaveChanges();
            base.Seed(context);
        }
    }
}
