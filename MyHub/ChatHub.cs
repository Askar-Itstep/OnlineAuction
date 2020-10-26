using DataLayer.Entities;
using Microsoft.AspNet.SignalR;
using OnlineAuction.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineAuction
{
    public class ChatHub : Hub  //хаб-концентратор
    {
        public static List<UserHub> Users { get; set; } 
        public static UserHub User { get; set; } //аккаунт пользователя

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
            using (Model1 db = new Model1()) {                
                Users = db.UserHubs.ToList(); //должно обновл.
                
                var id = Context.ConnectionId;

                //если имеющ. юзеры не имеют такого подключ. - добав.нов.юзера
                if (!Users.Any(x => x.ConnectionId == id)) {
                    User.ConnectionId = id;
                    if (Users.FirstOrDefault(u => u.AccountId == User.AccountId) != null) {
                        SqlParameter paramConnId = new SqlParameter("@ConnId", id);
                        SqlParameter paramAccId = new SqlParameter("@AccId", User.AccountId);
                        db.Database.ExecuteSqlCommand("Update UserHubs SET ConnectionId = @ConnId WHERE AccountId = @AccId", paramConnId, paramAccId);
                    }
                    else {
                        Users.Add(User);
                        db.UserHubs.Add(User);  //добавл., но также доб. нов. Account??????????
                    }
                    await db.SaveChangesAsync();
                    //Users = db.UserHubs.ToList();

                    //текущему пользователю вывести список юзеров на клиенте
                    Clients.Caller.onConnected(id, userName, Users);//только на клиенте вызывающ. юзера

                    // Показать нов. юзера - всем пользователям, кроме текущего
                    Clients.AllExcept(id).onNewUserConnected(id, User); //userName
                }
            }

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