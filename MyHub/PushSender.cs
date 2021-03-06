﻿using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer.Entities;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
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
        private IHubConnectionContext<dynamic> Clients { get; set; }

        //public static List<UserHubBO> UsersBO { get; set; } //= ChatHub.Users;   
        //public static List<UserHubBO> Users { get; set; } //= ChatHub.Users;   
        public UserHubBO UserBO { get; set; }
        public UserHub User { get; set; }

        public async Task SendHello(string message)
        {
            ChatHub.User = mapper.Map<UserHub>(UserBO);
            await Clients.All.hello(UserBO.Account.FullName, message);
        }

        public async Task SendMessage(string message, bool keySignIn = false)
        {
            //var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            ChatHub.User = mapper.Map<UserHub>(UserBO);
            await Clients.All.addMessage(UserBO.Account.FullName, message, UserBO.Account.Id);

            //сохр. в БД
            var clientBO = DependencyResolver.Current.GetService<ClientBO>();
            ClientBO sender = clientBO.LoadAll().FirstOrDefault(c => c.AccountId == UserBO.Account.Id);
            List<ClientBO> addressers = clientBO.LoadAll().ToList();

            //не все клиенты, а только вошедшие на chat  //сопоставить с юзерами ChatHub       
            var usersHubBO = ChatHub.Users.Select(u => mapper.Map<UserHubBO>(u));
            IEnumerable<AccountBO> chatAccounts = usersHubBO.Select(u => u.Account);
            var chatClients = addressers.SelectMany(c => chatAccounts,
                                                (c, a) => new { client = c, account = a })
                                                .Where(c => c.client.AccountId == c.account.Id)
                                                .Select(c => c.client);

            if (keySignIn == false) //запись в БД кроме сообщ. приветствия
            {
                //chatClients.ToList().ForEach(a => SaveMessage(message, User.Account.Id, a));
            }
        }


        public async Task<bool> CommunicationWIthAuthorAsync(string message, string myConnectId, string connectId)
        {
            ChatHub.User = mapper.Map<UserHub>(UserBO);
            //сообщ. только у получателя!
           await Clients.Client(connectId).addMessage(UserBO.Account.FullName, message, UserBO.AccountId);
            //доп сообщ. send self
            await Clients.Client(myConnectId).addMessage(UserBO.Account.FullName, message, UserBO.AccountId);
            //------------------save database----------------------
            //найти клиента с соотв. connectID
            var user = ChatHub.Users.FirstOrDefault(u => u.ConnectionId == connectId);
            int userAccountId = 0;
            if (user != null)
            {
                userAccountId = user.AccountId;
            }

            ClientBO actorBO = DependencyResolver.Current.GetService<ClientBO>().LoadAll().FirstOrDefault(c => c.AccountId == UserBO.Account.Id);
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
                messageBO.Save(messageBO);
            }
        }
    }
}