using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SelectOnWeb.Models
{
    public class RoomViewModel
    {
        public int roomId { get; set; }
        public string roomName { get; set; }
        public int cur { get; set; }
        public int after15min { get; set; }
    }
}