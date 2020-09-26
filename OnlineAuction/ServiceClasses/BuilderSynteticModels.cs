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
    public class BuilderSynteticModels
    {
        private static Model1 db = new Model1();
        public static IMapper mapper;        

        public BuilderSynteticModels(IMapper mapper)
        {
        }
        public static async Task<AuctionBO> CreateEntity(AuctionEditVM editVM, AuctionBO auction, object userId, HttpPostedFileBase upload)
        {
            ImageBO image = CreateImageEntity(editVM, upload);
            ProductBO product = CreateProductEntity(editVM, image);

            //var client = db.Clients.Where(c => c.AccountId == (int)userId).FirstOrDefault();
            var client = DependencyResolver.Current.GetService<ClientBO>().LoadAll().Where(c => c.AccountId == (int)userId).FirstOrDefault();
            CreateAuctionEntity(editVM, ref auction, product, client);
            CreateBetAuctionEntity(editVM, auction);    //, client
            ////await db.SaveChangesAsync();
            return await Task.FromResult(auction);
        }

        private static void CreateAuctionEntity(AuctionEditVM editVM, ref AuctionBO auction, ProductBO product, ClientBO client)
        {
            auction = mapper.Map<AuctionBO>(editVM);
            if (editVM.Step != null || editVM.Step == 0) {
                auction.Step = (decimal)editVM.Step;
            }
            else {
                auction.Step = editVM.Price * (decimal)0.1;
            }
            auction.Product = product;
            //auction.RedemptionPrice = editVM.RedemptionPrice == 0 ? auction.Product.Price * 3 : editVM.RedemptionPrice;
            //auction.Description = editVM.Description;
            //auction.BeginTime = editVM.DayBegin + editVM.TimeBegin;
            //auction.EndTime = auction.BeginTime + TimeSpan.FromHours((int)editVM.Duration);
            auction.ActorId = (int)client.Id;
            auction.WinnerId = (int)client.Id; // в начале! - потом перезапишется
            auction.IsActive = true;
            ////db.Auctions.Add(auction);
            ////auction.Save(auction);
        }

        private static void CreateBetAuctionEntity(AuctionEditVM editVM, AuctionBO auction)    //, ClientBO client
        {
            //BetAuction betAuction = new BetAuction { Auction = auction, Bet = editVM.Price, Client = client };
            var betAuction = DependencyResolver.Current.GetService<BetAuctionBO>();
            betAuction.Auction = auction;
            betAuction.Bet = editVM.Price;
            betAuction.ClientId = auction.ActorId;
            //db.BetAuction.Add(betAuction);
            betAuction.Save(betAuction);
        }

        private static ProductBO CreateProductEntity(AuctionEditVM editVM, ImageBO image)
        {
            //Product product = new Product { Image = image, Price = editVM.Price, Title = editVM.Title };
            var product = DependencyResolver.Current.GetService<ProductBO>();
            product.Image = image;
            product.Price = editVM.Price;
            product.Title = editVM.Title;
            ////db.Products.Add(product);
            //product.Save(product);
            return product;
        }

        //Create->flag:false, Edit->flag:true
        private static ImageBO CreateImageEntity(AuctionEditVM editVM, HttpPostedFileBase upload, bool flag = false)
        {
            byte[] myBytes = new byte[upload.ContentLength];
            upload.InputStream.Read(myBytes, 0, upload.ContentLength);
            //Image image = new Image { FileName = editVM.Title, ImageData = myBytes };
            var image = DependencyResolver.Current.GetService<ImageBO>();
            image.FileName = editVM.Title;
            image.ImageData = myBytes;
            
            //if (flag == false) {
            //    //db.Images.Add(image);
            //    image.Save(image);
            //}
            return image;
        }


        //------------------- E.D.I.T.--------------------------------------------
        public static async Task EditEntityAsync(ISyntetic editVM, BaseBusinessObject businessObject, HttpPostedFileBase upload=null)//AuctionEditVM editVM, AuctionBO auctionBO
        {
            if (businessObject is AuctionBO auctionBO) {
                editVM = (AuctionEditVM)editVM;

                var editBO = mapper.Map<AuctionBO>(editVM); //1)из формы    
                EditEntity(auctionBO, editBO, 0);   //-> listTypes[0]   //
                  //2)Product-----------------
                ProductBO productBO = DependencyResolver.Current.GetService<ProductBO>();
                productBO = productBO.LoadAll().FirstOrDefault(p => p.Title == ((AuctionEditVM)editVM).Title);
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
                        //Image image = CreateImageEntity((AuctionEditVM)editVM, upload, true);    //true->edit
                        ImageBO editImageBO = DependencyResolver.Current.GetService<ImageBO>();
                        //ImageBO editImageBO = mapper.Map<ImageBO>(image);
                        editImageBO = CreateImageEntity((AuctionEditVM)editVM, upload, true);    //true->edit
                        EditEntity(imageBO, editImageBO, 3);
                        imageBO.Save(imageBO);

                        //при редактир. нет новой записи ->переустанова ссылок не треб.
                        //productBO.ImageId = newImageBO.Id;
                        productBO.Image = imageBO;
                    }
                }
                productBO.Save(productBO);
                auctionBO.Product = productBO;
                auctionBO.Save(auctionBO);
            }
            else if (businessObject is OrderBO orderBO) {
                editVM = (OrderFullMapVM)editVM;
                OrderVM orderVM = mapper.Map<OrderVM>(editVM);
                OrderBO editBO = mapper.Map<OrderBO>(orderVM);
                EditEntity(orderBO, editBO, 5);
                //orderBO.Save(orderBO);
            }


        }
        private static List<Type> listTypes = new List<Type>()
        { typeof(AuctionBO), typeof(ProductBO), typeof(ClientBO), typeof(ImageBO), typeof(BetAuctionBO) , typeof(OrderBO), typeof(ItemBO)};
        private static List<string> simpleList = new List<string>() { "int32", "nullable", "decimal", "float", "string", "byte[]", "double", "bool" };

        private static void EditEntity(BaseBusinessObject modelBO, BaseBusinessObject editBO, int key)
        {
            foreach (PropertyInfo prop in listTypes[key].GetProperties()) {
                try {
                    if (!listTypes.Contains(prop.PropertyType)) {
                        if (simpleList.Where(s=>prop.PropertyType.Name.ToLower().Contains(s)).Count() != 0) {
                            if (prop.PropertyType.Name.ToLower() == "string" && prop.GetValue(editBO) != "") {
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                            else if ((prop.PropertyType.Name.ToLower().Contains("int") || prop.PropertyType.Name.ToLower().Contains("nullable")) && (int)prop.GetValue(editBO) != 0) {
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
                            else if (prop.PropertyType.Name.ToLower().Contains("bool") && (bool)prop.GetValue(editBO) != null) {  //decimal 
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                        }
                    }
                    if (prop.PropertyType.Name.Contains("DateTime") || prop.PropertyType.Name.Contains("TimeSpan")) {
                        prop.SetValue(modelBO, prop.GetValue(editBO));
                        System.Diagnostics.Debug.WriteLine(prop.PropertyType.Name + ": " + prop.GetValue(editBO));
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