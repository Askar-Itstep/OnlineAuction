using AutoMapper;
using DataLayer.Entities;
using OnlineAuction.Entities;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OnlineAuction.ServiceClasses
{
    public class BuilderModels
    {

        private static Model1 db = new Model1();
        private IMapper mapper;

        public BuilderModels(IMapper mapper)
        {
            this.mapper = mapper;
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
        private static void CreateBetAuctionEntity(AuctionEditVM editVM, Auction auction, Client client)
        {
            BetAuction betAuction = new BetAuction { Auction = auction, Bet = editVM.Price, Client = client };
            db.BetAuction.Add(betAuction);
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

        private static Product CreateProductEntity(AuctionEditVM editVM, Image image)
        {
            Product product = new Product { Image = image, Price = editVM.Price, Title = editVM.Title };
            db.Products.Add(product);
            return product;
        }

        private static Image CreateImageEntity(AuctionEditVM editVM, HttpPostedFileBase upload)
        {
            //byte[] myBytes = new byte[editVM.Upload.ContentLength];
            //editVM.Upload.InputStream.Read(myBytes, 0, editVM.Upload.ContentLength);
            //Image image = new Image { FileName = editVM.Title, ImageData = myBytes };

            byte[] myBytes = new byte[upload.ContentLength];
            upload.InputStream.Read(myBytes, 0, upload.ContentLength);
            Image image = new Image { FileName = editVM.Title, ImageData = myBytes };

            db.Images.Add(image);
            return image;
        }

        public static Task EditEntity(AuctionEditVM editVM, Auction auction, object userId, HttpPostedFileBase upload)
        {
            throw new NotImplementedException();
        }
    }
}