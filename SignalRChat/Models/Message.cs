using System;

namespace SignalRChat.Models
{
    public class Message
    {
        public Guid Guid { get; set; }
        public string Text { get; set; }
        public string UserGuid { get; set; }
        public string UserName { get; set; }
        public DateTime Time { get; set; }
    }
}