using System.Collections.Generic;

namespace OnlineAuction.ViewModels
{
    public class ClientVM
    {
        public int? Id { get; set; }
        public ICollection<OrderVM> Orders { get; set; }

        private decimal cash;
        
        public int AccountId { get; protected set; }
        public virtual AccountVM Account { get; protected set; }
        
        public bool IsDebitor()
        {
            return cash < 0 ? true : false;
        }

        //а вхоДн. сумма отрицательной?
        public void AddCash(decimal sum)
        {
            cash += sum;
        }
        public void SetAccountId(int accountId)
        {
            AccountId = accountId;
        }
    }
}