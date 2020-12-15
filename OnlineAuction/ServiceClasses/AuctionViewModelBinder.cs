using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineAuction.ServiceClasses
{
    //не использ. - стандартн. работает!
    public class AuctionViewModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            // Получаем поставщик значений
            var valueProvider = bindingContext.ValueProvider;

            // получаем данные по одному полю
            ValueProviderResult vprId = valueProvider.GetValue("Id");


            // получаем данные по остальным полям
            string title = (string)valueProvider.GetValue("Title").ConvertTo(typeof(string));
            string descript = (string)valueProvider.GetValue("Description").ConvertTo(typeof(string));
            int prodId = (int)valueProvider.GetValue("ProductId").ConvertTo(typeof(int));
            int categorId = (int)valueProvider.GetValue("CategoryId").ConvertTo(typeof(int));
            decimal price = (decimal)valueProvider.GetValue("Price").ConvertTo(typeof(decimal));
            decimal step = (decimal)valueProvider.GetValue("Step").ConvertTo(typeof(int));
            decimal redemPrice = (decimal)valueProvider.GetValue("RedemptionPrice").ConvertTo(typeof(int));
            DateTime dayBegin = (DateTime)valueProvider.GetValue("DayBegin").ConvertTo(typeof(int));
            TimeSpan timeBegin = (TimeSpan)valueProvider.GetValue("TimeBegin").ConvertTo(typeof(int));
            float duration = (float)valueProvider.GetValue("Duration").ConvertTo(typeof(int));

            int year = (int)valueProvider.GetValue("Year").ConvertTo(typeof(int));
            AuctionEditVM auctionEditVM = new AuctionEditVM() {
                Title=title, Description=descript, ProductId=prodId, CategoryId=categorId, Price=price, Step=step, RedemptionPrice=redemPrice,
                DayBegin=dayBegin, TimeBegin=timeBegin, Duration=duration
            };

            // если поле Id определено (редактирование)
            if (vprId != null)
            {
                auctionEditVM.Id = (int)vprId.ConvertTo(typeof(int));
            }
            return auctionEditVM;
        }
    }
}
