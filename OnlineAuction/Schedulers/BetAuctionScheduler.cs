using OnlineAuction.Jobs;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuction.Schedulers
{
    public class BetAuctionScheduler
    {
        //public static DateTime DateBegin { get; set; }
        //public static string GetDateBeginString()
        //{
        //    return DateBegin.ToString();
        //}

        public static DateTime DateEnd { get; set; }

        //public static string GetDateEndString()
        //{
        //    return DateEnd.ToString();
        //}

        public static int AuctionId { get; set; }


        public static async void Start()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<WinnerFinder>()
                //.UsingJobData("DateBegin", GetDateBeginString())
                .UsingJobData("AuctionId", AuctionId)
                .Build();

            ITrigger simpleTrigger = TriggerBuilder.Create()
                 .WithIdentity("trigger2", "group2")
                //.StartAt(DateTime.SpecifyKind(DateBegin, DateTimeKind.Utc))
                .EndAt(DateTime.SpecifyKind(DateEnd, DateTimeKind.Utc))
                .Build();

        }
    }
}