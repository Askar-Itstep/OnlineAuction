using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuction.ViewModels
{
    public class UserHubVM
    {
        public int Id { get; set; }

        public int AccountId { get; set; }
        public virtual AccountVM Account { get; set; }

        public string ConnectionId { get; set; }
    }
}