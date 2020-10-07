using Microsoft.AspNet.SignalR;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineAuction
{
    public class ChatHub : Hub  //хаб-концентратор
    {
        public static List<AccountVM> Users = new List<AccountVM>();
        public static AccountVM Account { get; set; } //аккаунт пользователя

        public void Hello()
        {
            Clients.All.hello();
        }
        public async Task HelloCaller(string name, string message)
        {
            await Clients.Caller.addMessage(name, message);
        }
        //к данным методам обращ. в клиенте API SignalR
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message, Account.Id);
        }
        
        public void Connect(string userName)
        {
            var id = Context.ConnectionId;
            //если имеющ. юзеры не имеют такого подключ. - добав.нов.юзера
            if (!Users.Any(x => x.ConnectionId == id)) {
                Account.ConnectionId = id;
                Users.Add(Account);
                // подключить текущего пользователю
                Clients.Caller.onConnected(id, userName, Users);

                // Показать кто вошел - всем пользователям, кроме текущего
                Clients.AllExcept(id).onNewUserConnected(id, userName);
            }           
        }

        // Отключение пользователя
        public override Task OnDisconnected(bool stopCalled)
        {
            var item = Users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null) { 
                Users.Remove(item);
                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.FullName);  // key.Item2
            }

            return base.OnDisconnected(stopCalled);
        }
        //---------не используются-----------------
        public async Task JoinRoomAsync(string roomName)
        {
            await Groups.Add(Context.ConnectionId, roomName);
            Clients.Group(roomName).addMessage(Context.User.Identity.Name + " joined.");
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.Remove(Context.ConnectionId, roomName);
        }
    }
}