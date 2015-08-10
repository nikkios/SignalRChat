using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Services.Description;
using Microsoft.AspNet.SignalR;
using SignalRChat.Models;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        private Storage Storage;

        public ChatHub()
        {
            Storage = new Storage();
        }

        public void Send(string name, string text)
        {
            if (text == "CLEAR_ALL")
            {
                Storage.Clear();
                return;
            }

            var id = Context.ConnectionId;

            var msg = new Models.Message
            {
                Guid = Guid.NewGuid(),
                UserGuid = id,
                UserName = name,
                Text = text, 
                Time = DateTime.Now
            };

            Storage.Save(msg);
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(msg);
        }

        public void Login(string title)
        {
            var id = Context.ConnectionId;

            var user = new User()
            {
                Id = id,
                Title = title
            };

            var messages = Storage.Load();
            Clients.Caller.SetChatHistory(messages);

            var users = Storage.AddUser(user);
            Clients.All.UpdateUserList(users);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var id = Context.ConnectionId;
            var users = Storage.RemoveUser(id);

            if (users != null)
                Clients.All.UpdateUserList(users);

            return base.OnDisconnected(stopCalled);
        }
    }
}