using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OnlineAuction.ServiceClasses
{
    public class PushSender
    {
         //private readonly static Lazy<PushSender> _instanceGroup = new Lazy<PushSender>(() 
        //    => new PushSender(GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Groups)); 

        private readonly static Lazy<PushSender> _instanceClient = new Lazy<PushSender>(()
            => new PushSender(GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients)); 

        //public static PushSender InstanceGroup => _instanceGroup.Value;
        public static PushSender InstanceClient => _instanceClient.Value;

        public PushSender(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }

        //public PushSender(IGroupManager groups)
        //{
        //    this.Groups = groups;
        //}

        private IHubConnectionContext<dynamic> Clients { get; set; }

        public static List<UserHub> Users = ChatHub.Users;    
        //private IGroupManager Groups { get; set; }

        public  UserHub User { get; set; }

        public async Task SendHello(string message)
        {
            ChatHub.User = User;
            await Clients.All.hello(User.Account.FullName, message);
        }
        public  async Task SendMessage(string message)
        {
            //var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            ChatHub.User = User;
            await Clients.All.addMessage(User.Account.FullName, message, User.Account.Id);

            //сохр. в БД
            //var clientBO = DependencyResolver.Current.GetService<ClientBO>();
            //ClientBO sender = clientBO.LoadAll().FirstOrDefault(c => c.AccountId == User.Account.Id);
            //List<ClientBO> addressers = clientBO.LoadAll().ToList();
            //addressers.ForEach(a => SaveMessage(message, User.Account.Id, a));
        }

        public async Task CommunicationWIthAuthor(string message, string myConnectId, string connectId)
        {
            ChatHub.User = User;

            //signalR отправит и откр. сообщ. только у получателя!
            await Clients.Client(connectId).addMessage(User.Account.FullName, message, User.AccountId);
           
            //доп сообщ. self-send
            await Clients.Client(myConnectId).addMessage(User.Account.FullName, message, User.AccountId);
        }

        public static void SaveMessage(string message, object accountId, ClientBO actorBO)
        {
            var messageBO = DependencyResolver.Current.GetService<MessageBO>();
            messageBO.Sms = message;
            var clientBO = DependencyResolver.Current.GetService<ClientBO>();

            //исправить-не все клиенты, а только вошедшие на сайт и только в рамках аукциона!
            List<ClientBO> clients = clientBO.LoadAll().ToList(); 
            var findClient = clients.FirstOrDefault(c => c.AccountId == (int)accountId);
            if (findClient != null) {
                messageBO.ClientId = (int)findClient.Id;
                messageBO.PartnerId = (int)actorBO.Id;
                messageBO.Save(messageBO);
            }
        }
    }
}