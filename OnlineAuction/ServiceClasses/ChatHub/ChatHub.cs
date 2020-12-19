using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineAuction
{
    public class ChatHub : Hub  //хаб-концентратор
    {   
        //-----------------------------------------------------------------------------
        public static List<UserVM> Users { get; set; } 
        public static UserVM User { get; set; } //аккаунт пользователя
        private IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "r7NFXF1rmprl3xQlJ8lmhdWyVguJqNprrnxnUA2P",
            BasePath = "https://asp-net-with-firebase-default-rtdb.firebaseio.com/"
        };
        private IFirebaseClient client;

        public void Hello()
        {
            Clients.All.hello();
        }
        public async Task HelloCaller(string name, string message)
        {
            await Clients.Caller.addMessage(name, message);
        }

        //к данным методам обращ. в клиенте API SignalR
        public void Send(string name, string message, int?accountId)
        {
            Clients.All.addMessage(name, message, accountId);
        }

        public async Task Connect(string userName)
        {
            //============================FIREBASE=======================================
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse firebaseResponse = await client.GetAsync("Users");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(firebaseResponse.Body);
            //после аутенитиф.  БД не пустая (кроме ConnectId, Account)
            if (data != null)
            {
                Users = new List<UserVM>();
                //
                foreach (var item in data)
                {
                    Users.Add(JsonConvert.DeserializeObject<UserVM>(((JProperty)item).Value.ToString()));
                }
                var connectId = Context.ConnectionId;
                if (!Users.Any(x => x.ConnectionId == connectId))
                {
                    User.ConnectionId = connectId;
                    var UserDB = Users.FirstOrDefault(u => u.AccountId == User.AccountId);
                    if ( UserDB!= null)
                    {
                        UserDB.ConnectionId = connectId;
                        UserDB.Account = User.Account;
                        await UpdateUserIntoFirebaseAsync(UserDB);
                    }
                    else
                    {
                        await InsertUserToFirebaseAsync(User);
                    }
                    Users = await GetUsersOutFirebase();
                    Clients.Caller.onConnected(connectId, userName, Users);

                    Clients.AllExcept(connectId).onNewUserConnected(connectId, User); 
                }
            }

        }

        private async Task InsertUserToFirebaseAsync(UserVM user)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = user;
            PushResponse responce = await client.PushAsync("Users/", data);
            user.Id = responce.Result.name;
            SetResponse setResponse = client.Set("Users/" +user.Id, user);
        }

        private async Task UpdateUserIntoFirebaseAsync(UserVM user)
        {
            client = new FireSharp.FirebaseClient(config);
            SetResponse setResponse = await client.SetAsync("Users/" + user.Id, user);
        }

        private async Task<List<UserVM>> GetUsersOutFirebase()
        {
            FirebaseResponse firebaseResponse = await client.GetAsync("Users");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(firebaseResponse.Body);
            var list = new List<UserVM>();
            foreach (var item in data)
            {
                list.Add(JsonConvert.DeserializeObject<UserVM>(((JProperty)item).Value.ToString()));
            }
            return list;
        }

        // Отключение пользователя - при переходе на др. страницу или закрыт. браузера (только не сервера!)
        public override Task OnDisconnected(bool stopCalled)
        {
            #region Lazy connect- MSSQL
            //using (Model1 db = new Model1()) {
            //    Users = db.UserHubs.ToList();
            //    //после отвала соед. в Connect() в клиенте происх. перезапись ConnId (стр. 91 _ChatPartView) ->может не найти!->Error
            //    var user = Users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            //    if (user != null) {
            //        try {
            //            Clients.All.onUserDisconnected(Context.ConnectionId, user.Account.FullName);
            //            Users.Remove(user);
            //            db.UserHubs.Remove(user);
            //            db.SaveChanges();
            //        }
            //        catch(Exception e) {
            //            System.Diagnostics.Debug.WriteLine("Error: ", e.Message);
            //        }

            //    }
            //}
            #endregion

            //Fast-connect: Realtime DB Google Firebase
            if (User != null)
            {
                try
                {
                    Clients.All.onUserDisconnected(Context.ConnectionId, User.Account.FullName);
                    Users.Remove(User);
                    //удалять из базы???????
                    client = new FireSharp.FirebaseClient(config);
                    FirebaseResponse response = client.Delete("Users/" + User.Id);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error: ", e.Message);
                }

            }

            return base.OnDisconnected(stopCalled);
        }


    }
}