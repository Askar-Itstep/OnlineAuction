﻿using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using OnlineAuction.ServiceClasses;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Unity;

namespace OnlineAuction
{
    internal class AutoMapperConfig
    {
        public static void RegisterWithUnity(UnityContainer container)
        {
            IMapper mapper = CreateMapperConfiguration().CreateMapper();
            container.RegisterInstance(mapper);
        }

        private static MapperConfiguration CreateMapperConfiguration()
        {
            return new MapperConfiguration(mpr =>
            {
                mpr.CreateMap<Account, AccountBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<AccountBO>()).ReverseMap();

                mpr.CreateMap<AccountBO, AccountVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<AccountVM>()).ReverseMap();

                mpr.CreateMap<Account, AccountVM>()
             .ConstructUsing(c => DependencyResolver.Current.GetService<AccountVM>()).ReverseMap();
                //.ForMember(dist=>dist.ConnectionId, opt=>opt.MapFrom(src=>0));

                //-------------------RegisterVM ------------------------------
                mpr.CreateMap<RegisterModel, AccountBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<AccountBO>())
                .ForMember(dist => dist.Address, opt => opt.MapFrom(src => new AddressVM { City = src.City, Region = src.Region, Street = src.Street, House = src.House }))
                .ForMember(dist => dist.Balance, opt => opt.MapFrom(src => 0))
                .ForMember(dist => dist.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dist => dist.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dist => dist.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dist => dist.Gender, opt => opt.MapFrom(src => src.Gender))
                ;


                //---------------Role ----------------------------------
                mpr.CreateMap<Role, RoleBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<RoleBO>()).ReverseMap();

