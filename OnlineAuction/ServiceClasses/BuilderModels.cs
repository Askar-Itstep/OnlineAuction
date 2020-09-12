using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using OnlineAuction.Entities;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OnlineAuction.ServiceClasses
{
    public class BuilderModels
    {

        private static Model1 db = new Model1();
        public static IMapper mapper;

        public BuilderModels(IMapper mapper)
        {
            //this.mapper = mapper;
        }
        public static async Task CreateEntity(AuctionEditVM editVM, Auction auction, object userId, HttpPostedFileBase upload)
        {
            Image image = CreateImageEntity(editVM, upload);
            Product product = CreateProductEntity(editVM, image);

            var client = db.Clients.Where(c => c.AccountId == (int)userId).FirstOrDefault();
            CreateAuctionEntity(editVM, auction, product, client);
            CreateBetAuctionEntity(editVM, auction, client);
            await db.SaveChangesAsync();
        }

        private static void CreateAuctionEntity(AuctionEditVM editVM, Auction auction, Product product, Client client)
        {
            auction.ActorId = (int)client.Id;
            auction.Product = product;
            if (editVM.Step != null || editVM.Step == 0) {
                auction.Step = (decimal)editVM.Step;
            }
            else {
                auction.Step = editVM.Price * (decimal)0.1;
            }
            auction.RedemptionPrice = editVM.RedemptionPrice == 0 ? auction.Product.Price * 3 : editVM.RedemptionPrice;
            auction.Description = editVM.Description;
            auction.BeginTime = editVM.DayBegin + editVM.TimeBegin;
            auction.EndTime = auction.BeginTime + TimeSpan.FromHours((int)editVM.Duration);
            auction.WinnerId = (int)client.Id; // в начале! - потом перезапишется
            db.Auctions.Add(auction);
        }

        private static void CreateBetAuctionEntity(AuctionEditVM editVM, Auction auction, Client client)
        {
            BetAuction betAuction = new BetAuction { Auction = auction, Bet = editVM.Price, Client = client };
            db.BetAuction.Add(betAuction);
        }

        private static Product CreateProductEntity(AuctionEditVM editVM, Image image)
        {
            Product product = new Product { Image = image, Price = editVM.Price, Title = editVM.Title };
            db.Products.Add(product);
            return product;
        }

        //Create->flag:false, Edit->flag:true
        private static Image CreateImageEntity(AuctionEditVM editVM, HttpPostedFileBase upload, bool flag = false)
        {
            //byte[] myBytes = new byte[editVM.Upload.ContentLength];
            //editVM.Upload.InputStream.Read(myBytes, 0, editVM.Upload.ContentLength);
            //Image image = new Image { FileName = editVM.Title, ImageData = myBytes };

            byte[] myBytes = new byte[upload.ContentLength];
            upload.InputStream.Read(myBytes, 0, upload.ContentLength);
            Image image = new Image { FileName = editVM.Title, ImageData = myBytes };
            if (flag == false) {
                db.Images.Add(image);
            }
            return image;
        }


        //------------------- E.D.I.T.--------------------------------------------
        public static async Task EditEntityAsync(AuctionEditVM editVM, AuctionBO auctionBO, object userId, HttpPostedFileBase upload)//
        {
            var editBO = mapper.Map<AuctionBO>(editVM); //1)из формы    
            EditEntity(auctionBO, editBO, 0);

            //2)Product-----------------
            ProductBO productBO = DependencyResolver.Current.GetService<ProductBO>();
            productBO = productBO.LoadAll().FirstOrDefault(p => p.Title == editVM.Title);
            ProductBO productEdit = mapper.Map<ProductBO>(editVM);
            EditEntity(productBO, productEdit, 1);

            //3)BetAuction----------
            BetAuctionBO betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();
            betAuctionBO = betAuctionBO.LoadAll().Where(b => b.AuctionId == auctionBO.Id && b.ClientId == auctionBO.ActorId).FirstOrDefault();
            BetAuctionBO betEdit = mapper.Map<BetAuctionBO>(editVM);
            EditEntity(betAuctionBO, betEdit, 4);

            //4)Image----------------
            ImageBO imageBO = null;
            if (upload != null) {
                imageBO = DependencyResolver.Current.GetService<ImageBO>();
                imageBO = imageBO.LoadAll().FirstOrDefault(i => i.Id == auctionBO.Product.ImageId);
                if (imageBO != null) {
                    Image image = CreateImageEntity(editVM, upload, true);    //true->edit
                    ImageBO editImageBO = mapper.Map<ImageBO>(image);
                    EditEntity(imageBO, editImageBO, 3);
                    imageBO.Save(imageBO);

                    //4)~recurs           //при редактир. нет новой записи ->переустанов. ссылок не треб.
                    //productBO.ImageId = newImageBO.Id;
                    productBO.Image = imageBO;
                }
            }
            productBO.Save(productBO);
            auctionBO.Product = productBO;
            auctionBO.Save(auctionBO);
        }
        private static List<Type> listTypes = new List<Type>() { typeof(AuctionBO), typeof(ProductBO), typeof(ClientBO), typeof(ImageBO), typeof(BetAuctionBO) };
        private static List<string> simpleList = new List<string>() { "int32", "decimal", "float", "string", "byte[]", "double" };

        private static void EditEntity(BaseBusinessObject modelBO, BaseBusinessObject editBO, int key)
        {
            foreach (PropertyInfo prop in listTypes[key].GetProperties()) {
                //foreach (var type in listTypes) {
                try {
                    if (!listTypes.Contains(prop.PropertyType)) {
                        if (simpleList.Contains(prop.PropertyType.Name.ToLower())) {    //"int", "decimal", "float", "string" 
                            if (prop.PropertyType.Name.ToLower() == "string" && prop.GetValue(editBO) != "") {
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                            else if (prop.PropertyType.Name.ToLower().Contains("int") && (int)prop.GetValue(editBO) != 0) {
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                            else if (prop.PropertyType.Name.ToLower().Contains("decimal") && (decimal)prop.GetValue(editBO) != 0) {  //decimal 
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                            else if (prop.PropertyType.Name.ToLower().Contains("float") && (float)prop.GetValue(editBO) != 0) {  //decimal 
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                            else if (prop.PropertyType.Name.ToLower().Contains("byte[]") && (byte[])prop.GetValue(editBO) != null) {  //decimal 
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                        }
                    }
                    if (prop.PropertyType.Name.Contains("DateTime") || prop.PropertyType.Name.Contains("TimeSpan")) {
                        prop.SetValue(modelBO, prop.GetValue(editBO));
                        //System.Diagnostics.Debug.WriteLine(prop.PropertyType.Name + ": " + prop.GetValue(editBO));
                    }
                }
                catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
                //}
            }

        }
    }
}