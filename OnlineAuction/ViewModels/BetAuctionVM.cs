namespace OnlineAuction.ViewModels
{
    public class BetAuctionVM
    {
        public int Id { get; set; }

        public int AuctionId { get; set; }
        public virtual AuctionVM Auction { get; set; }

        public int ClientId { get; set; }
        public virtual ClientVM Client { get; set; }

        public decimal Bet { get; set; }



    }
}