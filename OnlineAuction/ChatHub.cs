using BusinessLayer.BusinessObject;
using Microsoft.AspNet.SignalR;
using OnlineAuction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OnlineAuction
{
    public class ChatHub : Hub  //хаб-концентратор
    {
        public static List<AccountVM> Users = new List<AccountVM>();
        public static AccountVM AccountVM { get; set; }

        public void Hello()
        {
            Clients.All.hello();
        }

        //к данным методам обращ. в клиенте API SignalR
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }
        public void SendById(string connectId, string name, string message)
        {
            //Clients.Client(connectId).addMessage(name, message);
            Clients.Client(connectId).sendById(name, message);
        }
        // Подключение нового пользователя
        public void Connect(string userName)
        {
            var id = Context.ConnectionId;
            //если имеющ. юзеры не имеют такого подключ. - добав. нов. юзера
            if (!Users.Any(x => x.ConnectionId == id)) {
                AccountVM.ConnectionId = id;
                Users.Add(AccountVM);

                // подключить текущего пользователю
                Clients.Caller.onConnected(id, userName, Users);

                // Посылаем сообщение всем пользователям, кроме текущего
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
                Clients.All.onUserDisconnected(id, item.FullName);
            }

            return base.OnDisconnected(stopCalled);
        }

        public async Task JoinRoomAsync(string roomName)
        {
            await Groups.Add(Context.ConnectionId, roomName);
            Clients.Group(roomName).addChatMessage(Context.User.Identity.Name + " joined.");
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.Remove(Context.ConnectionId, roomName);
        }
    }
}