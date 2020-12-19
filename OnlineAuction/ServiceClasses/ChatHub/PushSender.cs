using AutoMapper;
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
        private readonly static Lazy<PushSender> _instanceClient = new Lazy<PushSender>(()
            => new PushSender(GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients));

        public static PushSender InstanceClient => _instanceClient.Value;

        public PushSender(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }
        public IMapper mapper;
        private IHubConnectionContext<dynamic> Clients { get; set; } // != ClientBO

        public UserVM UserVM { get;  set; }

        //1-ый вызов метода инициал. _instance ChatHub + context SignalR.GlobalHost
        public async Task SendHello(string message)
        {
            ChatHub.User = mapper.Map<UserVM>(UserVM);
            await Clients.All.hello(UserVM.Account.FullName, message);
        }

        public async Task SendMessage(string message, bool keySignIn = false)
        {
            //var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            ChatHub.User = UserVM;
            await Clients.All.addMessage(UserVM.Account.FullName, message);

            #region запись сообщен. в MSSQL кроме сообщ. приветствия
            var clientBO = DependencyResolver.Current.GetService<ClientBO>();
            ClientBO sender = clientBO.LoadAll().FirstOrDefault(c => c.AccountId == UserVM.Account.Id);
            List<ClientBO> addressers = clientBO.LoadAll().ToList(); 

            //не все клиенты, а только вошедшие на chat  //сопоставить с юзерами ChatHub       
            var usersHubBO = ChatHub.Users.Select(u => mapper.Map<UserHubBO>(u));
            IEnumerable<AccountBO> chatAccounts = usersHubBO.Select(u => u.Account);
            var chatClients = addressers.SelectMany(c => chatAccounts,
                                                (c, a) => new { client = c, account = a })
                                                .Where(c => c.client.AccountId == c.account.Id)
                                                .Select(c => c.client);
            if (keySignIn == false) 
            {
                chatClients.ToList().ForEach(a => SaveMessage(message, UserVM.Account.Id, a));
            }
            #endregion
        }


        public async Task<bool> CommunicationWIthAuthorAsync(string message, string myConnectId, string connectId)
        {
            ChatHub.User = UserVM; 
            //сообщ. только у получателя!
            await Clients.Client(connectId).addMessage(UserVM.Account.FullName, message, UserVM.AccountId);

            //доп сообщ. send self
            await Clients.Client(myConnectId).addMessage(UserVM.Account.FullName, message, UserVM.AccountId);

            //------------------save database----------------------
            //найти клиента с соотв. connectID
            var user = ChatHub.Users.FirstOrDefault(u => u.ConnectionId == connectId);
            int userAccountId = 0;
            if (user != null)
            {
                userAccountId = user.AccountId;
            }

            //using MSSQL
            ClientBO actorBO = DependencyResolver.Current.GetService<ClientBO>().LoadAll().FirstOrDefault(c => c.AccountId == UserVM.Account.Id);
            if (userAccountId != 0)
            {
                SaveMessage(message, userAccountId, actorBO);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SaveMessage(string message, object accountId, ClientBO actorBO)
        {
            var messageBO = DependencyResolver.Current.GetService<MessageBO>();
            messageBO.Sms = message;
            var clientBO = DependencyResolver.Current.GetService<ClientBO>();
            IEnumerable<ClientBO> clients = clientBO.LoadAll();

            var senderClient = clients.FirstOrDefault(c => c.AccountId == (int)accountId);

            if (senderClient != null)
            {
                messageBO.ClientId = (int)senderClient.Id;
                messageBO.PartnerId = (int)actorBO.Id;
                //messageBO.Save(messageBO);
            }
        }
    }
}