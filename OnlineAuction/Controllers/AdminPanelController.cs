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
            if (key == 0)
            {
                res = CategoriesOnGenderStatistic();
                return new JsonResult { Data = new { success = true, data = res }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else if (key == 1)
            {
                res = RegistrationUsersOnYear();
                return new JsonResult { Data = new { success = true, data = res }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return new JsonResult { Data = new { success = false, bundle = "Error. Key isNull" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        private string[] colors = { "bisque", "orange", "green", "aqua", "gold", "olive", "azure", "yelloy", "violet" };
        private object RegistrationUsersOnYear()
        {
            var seq = StatisticModels.Select(s => new { s.Account.Gender, Year = s.Account.CreateAt });
            var labels = seq.GroupBy(s => s.Year).Select(s => s.First()).Select(s => s.Year); //2017, 2018,..
            var countMenOnYear = seq.Where(s => s.Gender == Gender.MEN).GroupBy(
                                            s => s.Year).Select(s => new { Year = s.Key, Count = s.Count() });
            var countWomenOnYear = seq.Where(s => s.Gender == Gender.WOMEN).GroupBy(
                                             s => s.Year).Select(s => new { Year = s.Key, Count = s.Count() });
            var countXOnYear = seq.Where(s => s.Gender == Gender.UNDEFINED).GroupBy(
                                                s => s.Year).Select(s => new { Year = s.Key, Count = s.Count() });
            var arrLabel = new ArrayList();
            for (int i = 0; i < labels.Count(); i++)
            {
                var item = arrLabel.Add(new { year = labels.ToList()[i], color = colors[i] });
            }
            var arrData = new ArrayList { countMenOnYear, countWomenOnYear, countXOnYear };
            //Итог--
            var res = new { labels, arrLabel, arrData };
            return res;
        }

        private object CategoriesOnGenderStatistic()
        {
            var seq = StatisticModels.Select(s => new { s.Account.Gender, CategoryId = s.Product.Category.Id, CategoryName = s.Product.Category.Title });
            #region Print
            //foreach (var item in seq)
            //{
            //    System.Diagnostics.Debug.WriteLine(item.CategoryName);
            //}
            #endregion
            var labels = seq.GroupBy(s => s.CategoryId).Select(s => s.First()).Select(s => s.CategoryName); //DVD, Electronic..

            var countOnCategoriesForMan = seq.Where(s => s.Gender == Gender.MEN).GroupBy(
                                                    s => s.CategoryId).Select(s => new { CategoryId = s.Key, Count = s.Count() });
            var countOnCategoriesForWomen = seq.Where(s => s.Gender == Gender.WOMEN).GroupBy(
                                                       s => s.CategoryId).Select(s => new { CategoryId = s.Key, Count = s.Count() });
            var countOnCategoriesForX = seq.Where(s => s.Gender == Gender.UNDEFINED).GroupBy(
                                                    s => s.CategoryId).Select(s => new { CategoryId = s.Key, Count = s.Count() });
            #region Print
            //foreach (var item in countOnCategoriesForMan)
            //{
            //    System.Diagnostics.Debug.WriteLine("Category: {0}, Count: {1}", item.Category, item.Count);
            //}
            #endregion
            var arrLabel = new ArrayList { new { gender = "Mens", color = "bisque" }, new { gender = "Womens", color = "orange" }, new { gender = "X", color = "green" } };
            var arrData = new ArrayList { countOnCategoriesForMan, countOnCategoriesForWomen, countOnCategoriesForX };//new {  };
            //-------
            var res = new { labels, arrLabel, arrData };
            return res;
        }
    }
}