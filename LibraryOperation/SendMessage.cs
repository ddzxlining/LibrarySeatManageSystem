using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using Wechat;

namespace LibraryOperation
{
    class SendMessage
    {
        private static string token = string.Empty;

        //获取操作微信公众平台需要的access_token信息。测试阶段，不必考虑安全性问题，直接在本地卡机上获取token即可。
        //等正式运行时在服务端获取token，然后存在数据库中。避免出现过多调用access_token而引起的次数超过微信限制或者其他安全问题。

        /// <summary>
        ///在Redis服务器中，AccessToken的有效期设置为6000秒，而微信服务器获取到的AccessToken有效期为7200秒，
        ///这样可以有效避免正在发送消息呢，AccessToken过期导致的发送消息失败,尝试从redis数据库中获取微信AccessToken，
        ///如果获取到的值为null，那么本地请求微信服务器获取一个AccessToken，然后更新Redis中的AccessToken.
        /// </summary>
        public SendMessage()
        {
            RedisHelper redis = new RedisHelper();
            token=redis.GetString("accessToken");
            if (token == null)
            {
                WebClient client = new WebClient();
                string str = client.DownloadString(new Uri("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wx857e6ba0e902375f&secret=b0cf61ae49413c6235b2a9715fa4bc74"));
                JObject res = JObject.Parse(str);
                token = MyConvert.toString(res["access_token"]);
                redis.SetString("accessToken", token, 6000);
            }          
        }        

        /// <summary>
        /// 发送座位到期消息，使用用户的微信OpenID，等信息。模板ID请查看微信公众平台的管理页面。
        /// </summary>
        /// <param name="no">学号</param>
        /// <param name="openid">微信OpenID</param>
        /// <param name="room">自习室名称</param>
        /// <param name="seatno">座位号</param>
        /// <param name="remain">座位剩余时间</param>
        /// <param name="deadline">座位截止时间</param>
        /// <returns>发送消息是否成功</returns>
        public bool SendSeatOutDateMessage(string no,string openid, string room, string seatno, string remain, string deadline)
        {
            if (openid.Length > 10)
            {
                WeChatMessageUtil wcm = new WeChatMessageUtil();
                Dictionary<string, string> title = new Dictionary<string, string>();
                title.Add("touser", openid);
                title.Add("template_id", "MjMhCvWKW6eCnVtCxXCrKH7ca6dZDdLSG4AzQgX1y1I");
                title.Add("topcolor", "#FF0000");
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("first", no + "\n");
                data.Add("keyword1", room + "\n");
                data.Add("keyword2", seatno + "\n");
                data.Add("keyword3", remain + "分钟\n");
                data.Add("keyword4", deadline + "\n");
                string res = wcm.SendMessage(token, "https://api.weixin.qq.com/cgi-bin/message/template/send", Method.POST, title, data);
                if (res.ToLower() == "ok")
                    return true;
                else
                    return false;
            }           
            else
                return false;
        }

        /// <summary>
        /// 发送选择座位成功消息。
        /// </summary>
        /// <param name="no">学号</param>
        /// <param name="openid">微信OpenID</param>
        /// <param name="room">自习室名称</param>
        /// <param name="seatno">座位号</param>
        /// <param name="time1">选座时间</param>
        /// <param name="time2">座位到期时间</param>
        /// <returns>发送消息是否成功</returns>
        public bool SendSelectSeatSuccessMessage(string no,string openid, string room, string seatno, string time1, string time2)
        {
            if (openid.Length > 10)
            {
                WeChatMessageUtil wcm = new WeChatMessageUtil();
                Dictionary<string, string> title = new Dictionary<string, string>();
                title.Add("touser", openid);
                title.Add("template_id", "fpDqDfWo1zsRwT33H-3kLIzg1OPRxzy75105DrTgY4s");
                title.Add("topcolor", "#FF0000");
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("first", no + "\n");
                data.Add("keyword1", room + "\n");
                data.Add("keyword2", seatno + "\n");
                data.Add("keyword3", time1 + "\n");
                data.Add("keyword4", time2 + "\n");
                string res = wcm.SendMessage(token, "https://api.weixin.qq.com/cgi-bin/message/template/send", Method.POST, title, data);
                if (res.ToLower() == "ok")
                    return true;
                else
                    return false;
            }
            else
                return false;            
        }

        /// <summary>
        /// 发送预约座位成功消息。
        /// </summary>
        /// <param name="no">学号</param>
        /// <param name="openid">微信OpenID</param>
        /// <param name="room">自习室名称</param>
        /// <param name="seatno">座位号</param>
        /// <returns>发送消息是否成功</returns>
        public  bool SendOrderSeatSuccessMessage(string no, string openid, string room, string seatno)
        {
            if (openid.Length > 10)
            {
                WeChatMessageUtil wcm = new WeChatMessageUtil();
                Dictionary<string, string> title = new Dictionary<string, string>();
                title.Add("touser", openid);
                title.Add("template_id", "Xh44vEGShQuwIdsb_JOK2WPyiEICwEuWSNPgErX0gdo");
                title.Add("topcolor", "#FF0000");
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("first", no + "\n");
                data.Add("keyword1", room + "\n");
                data.Add("keyword2", seatno + "\n");
                string res = wcm.SendMessage(token, "https://api.weixin.qq.com/cgi-bin/message/template/send", Method.POST, title, data);
                if (res.ToLower() == "ok")
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
    }
}
