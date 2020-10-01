using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineAuction.ServiceClasses
{
    public class PushSender
    {
        private readonly static Lazy<PushSender> _instance = new Lazy<PushSender>(() 
            => new PushSender(GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients));
       
        public static PushSender Instance
        {
            get
            {
                return _instance.Value;
            }
        }
        public PushSender(IHubConnectionContext<dynamic> clients)
        {
            this.Clients = clients;
        }
        private IHubConnectionContext<dynamic> Clients { get; set; }

        public static List<AccountVM> Users = ChatHub.Users;    //new List<AccountVM>();
        public  AccountVM AccountVM { get; set; }
        //public string ConnectionId { get; set; }

        public  async Task SendMessage(string message)
        {
            //var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            ChatHub.AccountVM = AccountVM;
            List<AccountVM> res = ChatHub.Users;
            await Clients.All.addMessage(message);
        }

        //public async Task SendMessageGroup(string message)
        //{
        //    ChatHub.AccountVM = AccountVM;

        //    //await context.Groups.Add(, )
        //    await Clients.All.addMessage(message);

        //}
        public async Task CommunicationWIthAuthor(string message, string connectId)
        {
            ChatHub.AccountVM = AccountVM;
            await Clients.Client(connectId).addMessage(AccountVM.FullName, message);
        }
    }
}