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

      //--------------------------------------Image ---------------------------------------
                mpr.CreateMap<Image, ImageBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<ImageBO>());

                mpr.CreateMap<ImageBO, Image>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Image>());

                mpr.CreateMap<ImageVM, ImageBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<ImageBO>()).ReverseMap();

      //--------------------------------------Address ---------------------------------------
                mpr.CreateMap<Address, AddressBO>()
                                .ConstructUsing(c => DependencyResolver.Current.GetService<AddressBO>());

                mpr.CreateMap<AddressBO, Address>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<Address>());

                mpr.CreateMap<AddressVM, AddressBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<AddressBO>());

                mpr.CreateMap<AddressBO, AddressVM>()
              .ConstructUsing(c => DependencyResolver.Current.GetService<AddressVM>());
            });
         }
    }
}