using AutoMapper;
using BusinessLayer.BusinessObject;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OnlineAuction.Jobs
{
    public class EmailSender : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.JobDetail.JobDataMap;
            int winnerId = data.GetInt("winnerId");
            int auctionId = data.GetInt("auctionId");
            AuctionBO auction = DependencyResolver.Current.GetService<AuctionBO>().Load(auctionId);
            BetAuctionBO betAuctionBO = DependencyResolver.Current.GetService<BetAuctionBO>();

            //ставки в аукционе
            List<BetAuctionBO> betAuctions = betAuctionBO.LoadAll().Where(b => b.AuctionId == auctionId).ToList();

            //кто делал эти ставки - участники аукциона
            List<ClientBO> clients = betAuctions.Select(b => b.Client).ToList();
            var winner = clients.FirstOrDefault(c => c.Id == winnerId);

            //их аккаунты
            List<AccountBO> accounts = clients.Select(c => c.Account).ToList();
            List<string> emails = accounts.Select(a => a.Email).ToList();

            //winner
            var winnerAccount = accounts.FirstOrDefault(a => a.Id == winner.AccountId);
            foreach (var email in emails) {
                using (MailMessage message = new MailMessage("admin@mail.ru", email)) {

                    message.Subject = "Новостная рассылка";
                    message.Body = String.Format("Победитель аукциона: {0}", winnerAccount.FullName);
                    using (SmtpClient client = new SmtpClient
                    {
                        EnableSsl = true,
                        Host = "smtp.mail.ru",
                        Port = 25,
                        Credentials = new NetworkCredential("admin@mail.ru", "admin")
                    }) {
                        await client.SendMailAsync(message);
                    }
                }
            }
        }
    }
}