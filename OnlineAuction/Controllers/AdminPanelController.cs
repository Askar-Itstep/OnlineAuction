using AutoMapper;
using OnlineAuction.ServiceClasses;
using OnlineAuction.ViewModels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OnlineAuction.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminPanelController : Controller
    {
        private IMapper mapper;
        public AdminPanelController(IMapper mapper)
        {
            this.mapper = mapper;
        }
        public ActionResult Index(int? statistic)
        {
            return View();
        }

        public ActionResult SwitchContainer(string platform)
        {
            //db.Platform->INSERT INTO Platform values(azure, GETDATE()) 
            return RedirectToAction("Index");
        }
        //===================================S.T.A.T.I.S.T.I.C.S.=====================================
        public static IEnumerable<StatisticViewModel> StatisticModels { get; set; }
        private string[] colors = { "bisque", "orange", "green", "aqua", "gold", "olive", "azure", "yelloy", "violet" };
        public ActionResult Statistic()
        {
            ServiceStatistics.mapper = mapper;
            StatisticModels = ServiceStatistics.CreateStatisticModel();
            #region Print
            //foreach (var item in StatisticModels)
            //{
            //    System.Diagnostics.Debug.WriteLine("auctID: {0}, Fullname: {1}, prodTitle: {2}, countBet: {3}, maxBet{4}, isBuy: {5}", item.AuctionId,
            //        item.Account.FullName, item.Product.Title, item.CountBet, item.MaxBet, item.IsBuy);
            //}
            #endregion

            return new JsonResult { Data = "Данные статистики подготовлены!", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public ActionResult BeginHandler(int? key)
        {
            object res = null;
            #region Remove
            //if (key == null)
            //{
            //    return new JsonResult { Data = new { success = false, bundle = "Error. Key isNull" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //}
            //else 
            #endregion
            switch (key)
            {
                case 0:
                    {
                        res = CategoriesByGenderStatistic();
                        return new JsonResult { Data = new { success = true, data = res }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                case 1:
                    {
                        res = RegistrationUsersOnYear();
                        return new JsonResult { Data = new { success = true, data = res }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                case 2:
                    {
                        res = CategoriesByArea();
                        return new JsonResult { Data = new { success = true, data = res }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                case 3:
                    {
                        res = StatisticByAllCategories();
                        return new JsonResult { Data = new { success = true, data = res }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                case 4:
                    {
                        res = CategoriesByAge();
                        return new JsonResult { Data = new { success = true, data = res }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                case 5:
                    {
                        res = AvgPriceByCategory();
                        return new JsonResult { Data = new { success = true, data = res }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
            }
            return new JsonResult { Data = new { success = false, bundle = "Error. Key isNull" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        //5
        private object AvgPriceByCategory()
        {
            var seq = StatisticModels.Select(s => new { CategoryTitle = s.Product.Category.Title, Prcies = s.Product.Price });
            var labelsOx = seq.GroupBy(s => s.CategoryTitle).Select(s => s.First()).Select(s => s.CategoryTitle);//DVD, Book, ..
            //2
            var titleAvgPrices = labelsOx.Select(s => "AVG price " + s);
            var arrLabel = new ArrayList();
            for (int i = 0; i < titleAvgPrices.Count(); i++)
            {
                arrLabel.Add(new { avgPrice = titleAvgPrices.ToList()[i], color = colors[i] });
            }
            //3-сред. цены по категориям

            var arrData = new ArrayList();
            foreach (var category in labelsOx)
            {
                var avgPriceCurrCategory = seq.Where(s => s.CategoryTitle == category).GroupBy(c => c.CategoryTitle).Select(s => new
                {
                    Category = s.Key,
                    Count = s.Average(a => a.Prcies)
                });
                //System.Diagnostics.Debug.WriteLine("category: {0}, avg: {1}" + avgPriceCurrCategory.Ca);
                arrData.Add(avgPriceCurrCategory);
            }

            return new { labels = labelsOx, arrLabel, arrData };
        }

        //4
        private object CategoriesByAge()
        {
            //юзеров сгруппиров. по 10-летн. группам
            var seq = StatisticModels.Select(s => new { s.Product.CategoryId, CategoryTitle = s.Product.Category.Title, s.Account.Age });
            var labelsOx = seq.GroupBy(s => s.Age).Select(s => s.First()).Select(s => s.Age);   //23,30,40..
            var labels = seq.GroupBy(s => s.CategoryTitle).Select(s => s.First()).Select(s => s.CategoryTitle); //DVD, Book, ..

            var arrLabel = new ArrayList();
            for (int i = 0; i < labels.Count(); i++)
            {
                arrLabel.Add(new { category = labels.ToList()[i], color = colors[i] });
            }
            var arrData = new ArrayList();

            foreach (var age in labelsOx)
            {
                var countCategoriesByAge = seq.Where(s => s.Age == age).GroupBy(s => s.CategoryTitle).Select(s => new { Category = s.Key, Count = s.Count() });
                arrData.Add(countCategoriesByAge);
            }
            return new { labels = labelsOx, arrLabel, arrData };
        }
        //3
        private object StatisticByAllCategories()   //популярность категорий
        {
            var seq = StatisticModels.Select(s => new { CategorId = s.Product.CategoryId, CategoryTitle = s.Product.Category.Title });

            var labelsOx = seq.GroupBy(s => s.CategoryTitle).Select(s => s.First()).Select(s => s.CategoryTitle); //DVD, Electronic..

            var arrData = new ArrayList();
            var categories = seq.GroupBy(s => s.CategoryTitle).Select(s => new { Category = s.Key, Count = s.Count() });
            foreach (var categoryWIthCount in categories)
            {
                arrData.Add(categoryWIthCount);
            }
            var labels = labelsOx;
            var arrLabel = new ArrayList();
            for (int i = 0; i < labels.Count(); i++)
            {
                var item = arrLabel.Add(new { category = labels.ToList()[i], color = colors[i] });
            }
            var res = new { labels = labelsOx, arrLabel, arrData };
            return res;
        }

        //2
        private object CategoriesByArea()
        {
            var seq = StatisticModels.Select(s => new { CategorId = s.Product.CategoryId, CategoryTitle = s.Product.Category.Title, s.Account.Address.City, s.Account.Address.Street });
            var labels = seq.GroupBy(s => s.City).Select(s => s.First()).Select(s => s.City); //Almaty, Karagandy, Almaty...
            var labelsOx = seq.GroupBy(s => s.CategoryTitle).Select(s => s.First()).Select(s => s.CategoryTitle); //DVD, Electronic..

            #region ExampleAlmaty
            //var countCategoriasByAlmaty = seq.Where(s => s.City.ToUpper().Contains("ALMATY")).GroupBy(d => d.CategoryTitle).Select(s => new { s.Key, Count = s.Count() });
            //foreach (var item in countCategoriasByAlmaty)
            //{
            //    System.Diagnostics.Debug.WriteLine(item);   //{DVD, 8}, {Book, 2}..
            //}
            #endregion
            //a) foreach
            var arrData = new ArrayList();
            foreach (var item in labels)
            {
                //System.Diagnostics.Debug.WriteLine(item);
                var countCategoriasByCity = seq.Where(s => s.City == item).GroupBy(s => s.CategoryTitle).Select(s => new { s.Key, Count = s.Count() });
                arrData.Add(countCategoriasByCity);
            }

            #region Error path
            //var categories = seq.GroupBy(s => s.CategoryTitle).Select(s => new { Category = s.Key, Count = s.Count(), Area = s.Select(a => a.City) });
            //foreach (var categoryWIthCount in categories)
            //{
            //    System.Diagnostics.Debug.WriteLine("category: " + categoryWIthCount);   //{DVD, 12}, {Elecronic, 4}..
            //    arrData.Add(categoryWIthCount);
            //}
            #endregion

            var arrLabel = new ArrayList();
            for (int i = 0; i < labels.Count(); i++)
            {
                var item = arrLabel.Add(new { area = labels.ToList()[i], color = colors[i] });
                //var item2 = arrLabel.Add(new { category = categories.ToList()[i].Category, color = colors[i] });
            }
            var res = new { labels = labelsOx, arrLabel, arrData };
            return res;
        }

        //1
        private object RegistrationUsersOnYear()
        {
            var seq = StatisticModels.Select(s => new { Gender = s.Account.Gender.ToString(), Year = s.Account.CreateAt });
            var labelsOx = seq.GroupBy(s => s.Year).Select(s => s.First()).Select(s => s.Year); //2017, 2018,..
            //а может быть не 3 группы
            var countMenOnYear = seq.Where(s => s.Gender.Equals(Gender.MEN.ToString()))
                                                                                                .GroupBy(s => s.Year).Select(s => new { Year = s.Key, Count = s.Count() });
            var countWomenOnYear = seq.Where(s => s.Gender.Equals(Gender.WOMEN.ToString()))
                                                                                                    .GroupBy(s => s.Year).Select(s => new { Year = s.Key, Count = s.Count() });
            var countXOnYear = seq.Where(s => s.Gender.Equals(Gender.UNDEFINED.ToString()))
                                                                                                    .GroupBy(s => s.Year).Select(s => new { Year = s.Key, Count = s.Count() });
            var arrData = new ArrayList { countMenOnYear, countWomenOnYear, countXOnYear };

            var genders = seq.GroupBy(s => s.Gender).Select(g => new { Gender = g.Key, Count = g.Count() });
            var arrLabel = new ArrayList();
            for (int i = 0; i < genders.Count(); i++)
            {
                arrLabel.Add(new { gender = genders.ToList()[i].Gender, color = colors[i] });//Error!
            }
            //Итог--
            var res = new { labels = labelsOx, arrLabel, arrData };
            return res;
        }
        //0------------------------------------------
        private object CategoriesByGenderStatistic()
        {
            var seq = StatisticModels.Select(s => new { s.Account.Gender, CategoryId = s.Product.Category.Id, CategoryName = s.Product.Category.Title });
            #region Print
            //foreach (var item in seq)
            //{
            //    System.Diagnostics.Debug.WriteLine(item.CategoryName);
            //}
            #endregion
            var labels = seq.GroupBy(s => s.CategoryId).Select(s => s.First()).Select(s => s.CategoryName); //DVD, Electronic..

            var countByCategoriesForMan = seq.Where(s => s.Gender == Gender.MEN).GroupBy(s => s.CategoryId).Select(s => new { CategoryId = s.Key, Count = s.Count() });
            var countByCategoriesForWomen = seq.Where(s => s.Gender == Gender.WOMEN).GroupBy(s => s.CategoryId).Select(s => new { CategoryId = s.Key, Count = s.Count() });
            var countByCategoriesForX = seq.Where(s => s.Gender == Gender.UNDEFINED).GroupBy(
                                                                                                 s => s.CategoryId).Select(s => new { CategoryId = s.Key, Count = s.Count() });
            #region Print
            //foreach (var item in countOnCategoriesForMan)
            //{
            //    System.Diagnostics.Debug.WriteLine("Category: {0}, Count: {1}", item.Category, item.Count);
            //}
            #endregion
            var arrLabel = new ArrayList { new { gender = "Mens", color = "bisque" }, new { gender = "Womens", color = "orange" }, new { gender = "X", color = "green" } };
            var arrData = new ArrayList { countByCategoriesForMan, countByCategoriesForWomen, countByCategoriesForX };
            //-------
            var res = new { labels, arrLabel, arrData };
            return res;
        }
    }
}