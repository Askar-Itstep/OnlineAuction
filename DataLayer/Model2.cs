using DataLayer.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace DataLayer
//{
    //    public partial class Model2:DbContext
    //    {
    //        public Model2() : base("name=Model2")
    //        {
    //        }

    //        public virtual DbSet<Role> Roles { get; set; }
    //        public virtual DbSet<Account> Account { get; set; }
    //        public virtual DbSet<Address> Addresses { get; set; }
    //        public virtual DbSet<Client> Clients { get; set; }
    //        public virtual DbSet<Image> Images { get; set; }
    //        public virtual DbSet<Item> Items { get; set; }
    //        public virtual DbSet<Product> Products { get; set; }
    //        public virtual DbSet<Order> Orders { get; set; }
    //        public virtual DbSet<Message> Messages { get; set; }
    //        public virtual DbSet<Auction> Auctions { get; set; }
    //        public virtual DbSet<AuctionClientsLink> AuctionClientsLinks { get; set; }
    //        public virtual DbSet<BetAuction> BetAuction { get; set; }

    //        public virtual DbSet<RoleAccountLink> RoleAccountLinks { get; set; }

    //        //--------SignalR-----------------
    //        public virtual DbSet<UserHub> UserHubs { get; set; }

    //        protected override void OnModelCreating(DbModelBuilder modelBuilder)
    //        {
    //        }
    //    }

    //public class MyContextInitializer : DropCreateDatabaseIfModelChanges<Model2>
    //{
        //private string connectName = "blobContainer";
        //private string uripath = "https://storageauction.blob.core.windows.net/blobcontainer";
        //private string blobContainerName = "blobcontainer";    //именя контейнеров в Azure
        //public static bool UploadFile(FileStream fileStream, string connectName, string boxName)
        //{
        //    try
        //    {
                //string filename = fileStream.Name;
                //string storagekey = ConfigurationManager.ConnectionStrings[connectName].ConnectionString;
                //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storagekey);

                //CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                //CloudBlobContainer container = blobClient.GetContainerReference(boxName);

                //container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                //CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);

                //blockBlob.UploadFromStreamAsync(fileStream);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Upload Error: " + ex.Message);
        //        return false;
        //    }
        //}
        //protected override void Seed(Model2 context)
        //{
        //    //1) Roles
        //    Role adminRole = new Role { RoleName = "admin" };
        //    Role moderRole = new Role { RoleName = "moder" };
        //    Role memberRole = new Role { RoleName = "member" };
        //    Role clientRole = new Role { RoleName = "client" };

        //    context.Roles.Add(adminRole);
        //    context.Roles.Add(memberRole);
        //    context.Roles.Add(clientRole);

        //    //2) Images            
        //    //----------нужно загрузить из папки File: 1-ый файл для лиц; 2-ой для фона---------------
        //    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files"); //../Sportclub/Sportclub/Files
        //    DirectoryInfo dir = new DirectoryInfo(path);
        //    List<string> filenames = new List<string>();
        //    foreach (var item in dir.GetFiles())
        //    {
        //        if (item.Name.Contains("default") || item.Name.Contains("men"))
        //        {
        //            filenames.Add(item.Name);
        //            using (var filestream = File.Open(item.FullName, FileMode.Open))
        //            {
        //                UploadFile(filestream, connectName, blobContainerName);
        //            }
        //        }
        //    }
        //    //сохр. в БД дефолт. изобр. юзера и товара
//            foreach (var filename in filenames)
//            {
//                string uriStr = Path.Combine(uripath, filename);
//                Image image = new Image { FileName = filename, URI = uriStr };
//                context.Images.Add(image);
//            }

//            //3)Account
//            Address address = new Address { Region = "Akmola", City = "Nur-Sultan", Street = "Imanova", House = "22" };
//            var defaultImageUser = context.Images.FirstOrDefaultAsync(i => i.FileName.Contains("men"));
//            Account account = new Account { FullName = "admin", Email = "admin@mail.ru", Password = "admin", Address = address, ImageId = defaultImageUser.Id };
//            context.Account.Add(account);


//            context.SaveChanges();
//            base.Seed(context);
//        }
//    }
//}
