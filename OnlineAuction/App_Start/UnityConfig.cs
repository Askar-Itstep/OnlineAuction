using System;
using System.Web.Mvc;
using Unity;
using Unity.AspNet.Mvc;

namespace OnlineAuction
{
    public static class UnityConfig
    {
        #region Unity Container
        //private static Lazy<IUnityContainer> container =
        //  new Lazy<IUnityContainer>(() =>
        //  {
        //      var container = new UnityContainer();
        //      RegisterTypes(container);
        //      return container;
        //  });
        
        #endregion


        public static void RegisterTypes()
        {
            var container = new UnityContainer();

            container.RegisterInstance<IUnityContainer>(container); 

            AutoMapperConfig.RegisterWithUnity(container);  

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}