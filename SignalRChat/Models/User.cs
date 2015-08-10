using System.Collections.Generic;

namespace SignalRChat.Models
{

    public class User
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public bool Removed { get; set; }
    }
}