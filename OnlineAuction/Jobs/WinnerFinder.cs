﻿using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
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
            //BetAuctionBO winnBet = HelperOrderCreate.CloseAuction(auction, bets: auctionBets);
            auctionBO = HelperOrderCreate.CloseAuction(auctionId, bets: auctionBets);

            //1)отправить push-уведомл. участникам аукциона
            AccountBO winnerBO = auctionBO.Winner.Account;  //winnBet.Client.Account;
            //Account winner = mapper.Map<Account>(winnerBO);
            var sender = PushSender.InstanceClient;
            await sender.SendMessage(string.Format("Победитель аукциона: {0}", winnerBO.FullName)); // winnBet.Client.Account.FullName

            //2)отправить письма о победителе аукциона
            EmailScheduler.AuctionId = auctionBO.Id;
            EmailScheduler.WinnerId = auctionBO.WinnerId;
            EmailScheduler.Start();
        }
    }
}