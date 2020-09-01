using System;
using System.Web.Mvc;
using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
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

                // mpr.CreateMap<AccountVM, Account>()
                //.ConstructUsing(c => DependencyResolver.Current.GetService<Account>());

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

                // mpr.CreateMap<ClientVM, Client>()
                //.ConstructUsing(c => DependencyResolver.Current.GetService<Role>());

   //-----------------------RoleAccountLink---------------------------
                mpr.CreateMap<RoleAccountLink, RoleAccountLinkBO>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<RoleAccountLinkBO>());

                mpr.CreateMap<RoleAccountLinkBO, RoleAccountLink>()
               .ConstructUsing(c => DependencyResolver.Current.GetService<RoleAccountLink>());

                // mpr.CreateMap<RoleAccountLinkVM, RoleAccountLink>()
                //.ConstructUsing(c => DependencyResolver.Current.GetService<Role>());
            });
         }
    }
}