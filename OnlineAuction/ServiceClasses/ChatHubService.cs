using AutoMapper;
using BusinessLayer.BusinessObject;
using DataLayer;
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

namespace OnlineAuction.ServiceClasses
{
    public class ChatHubService
    {
        private IMapper mapper;
        //using Firebase GoogleCloud-----------------------------

        private IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = MyConfig.authSecretFirebase, 
                                         //"r7NFXF1rmp..........yVguJqNprrnxnUA2P",
            BasePath = MyConfig.basePathFirebase
                                     //"https:\//asp-..........firebaseio.com/"
        };
        private IFirebaseClient client;
        private  UserVM UserVM { get; set; }

        public ChatHubService(IMapper mapper)
        {
            this.mapper = mapper;
        }
        public async Task<Tuple<string, UserVM, bool>> RunChatHubAsync(object accountId, AccountBO accountBO, RoleAccountLinkBO roleAdmin)
        {
            bool flag = CheckFirebase();
            if (!flag)
            {
                return new Tuple<string, UserVM, bool>("Firebase dsn't work!", new UserVM { Id = "", AccountId = (int)accountId, ConnectionId = "" }, flag);
            }
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

                if (UserVM == null)
                {
                    //в табл. UserHubs - нет постоянн. данных! (при выходе из чата-данные удалятся)                    
                    UserVM = new UserVM { Id = "", AccountId = (int)accountId, ConnectionId = "" };
                    //сохр. в GoogleFirebase
                    try
                    {
                        InsertUserToFirebaseAsync(UserVM); //сохр. п/пустой объект
                    }
                    catch (Exception e)
                    {
                        message = "Error: "+e.Message;
                    }
                }
                 //new model for Firebase 
                UserVM = new UserVM { Id = "", AccountId = (int)accountId, ConnectionId = "" };
                UserVM.Account = mapper.Map<AccountVM>(accountBO);

                //первый вызов инициализир. instance ChatHub
                var sender = PushSender.InstanceClient;
                sender.UserVM = UserVM;
                sender.mapper = mapper;
                await sender.SendHello(string.Format("А у нас новый участник: {0}", UserVM.Account.FullName));
            }
            return new Tuple<string, UserVM, bool>(message, UserVM, flag);
        }

        private bool CheckFirebase()
        {
            return config.Host != null;
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