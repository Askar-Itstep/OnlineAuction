using BusinessLayer.BusinessObject;
using OnlineAuction.Schedulers;
using OnlineAuction.ServiceClasses;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OnlineAuction.Jobs
{
    public class WinnerFinder : IJob    //найти победителя
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.JobDetail.JobDataMap;
            int auctionId = data.GetInt("auctionId");  

            AuctionBO auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
            var auction = auctionBO.Load(auctionId);

            BetAuctionBO betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();
            List<BetAuctionBO> auctionBets = betAuctionBO.LoadAll().Where(b => b.AuctionId == auctionId).ToList();
            HelperOrderCreate.CloseAuction(auction, bets: auctionBets);

            //запуск доп. задачи!
            EmailScheduler.AuctionId = auctionBO.Id;
            EmailScheduler.WinnerId = auctionBO.WinnerId;
            EmailScheduler.Start();
        }
    }
}