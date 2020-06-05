using SelectOnWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

namespace SelectOnWeb
{
    /// <summary>
    /// Wechat 的摘要说明
    /// </summary>
    public class Wechat : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string str = context.Request.HttpMethod.ToUpper();
            if (str == "GET")
            {
                string signature = context.Request.QueryString["signature"];
                string timestamp = context.Request.QueryString["timestamp"];
                string s = context.Request.QueryString["echostr"];
                string nonce = context.Request.QueryString["nonce"];
                if (this.check(signature, timestamp, nonce))
                {
                    HttpContext.Current.Response.Write(s);
                    HttpContext.Current.Response.End();
                }
            }
            else if (str == "POST")
            {
                MessageModels model = new MessageModels();
                string xml = new StreamReader(context.Request.InputStream).ReadToEnd();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                model.FromUserName = doc.DocumentElement.SelectSingleNode("FromUserName").InnerText;
                model.ToUserName = doc.DocumentElement.SelectSingleNode("ToUserName").InnerText;
                model.CreateTime = doc.DocumentElement.SelectSingleNode("CreateTime").InnerText;
                model.MsgId = (doc.DocumentElement.SelectSingleNode("MsgId") != null) ? doc.DocumentElement.SelectSingleNode("MsgId").InnerText : "";
                model.MsgType = doc.DocumentElement.SelectSingleNode("MsgType").InnerText;
                string msgType = model.MsgType;
                switch (msgType)
                {
                    case "text":
                        model.Content = doc.DocumentElement.SelectSingleNode("Content").InnerText;
                        break;
                    case "video":
                    case "shortvideo":
                        model.MediaId = doc.DocumentElement.SelectSingleNode("MediaId").InnerText;
                        model.ThumbMediaId = doc.DocumentElement.SelectSingleNode("ThumbMediaId").InnerText;
                        break;
                    case "voice":
                        model.MediaId = doc.DocumentElement.SelectSingleNode("MediaId").InnerText;
                        model.Format = doc.DocumentElement.SelectSingleNode("Format").InnerText;
                        break;
                    case "image":
                        model.PicUrl = doc.DocumentElement.SelectSingleNode("PicUrl").InnerText;
                        model.MediaId = doc.DocumentElement.SelectSingleNode("MediaId").InnerText;
                        break;
                    case "link":
                        model.Title = doc.DocumentElement.SelectSingleNode("Title").InnerText;
                        model.Description = doc.DocumentElement.SelectSingleNode("Description").InnerText;
                        model.Url = doc.DocumentElement.SelectSingleNode("Url").InnerText;
                        break;
                    case "location":
                        model.Location_X = doc.DocumentElement.SelectSingleNode("Location_X").InnerText;
                        model.Location_Y = doc.DocumentElement.SelectSingleNode("Location_Y").InnerText;
                        model.Scale = doc.DocumentElement.SelectSingleNode("Scale").InnerText;
                        model.Label = doc.DocumentElement.SelectSingleNode("Label").InnerText;
                        break;
                    case "event":
                        model.Event = doc.DocumentElement.SelectSingleNode("Event").InnerText;
                        string Event = model.Event;
                        switch (Event)
                        {
                            case "SCAN":
                                model.EventKey = doc.DocumentElement.SelectSingleNode("EventKey").InnerText;
                                model.Ticket = (doc.DocumentElement.SelectSingleNode("Ticket") != null) ? doc.DocumentElement.SelectSingleNode("Ticket").InnerText : null;
                                break;
                            case "CLICK":
                                model.EventKey = doc.DocumentElement.SelectSingleNode("EventKey").InnerText;
                                break;
                            case "VIEW":
                                model.EventKey = doc.DocumentElement.SelectSingleNode("EventKey").InnerText;
                                break;
                            case "LOCATION":
                                model.Latitude = doc.DocumentElement.SelectSingleNode("Latitude").InnerText;
                                model.Longitude = doc.DocumentElement.SelectSingleNode("Longitude").InnerText;
                                model.Precision = doc.DocumentElement.SelectSingleNode("Precision").InnerText;
                                break;
                            case "subscribe":
                                model.EventKey = (doc.DocumentElement.SelectSingleNode("EventKey") != null) ? doc.DocumentElement.SelectSingleNode("EventKey").InnerText : null;
                                model.Ticket = (doc.DocumentElement.SelectSingleNode("Ticket") != null) ? doc.DocumentElement.SelectSingleNode("Ticket").InnerText : null;
                                break;
                            case "unsubscribe":
                                break;
                            default:
                                model.EventKey = "所有都未匹配！";
                                break;
                        }
                        break;
                 }
                db_libraryEntities db = new db_libraryEntities();
                db.MessageModels.Add(model);
                db.SaveChanges();
            }      
        }
        private bool check(string signature, string timestamp, string nonce)
        {
            string str = "6798325fb243b8ed895df0faf787bfd2";
            string[] array = new string[] { timestamp, nonce, str };
            Array.Sort<string>(array);
            string s = string.Join("", array);
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            string str3 = BitConverter.ToString(SHA1.Create().ComputeHash(bytes)).Replace("-", "");
            return signature.Equals(str3, StringComparison.OrdinalIgnoreCase);
        }




        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}