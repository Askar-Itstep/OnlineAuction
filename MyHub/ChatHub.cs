using DataLayer.Entities;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
//using OnlineAuction.ViewModels;

namespace MyHub
{
    public class ChatHub : Hub  //хаб-концентратор
    {   
        //-----------------------------------------------------------------------------
        public static List<UserHub> Users { get; set; } 
        public static UserHub User { get; set; } //аккаунт пользователя
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
            //using (Model1 db = new Model1())
            //{
            //    Users = db.UserHubs.ToList(); //должно обновл.

            //    var id = Context.ConnectionId;

            //    //если имеющ. юзеры не имеют такого подключ. - добав.нов.юзера
            //    if (!Users.Any(x => x.ConnectionId == id))
            //    {
            //        User.ConnectionId = id;
            //        if (Users.FirstOrDefault(u => u.AccountId == User.AccountId) != null)
            //        {
            //            SqlParameter paramConnId = new SqlParameter("@ConnId", id);
            //            SqlParameter paramAccId = new SqlParameter("@AccId", User.AccountId);
            //            db.Database.ExecuteSqlCommand("Update UserHubs SET ConnectionId = @ConnId WHERE AccountId = @AccId", paramConnId, paramAccId);
            //        }
            //        else
            //        {
            //            Users.Add(User);
            //            db.UserHubs.Add(User);  //добавл., но также доб. нов. Account??????????
            //        }
            //        db.SaveChanges();
            //        //Users = db.UserHubs.ToList();

            //        //текущему пользователю вывести список юзеров на клиенте
            //        Clients.Caller.onConnected(id, userName, Users);//только на клиенте вызывающ. юзера

            //        // Показать нов. юзера - всем пользователям, кроме текущего
            //        Clients.AllExcept(id).onNewUserConnected(id, User); //userName
            //    }
            //}
            //============================FIREBASE=======================================
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse firebaseResponse = await client.GetAsync("Users");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(firebaseResponse.Body);

            if (data != null)
            {
                Users = new List<UserHub>();
                foreach (var item in data)
                {
                    Users.Add(JsonConvert.DeserializeObject<UserHub>(((JProperty)item).Value.ToString()));
                }
                var id = Context.ConnectionId;
                if (!Users.Any(x => x.ConnectionId == id))
                {
                    User.ConnectionId = id;
                    if (Users.FirstOrDefault(u => u.AccountId == User.AccountId) != null)
                    {
                        await UpdateUserIntoFirebaseAsync(User);
                    }
                    else
                    {
                        await InsertUserToFirebaseAsync(User);
                    }
                    Users = await GetUsersOutFirebase();
                    Clients.Caller.onConnected(id, userName, Users);

                    Clients.AllExcept(id).onNewUserConnected(id, User); 
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

        private async Task UpdateUserIntoFirebaseAsync(UserHub user)
        {
            //client = new FireSharp.FirebaseClient(config);
            //FirebaseResponse responce = await client.UpdateAsync("Users/", user);

            await InsertUserToFirebaseAsync(user);
        }

        private async Task<List<UserHub>> GetUsersOutFirebase()
        {
            FirebaseResponse firebaseResponse = await client.GetAsync("Users");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(firebaseResponse.Body);
            var list = new List<UserHub>();
            foreach (var item in data)
            {
                //list.Add(item.ResultAs<UserHub>());
                list.Add(JsonConvert.DeserializeObject<UserHub>(((JProperty)item).Value.ToString()));
            }
            return list;
        }

        // Отключение пользователя - при переходе на др. страницу или закрыт. браузера (только не сервера!)
        public override Task OnDisconnected(bool stopCalled)
        {
            using (Model1 db = new Model1()) {
                Users = db.UserHubs.ToList();
                //после отвала соед. в Connect() в клиенте происх. перезапись ConnId (стр. 91 _ChatPartView) ->может не найти!->Error
                var user = Users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                if (user != null) {
                    try {
                        Clients.All.onUserDisconnected(Context.ConnectionId, user.Account.FullName);
                        Users.Remove(user);
                        db.UserHubs.Remove(user);
                        db.SaveChanges();
                    }
                    catch(Exception e) {
                        System.Diagnostics.Debug.WriteLine("Error: ", e.Message);
                    }

                }
            }
            return base.OnDisconnected(stopCalled);
        }


        ////---------не используются-----------------
        //public async Task JoinRoomAsync(string roomName)
        //{
        //    await Groups.Add(Context.ConnectionId, roomName);
        //    Clients.Group(roomName).addMessage(Context.User.Identity.Name + " joined.");
        //}

        //public Task LeaveRoom(string roomName)
        //{
        //    return Groups.Remove(Context.ConnectionId, roomName);
        //}
    }
}