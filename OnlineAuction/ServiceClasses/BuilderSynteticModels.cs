﻿using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer;
using DataLayer.Entities;
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
        public static IMapper mapper;

        public BuilderSynteticModels(IMapper mapper)
        {
        }
        public static async Task<AuctionBO> CreateEntity(HttpPostedFileBase upload, AuctionBO auction = null, object userId = null, AuctionEditVM editVM = null)
        {
            ImageBO image = await CreateImageEntity(upload, editVM);
            if (editVM != null)
            {
                ProductBO product = CreateProductEntity(editVM, image).Item1;

                var client = DependencyResolver.Current.GetService<ClientBO>().LoadAll().Where(c => c.AccountId == (int)userId).FirstOrDefault();
                CreateAuctionEntity(editVM, ref auction, product, client);
                CreateBetAuctionEntity(editVM, auction);
                return await Task.FromResult(auction);
            }

            return null;
        }

        private static void CreateAuctionEntity(AuctionEditVM editVM, ref AuctionBO auction, ProductBO product, ClientBO client)
        {
            auction = mapper.Map<AuctionBO>(editVM);
            if (editVM.Step != null || editVM.Step == 0)
            {
                auction.Step = (decimal)editVM.Step;
            }
            else
            {
                auction.Step = editVM.Price * (decimal)0.1;
            }
            auction.Product = product;
            auction.ActorId = (int)client.Id;
            auction.WinnerId = (int)client.Id; // в начале! - потом перезапишется
            auction.IsActive = true;
        }

        private static void CreateBetAuctionEntity(AuctionEditVM editVM, AuctionBO auction)
        {
            var betAuction = DependencyResolver.Current.GetService<BetAuctionBO>();
            betAuction.Auction = auction;
            betAuction.Bet = editVM.Price;
            betAuction.ClientId = auction.ActorId;
            betAuction.Save(betAuction);
        }

        private static Tuple<ProductBO, ImageProductLinkBO> CreateProductEntity(AuctionEditVM editVM, ImageBO image)
        {
            var product = DependencyResolver.Current.GetService<ProductBO>();
            product.Image = image;  //потом убрать
            product.Price = editVM.Price;
            product.Title = editVM.Title;
            product.CategoryId = editVM.CategoryId;

            var imageProductLinkBO = DependencyResolver.Current.GetService<ImageProductLinkBO>();
            imageProductLinkBO.Image = image;
            imageProductLinkBO.Product = product;
            return new Tuple<ProductBO, ImageProductLinkBO>(product, imageProductLinkBO);
        }

        //Create->flag:false, Edit->flag:true      
        private static async Task<ImageBO> CreateImageEntity(HttpPostedFileBase upload, ISyntetic editVM = null, bool flag = false, AccountBO account = null)
        {
            ImageVM imageVM = null;
            if (editVM != null)
            {
                imageVM = new ImageVM { FileName = ((AuctionEditVM)editVM).Title };
            }
            else
            {
                imageVM = new ImageVM { FileName = upload.FileName };
            }
            ImageBO imageBO = DependencyResolver.Current.GetService<ImageBO>();

            //пока исп-ся  .DataLayer.MyConfig
            return await BlobHelper.SetImageAsync(upload, imageVM, imageBO, mapper, account, MyConfig.tempConnectString);
        }


        //------------------- E.D.I.T.--------------------------------------------
        public static async Task EditEntityAsync(ISyntetic editVM, BaseBusinessObject businessObject, HttpPostedFileBase upload = null)
        {
            if (businessObject is AuctionBO auctionBO)
            {
                //Auction----------
                editVM = (AuctionEditVM)editVM;
                var editBO = mapper.Map<AuctionBO>(editVM); //1)из формы    
                EditEntity(auctionBO, editBO, 0);   //)Product--> listTypes[0]   

                //Product------------
                ProductBO productBO = DependencyResolver.Current.GetService<ProductBO>();
                productBO = productBO.Load(editBO.ProductId);
                ProductBO productEdit = mapper.Map<ProductBO>(editVM);
                EditEntity(productBO, productEdit, 1);

                //3)BetAuction----------
                BetAuctionBO betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();
                betAuctionBO = betAuctionBO.LoadAll().Where(b => b.AuctionId == auctionBO.Id && b.ClientId == auctionBO.ActorId).FirstOrDefault();
                BetAuctionBO betEdit = mapper.Map<BetAuctionBO>(editVM);
                EditEntity(betAuctionBO, betEdit, 4);

                //4)Image----------------
                ImageBO imageBO = null;
                if (upload != null)
                {
                    imageBO = DependencyResolver.Current.GetService<ImageBO>();
                    imageBO = imageBO.LoadAll().FirstOrDefault(i => i.Id == auctionBO.Product.ImageId);
                    if (imageBO != null)
                    {
                        ImageBO editImageBO = DependencyResolver.Current.GetService<ImageBO>();
                        editImageBO = await CreateImageEntity(upload, (AuctionEditVM)editVM, true);    //true->edit
                        //EditEntity(imageBO, editImageBO, 3);  //теперь доп.изобр. добавл. в пул
                        ImageProductLinkBO imgProductLink = DependencyResolver.Current.GetService<ImageProductLinkBO>();
                        //1)перенос в пул визит. карт. (dbo.Image->new dbo.ImageProductLink)
                        var findImageLink = imgProductLink.LoadAll().FirstOrDefault(i => i.ImageId == imageBO.Id);
                        if (findImageLink is null)
                        {
                            imgProductLink.ImageId = imageBO.Id;
                            imgProductLink.ProductId = productBO.Id;
                            imgProductLink.Save(imgProductLink);
                        }
                        //2)при редактир.-нов.изобр. добвл. в пул
                        imgProductLink.ImageId = editImageBO.Id;
                        imgProductLink.ProductId = productBO.Id;
                        imgProductLink.Save(imgProductLink);
                        //imageBO.Save(imageBO);    //теперь визит.карт.не меняется -переустанова ссылок не треб.
                        //productBO.Image = imageBO;
                    }
                }
                //productBO.Save(productBO);
                auctionBO.Product = productBO;
                auctionBO.IsActive = true;
                auctionBO.Save(auctionBO);
            }
            else if (businessObject is OrderBO orderBO)
            {
                editVM = (OrderFullMapVM)editVM;
                OrderVM orderVM = mapper.Map<OrderVM>(editVM);
                OrderBO editBO = mapper.Map<OrderBO>(orderVM);
                EditEntity(orderBO, editBO, 5);
            }
        }

        private static List<Type> listTypes = new List<Type>()
        { typeof(AuctionBO), typeof(ProductBO), typeof(ClientBO), typeof(ImageBO), typeof(BetAuctionBO) , typeof(OrderBO), typeof(ItemBO)};

        private static List<string> simpleList = new List<string>() { "int32", "nullable", "decimal", "float", "string", "byte[]", "double", "bool", "Uri" };

        //key - ключ соотв. из listTypes
        private static void EditEntity(BaseBusinessObject modelBO, BaseBusinessObject editBO, int key)
        {
            foreach (PropertyInfo prop in listTypes[key].GetProperties())
            {
                try
                {
                    if (!listTypes.Contains(prop.PropertyType))
                    { //выбрать простые типы
                        if (simpleList.Where(s => prop.PropertyType.Name.ToLower().Contains(s)).Count() != 0)
                        {
                            if (prop.PropertyType.Name.ToLower() == "string" && prop.GetValue(editBO) != "")
                            {
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                            else if ((prop.PropertyType.Name.ToLower().Contains("int") || prop.PropertyType.Name.ToLower().Contains("nullable"))
                                && prop.GetValue(editBO) != null && (int)prop.GetValue(editBO) != 0)
                            {
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                            else if (prop.PropertyType.Name.ToLower().Contains("decimal") && (decimal)prop.GetValue(editBO) != 0)
                            {
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                            else if (prop.PropertyType.Name.ToLower().Contains("float") && (float)prop.GetValue(editBO) != 0)
                            {
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                            else if (prop.PropertyType.Name.ToLower().Contains("byte[]") && (byte[])prop.GetValue(editBO) != null)
                            {  //можно удалить
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                            else if (prop.PropertyType.Name.ToLower().Contains("bool") && (bool)prop.GetValue(editBO) != null)
                            {
                                System.Diagnostics.Debug.WriteLine(prop.Name + ": " + prop.GetValue(editBO));
                                prop.SetValue(modelBO, prop.GetValue(editBO));
                            }
                        }
                    }
                    //+Datetime, Timespan
                    if (prop.PropertyType.Name.Contains("DateTime") || prop.PropertyType.Name.Contains("TimeSpan"))
                    {
                        prop.SetValue(modelBO, prop.GetValue(editBO));
                        System.Diagnostics.Debug.WriteLine(prop.PropertyType.Name + ": " + prop.GetValue(editBO));
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }
    }
}