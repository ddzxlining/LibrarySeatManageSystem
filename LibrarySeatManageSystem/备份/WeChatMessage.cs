using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Wechat
{
    class WeChatMessage
    {
       public string SendMessage(string access_token,string url,Method method, Dictionary<string, string> title, Dictionary<string, string> data=null)
        {
            string requesturl = url+"?access_token=" + access_token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requesturl);
            request.Method = method.ToString();
            string str = MergeMessage(title, data);
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            request.ContentLength = buffer.Length;
            Stream m = request.GetRequestStream();
            m.Write(buffer, 0, buffer.Length);
            m.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            JObject result = JObject.Parse(sr.ReadToEnd());
            string res = (string)result["errmsg"];
            return res;
        }
        private string MergeMessage(Dictionary<string,string> title,Dictionary<string,string> _data=null)
        {
            JObject message = new JObject();
            foreach (var item in title)
                message.Add(item.Key, item.Value);
            if(_data.Count>0)
            {
                JObject data = new JObject();
                foreach (var item in _data)
                {
                    JObject temp = new JObject();
                    temp.Add("value", item.Value);
                    temp.Add("color", "#173177");
                    data.Add(item.Key, temp);
                }
                message.Add("data", data);
            }
            return message.ToString();         
        }
    }
}