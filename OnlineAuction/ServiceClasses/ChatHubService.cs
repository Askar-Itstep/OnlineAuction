using AutoMapper;
using BusinessLayer.BusinessObject;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OnlineAuction.ServiceClasses
{
    public class ChatHubService
    {
        private IMapper mapper;
        private IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "r7NFXF1rmprl3xQlJ8lmhdWyVguJqNprrnxnUA2P",
            BasePath = "https://asp-net-with-firebase-default-rtdb.firebaseio.com/"
        };
        private IFirebaseClient client;
        private  UserVM UserVM { get; set; }

        public ChatHubService(IMapper mapper)
        {
            this.mapper = mapper;
        }
        public async Task<Tuple<string, UserVM>> RunChatHubAsync(object accountId, AccountBO accountBO, RoleAccountLinkBO roleAdmin)
        {
            string message = "";
            if (accountBO != null && roleAdmin == null)
            {
                client = new FireSharp.FirebaseClient(config);
                FirebaseResponse firebaseResponse = await client.GetAsync("Users");
                dynamic data = JsonConvert.DeserializeObject<dynamic>(firebaseResponse.Body);

                if (data != null)
                {
                    List<UserVM> list = GetUsersFirebase(data);
                    UserVM = list.Where(u => u.AccountId == (int)accountId).FirstOrDefault();
                }

                if (UserVM == null)//UserHubBO
                {
                    //в табл. UserHubs - нет постоянн. данных! (при выходе из чата-данные удалятся)                    
                    UserVM = new UserVM { Id = "", AccountId = (int)accountId, ConnectionId = "" };
                    //сохр. в GoogleFirebase
                    try
                    {
                        InsertUserToFirebaseAsync(UserVM); //отступл. от правил (сохр. простой объект)
                    }
                    catch (Exception e)
                    {
                        message = e.Message;
                    }
                }
                //var account = mapper.Map<Account>(accountBO);

                //new model for Firebase 
                UserVM = new UserVM { Id = "", AccountId = (int)accountId, ConnectionId = "" };
                UserVM.Account = mapper.Map<AccountVM>(accountBO);

                //первый вызов инициализир. instance ChatHub
                var sender = PushSender.InstanceClient;
                sender.UserVM = UserVM;
                sender.mapper = mapper;
                await sender.SendHello(string.Format("А у нас новый участник: {0}", UserVM.Account.FullName));
            }
            return new Tuple<string, UserVM>(message, UserVM);
        }

        public static List<UserVM> GetUsersFirebase(dynamic data)
        {
            var list = new List<UserVM>();
            foreach (var item in data)
            {
                //System.Diagnostics.Debug.WriteLine("dynamic: " + JsonConvert.DeserializeObject<UserVM>(((JProperty)item).Value.ToString()));
                list.Add(JsonConvert.DeserializeObject<UserVM>(((JProperty)item).Value.ToString()));
            }

            return list;
        }

        public void InsertUserToFirebaseAsync(UserVM user)
        {
            client = new FireSharp.FirebaseClient(config);
            PushResponse responce = client.Push("Users/", user);
            user.Id = responce.Result.name;
            SetResponse setResponse = client.Set("Users/" + user.Id, user);
        }

    }
}