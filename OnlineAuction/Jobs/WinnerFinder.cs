using AutoMapper;
using BusinessLayer.BusinessObject;
using OnlineAuction.Schedulers;
using OnlineAuction.ServiceClasses;
using OnlineAuction.ViewModels;
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
        public static IMapper mapper { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.JobDetail.JobDataMap;
            int auctionId = data.GetInt("auctionId");  

            AuctionBO auctionBO = DependencyResolver.Current.GetService<AuctionBO>();
            var auction = auctionBO.Load(auctionId);

            BetAuctionBO betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();
            List<BetAuctionBO> auctionBets = betAuctionBO.LoadAll().Where(b => b.AuctionId == auctionId).ToList();
            BetAuctionBO winnBet = HelperOrderCreate.CloseAuction(auction, bets: auctionBets);

            //1)отправить push-уведомл. участникам аукциона
            AccountBO winnerBO = winnBet.Client.Account;
            AccountVM winner = mapper.Map<AccountVM>(winnerBO);
            var sender = PushSender.InstanceClient;
            sender.Account = winner;
            await sender.SendMessage(string.Format("Победитель аукциона: {0}", winner.FullName)); // winnBet.Client.Account.FullName

            //2)отправить письма о победителе аукциона
            EmailScheduler.AuctionId = auctionBO.Id;
            EmailScheduler.WinnerId = auctionBO.WinnerId;
            EmailScheduler.Start();
        }
    }
}