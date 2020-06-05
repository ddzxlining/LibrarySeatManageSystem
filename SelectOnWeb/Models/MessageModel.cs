using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SelectOnWeb.Models
{
    public class MessageModel
    {
        public string Content { get; set; }
        public string CreateTime { get; set; }
        public string Description { get; set; }
        public string Event { get; set; }
        public string EventKey { get; set; }
        public string Format { get; set; }
        public string FromUserName { get; set; }
        public int Id { get; set; }
        public string Label { get; set; }
        public string Latitude { get; set; }
        public string Location_X { get; set; }
        public string Location_Y { get; set; }
        public string Longitude { get; set; }
        public string MediaId { get; set; }
        public string MsgId { get; set; }
        public string MsgType { get; set; }
        public string PicUrl { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }
        public string ThumbMediaId { get; set; }
        public string Ticket { get; set; }
        public string Title { get; set; }
        public string ToUserName { get; set; }
        public string Url { get; set; }
    }
}