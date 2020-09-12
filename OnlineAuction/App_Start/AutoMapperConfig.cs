using System;
using System.Web.Mvc;
using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using OnlineAuction.ViewModels;
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
                .ConstructUsing(c => DependencyResolver.Current.GetService<AccountBO>());

                mpr.CreateMap<AccountBO, Account>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Account>());

                mpr.CreateMap<AccountVM, AccountBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<AccountBO>());

                mpr.CreateMap<AccountBO, AccountVM>()
             .ConstructUsing(c => DependencyResolver.Current.GetService<AccountVM>());

                //---------------Role ----------------------------------
                mpr.CreateMap<Role, RoleBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<RoleBO>());

                mpr.CreateMap<RoleBO, Role>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Role>());

                // mpr.CreateMap<RoleVM, Role>()
                //.ConstructUsing(c => DependencyResolver.Current.GetService<Role>());

   //------------------Client ----------------------------------
                mpr.CreateMap<Client, ClientBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<ClientBO>());

                mpr.CreateMap<ClientBO, Client>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Client>());

                mpr.CreateMap<ClientBO, ClientVM>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<ClientVM>());

                mpr.CreateMap<ClientVM, ClientBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<ClientBO>());

                //-----------------------RoleAccountLink---------------------------
                mpr.CreateMap<RoleAccountLink, RoleAccountLinkBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<RoleAccountLinkBO>());

                mpr.CreateMap<RoleAccountLinkBO, RoleAccountLink>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<RoleAccountLink>());

                // mpr.CreateMap<RoleAccountLinkVM, RoleAccountLink>()
                //.ConstructUsing(c => DependencyResolver.Current.GetService<Role>());

    //--------------------------------------Auctions ---------------------------------------
                mpr.CreateMap<Auction, AuctionBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<AuctionBO>());

                mpr.CreateMap<AuctionBO, Auction>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Auction>());

                mpr.CreateMap<AuctionBO, AuctionVM>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<AuctionVM>());

                mpr.CreateMap<AuctionVM, AuctionBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<AuctionBO>());

      //--------------------------------------Product ---------------------------------------
                mpr.CreateMap<Product, ProductBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<ProductBO>());

                mpr.CreateMap<ProductBO, Product>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Product>());

                mpr.CreateMap<ProductVM, ProductBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<ProductBO>()).ReverseMap();

      //--------------------------------------Image -------------------------------------------
                mpr.CreateMap<Image, ImageBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<ImageBO>());

                mpr.CreateMap<ImageBO, Image>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Image>());

                mpr.CreateMap<ImageVM, ImageBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<ImageBO>()).ReverseMap();

      //--------------------------------------Address ------------------------------------------
                mpr.CreateMap<Address, AddressBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<AddressBO>());

                mpr.CreateMap<AddressBO, Address>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Address>());

                mpr.CreateMap<AddressVM, AddressBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<AddressBO>());

                mpr.CreateMap<AddressBO, AddressVM>()
                 .ConstructUsing(c => DependencyResolver.Current.GetService<AddressVM>());
      //--------------------------------------BetAuctions ---------------------------------------
                mpr.CreateMap<BetAuction, BetAuctionBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<BetAuctionBO>());

                mpr.CreateMap<BetAuctionBO, BetAuction>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<BetAuction>());

                mpr.CreateMap<BetAuctionVM, BetAuctionBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<BetAuctionBO>());

                mpr.CreateMap<BetAuctionBO, BetAuctionVM>()
                 .ConstructUsing(c => DependencyResolver.Current.GetService<BetAuctionVM>());

                //==========================AuctionEditVM==================================================

                mpr.CreateMap<AuctionBO, AuctionEditVM>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<AuctionEditVM>())
                .ForMember(dest=>dest.Title, opt=>opt.MapFrom(src=>src.Product.Title))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.DayBegin, opt => opt.MapFrom(src => src.BeginTime))
                .ForMember(dest => dest.TimeBegin, opt => opt.MapFrom(src => new TimeSpan(src.BeginTime.Hour, src.BeginTime.Minute, src.BeginTime.Second)))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.EndTime - src.BeginTime));

                mpr.CreateMap<AuctionEditVM, AuctionBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<AuctionBO>())
                //.ForMember(dest => dest.RedemptionPrice, opt => opt.MapFrom(src => src.RedemptionPrice))
                .ForMember(dest => dest.BeginTime, opt => opt.MapFrom(src => src.DayBegin + src.TimeBegin))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.DayBegin+src.TimeBegin + TimeSpan.FromHours(src.Duration)));

                //----------------------
                mpr.CreateMap<AuctionEditVM, ProductBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<ProductBO>())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

                //----------------------
                mpr.CreateMap<AuctionEditVM, BetAuctionBO>()
                .ConstructUsing(c => DependencyResolver.Current.GetService<BetAuctionBO>())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.AuctionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Bet, opt => opt.MapFrom(src => src.Price));
                //============================================

            });
         }
    }
}