using System;

namespace OnlineAuction.ViewModels
{
    public class StatisticViewModel
    {
        public int AuctionId { get; set; }
        public DateTime DateOrder { get; set; }

        public int AccountId { get; set; }
        public virtual AccountVM Account { get; set; }

        public int ProductId { get; set; }
        public virtual ProductVM Product { get; set; }

        //показатель увлеченности
        public decimal CountBet { get; set; }

        //уровень платеже-способн.
        public decimal MaxBet { get; set; }

        //товар выкуплен?
        public bool IsBuy { get; set; }
    }
}