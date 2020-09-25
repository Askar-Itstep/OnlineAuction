using BusinessLayer.BusinessObject;
using OnlineAuction.Jobs;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuction.Schedulers
{
    public class EmailScheduler
    {
        public static int AuctionId { get; set; }
        public static int WinnerId { get; set; }

        public static async void Start()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<EmailSender>()
                .UsingJobData("auctionId", AuctionId)   //данные для EmailSender
                .UsingJobData("winnerId", WinnerId)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()  
                .WithIdentity("trigger1", "group1")     // идентифицируем триггер с именем и группой
                .StartNow()                            // запуск сразу после начала выполнения
                .WithSimpleSchedule(x => x            // настраиваем выполнение действия
                    //.WithIntervalInMinutes(1)          // через 1 минуту
                    //.RepeatForever())                   // бесконечное повторение
                    .WithRepeatCount(0))
                .Build();                               // создаем триггер

            await scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы
        }
    }
}