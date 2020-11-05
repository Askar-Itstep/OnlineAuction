using AutoMapper;
using OnlineAuction.ServiceClasses;
using OnlineAuction.ViewModels;
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
        // GET: AdminPanel
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

            return new JsonResult { Data = new { message = "Данные статистики подготовлены!", seq = StatisticModels }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public ActionResult CategoriesOnGenderStatistic()
        {
            var seq = StatisticModels.Select(s => new { Gender = s.Account.Gender, CategoryId = s.Product.Category.Id, CategoryName = s.Product.Category.Title });
            ViewBag.Seq = seq;
            return PartialView("Partial/_CategoriessOnGender");
        }
    }
}