                mpr.CreateMap<RoleBO, RoleVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<RoleVM>()).ReverseMap();

                mpr.CreateMap<RoleVM, Role>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Role>()).ReverseMap();

                //------------------Client ----------------------------------
                mpr.CreateMap<Client, ClientBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<ClientBO>()).ReverseMap();

                mpr.CreateMap<ClientBO, ClientVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<ClientVM>()).ReverseMap();

                mpr.CreateMap<ClientVM, Client>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Client>()).ReverseMap();

                //-----------------------RoleAccountLink---------------------------
                mpr.CreateMap<RoleAccountLink, RoleAccountLinkBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<RoleAccountLinkBO>()).ReverseMap();

                mpr.CreateMap<RoleAccountLinkBO, RoleAccountLinkVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<RoleAccountLinkVM>()).ReverseMap();

                mpr.CreateMap<RoleAccountLink, RoleAccountLinkVM>()
              .ConstructUsing(c => DependencyResolver.Current.GetService<RoleAccountLinkVM>()).ReverseMap();

                //--------------------------------------Auctions ---------------------------------------
                mpr.CreateMap<Auction, AuctionBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<AuctionBO>()).ReverseMap();

                mpr.CreateMap<AuctionBO, AuctionVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<AuctionVM>()).ReverseMap();

                mpr.CreateMap<AuctionVM, Auction>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Auction>()).ReverseMap();
                //--------------------------------------Category ---------------------------------------
                mpr.CreateMap<Category, CategoryBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<CategoryBO>()).ReverseMap();

                mpr.CreateMap<CategoryBO, CategoryVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<CategoryVM>()).ReverseMap();

                mpr.CreateMap<CategoryVM, Category>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Category>()).ReverseMap();

                //--------------------------------------Product ---------------------------------------
                mpr.CreateMap<Product, ProductBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<ProductBO>()).ReverseMap();

                mpr.CreateMap<ProductBO, ProductVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<ProductVM>()).ReverseMap();

                mpr.CreateMap<ProductVM, Product>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Product>()).ReverseMap();

                //--------------------------------------Image -------------------------------------------
                mpr.CreateMap<Image, ImageBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<ImageBO>()).ReverseMap();

                mpr.CreateMap<ImageBO, ImageVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<ImageVM>()).ReverseMap().ReverseMap();

                mpr.CreateMap<ImageVM, Image>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Image>()).ReverseMap().ReverseMap();
                //---------------------Addon!-----------------------------
                mpr.CreateMap<string, Uri>().ConvertUsing<StringToUriConverter>();
                mpr.CreateMap<Uri, string>().ConvertUsing<UriToStringConverter>();


                //--------------------------------------ImageProductLink -------------------------------------------
                mpr.CreateMap<ImageProductLink, ImageProductLinkBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<ImageProductLinkBO>()).ReverseMap();

                mpr.CreateMap<ImageProductLinkBO, ImageProductLinkVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<ImageProductLinkVM>()).ReverseMap();

                mpr.CreateMap<ImageProductLinkVM, ImageProductLink>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<ImageProductLink>()).ReverseMap();
                //--------------------------------------Address ------------------------------------------
                mpr.CreateMap<Address, AddressBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<AddressBO>()).ReverseMap();

                mpr.CreateMap<AddressBO, AddressVM>()
              .ConstructUsing(c => DependencyResolver.Current.GetService<AddressVM>()).ReverseMap();

                mpr.CreateMap<Address, AddressVM>()
                 .ConstructUsing(c => DependencyResolver.Current.GetService<AddressVM>()).ReverseMap();
                //--------------------------------------BetAuctions ---------------------------------------
                mpr.CreateMap<BetAuction, BetAuctionBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<BetAuctionBO>()).ReverseMap();

                mpr.CreateMap<BetAuctionBO, BetAuctionVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<BetAuctionVM>()).ReverseMap();

                mpr.CreateMap<BetAuctionVM, BetAuction>()
                 .ConstructUsing(c => DependencyResolver.Current.GetService<BetAuction>()).ReverseMap();

                //==========================AuctionEditVM==================================================
                //AuctionEdit, AuctionEditBO- таких типов нет

                 mpr.CreateMap<AuctionBO, AuctionEditVM>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<AuctionEditVM>())
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Product.Title))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.DayBegin, opt => opt.MapFrom(src => src.BeginTime))
                .ForMember(dest => dest.TimeBegin, opt => opt.MapFrom(src => new TimeSpan(src.BeginTime.Hour, src.BeginTime.Minute, src.BeginTime.Second)))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.EndTime - src.BeginTime));

                mpr.CreateMap<AuctionEditVM, AuctionBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<AuctionBO>())
                //.ForMember(dest => dest.RedemptionPrice, opt => opt.MapFrom(src => src.RedemptionPrice))
                .ForMember(dest => dest.BeginTime, opt => opt.MapFrom(src => src.DayBegin + src.TimeBegin))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.DayBegin + src.TimeBegin + TimeSpan.FromHours(src.Duration)));

                //----------------------
                mpr.CreateMap<AuctionEditVM, ProductBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<ProductBO>())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

                //----------------------
                mpr.CreateMap<AuctionEditVM, BetAuctionBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<BetAuctionBO>())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.AuctionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Bet, opt => opt.MapFrom(src => src.Price));
                //============================================

                //--------------------------------------Message ---------------------------------------
                mpr.CreateMap<Message, MessageBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<MessageBO>()).ReverseMap();

                //--------------------------------------Orders ---------------------------------------
                mpr.CreateMap<Order, OrderBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<OrderBO>()).ReverseMap();

                mpr.CreateMap<OrderVM, OrderBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<OrderBO>()).ReverseMap();

              //  mpr.CreateMap<OrderBO, OrderVM>()
              //.ConstructUsing(c => DependencyResolver.Current.GetService<OrderVM>());//.ReverseMap();

                mpr.CreateMap<Order, OrderVM>()
                 .ConstructUsing(c => DependencyResolver.Current.GetService<OrderVM>()).ReverseMap();

                //--------------------------------------Items ---------------------------------------
                mpr.CreateMap<Item, ItemBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<ItemBO>()).ReverseMap();

                mpr.CreateMap<ItemBO, ItemVM>() .ConstructUsing(c => DependencyResolver.Current.GetService<ItemVM>()).ReverseMap();

                mpr.CreateMap<Item, ItemVM>()
                 .ConstructUsing(c => DependencyResolver.Current.GetService<ItemVM>()).ReverseMap();


                //==========================OrderFullMapVM==================================================
                //OrderFullMap, OrderFullMapBO - таких типов нет!

                mpr.CreateMap<OrderVM, OrderFullMapVM>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<OrderFullMapVM>())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                //.ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))    //зачем?
                //.ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client)) 
                //.ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => src.IsApproved))
                .ForMember(dest => dest.AuctionIds, opt => opt.MapFrom(src => new List<int>()))
                .ForMember(dest => dest.ProductIds, opt => opt.MapFrom(src => new List<int>()))
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Items.Select(s => s.Product)))
                .ForMember(dest => dest.EndTimes, opt => opt.MapFrom(src => new List<DateTime>()));

                mpr.CreateMap<OrderFullMapVM, OrderVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<OrderVM>())
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.GetItems()))
               //.ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))    //зачем?
               //.ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client)) 
               //.ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => src.IsApproved))
               ;
                //-------------
                mpr.CreateMap<AuctionVM, OrderFullMapVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<OrderFullMapVM>())
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => 0))
               .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.WinnerId))
               .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Winner))
               .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false))
               .ForMember(dest => dest.AuctionIds, opt => opt.MapFrom(src => new List<AuctionVM>() { new AuctionVM { Id = src.Id } }))
               .ForMember(dest => dest.EndTimes, opt => opt.MapFrom(src => new List<DateTime>() { src.EndTime }))
               .ForMember(dest => dest.ProductIds, opt => opt.MapFrom(src => new List<int>() { src.ProductId }))
               .ForMember(dest => dest.Products, opt => opt.MapFrom(src => new List<ProductVM>() { src.Product }));

                mpr.CreateMap<OrderFullMapVM, AuctionVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<AuctionVM>())
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AuctionIds[0]))
               .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => 0))
               .ForMember(dest => dest.Actor, opt => opt.MapFrom(src => new ClientVM()))
               .ForMember(dest => dest.BeginTime, opt => opt.MapFrom(src => new DateTime()))
               .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTimes.ToList()[0]))
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductIds[0]))
               .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Products.ToList()[0]))
               .ForMember(dest => dest.Step, opt => opt.MapFrom(src => 0))
               .ForMember(dest => dest.RedemptionPrice, opt => opt.MapFrom(src => 0))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => "")) //src.Products.ToList()[0].Title
               .ForMember(dest => dest.BetAuctions, opt => opt.MapFrom(src => new List<BetAuctionVM>() { new BetAuctionVM() }))
               .ForMember(dest => dest.WinnerId, opt => opt.MapFrom(src => src.ClientId))
               .ForMember(dest => dest.Winner, opt => opt.MapFrom(src => src.Client));

                //======================= UserHub ========================================
                mpr.CreateMap<UserHub, UserHubBO>()
                               .ConstructUsing(c => DependencyResolver.Current.GetService<UserHubBO>()).ReverseMap();

                mpr.CreateMap<UserHubBO, UserHubVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<UserHubVM>()).ReverseMap();

                mpr.CreateMap<UserHub, UserHubVM>()
                 .ConstructUsing(c => DependencyResolver.Current.GetService<UserHubVM>()).ReverseMap();

            });
        }
    }
}