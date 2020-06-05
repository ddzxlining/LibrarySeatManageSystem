using System;
using System.Collections.Generic;

namespace Wechat
{
    class Program
    {
        public static string access_token = "4ZQGRpiuFJqJZKfkGsdxfpbtEA-Wz6-OfrGU_aeAECEczlcD1c_gR8oeb2QIseT-UjaytWNqnTgk5FcQ9VNVeIUyEVRg5QVZfCYihFHoHmvoKSEl1uOf2Cz7x0rCw4QoBOTiAAAOBG";
        static void Main(string[] args)
        {
            string templateId= "_94xAAW-4o26JtMEchC5VxCNUu3eRTITtnKUuJrJX-M";
            string touser = "o2yoS0QgCAF0uN1IMbugTkSnLFAQ";          
            string accessToken =DataBaseHelper.Get("accessToken");
            string[] context = { "A19140527","二楼自习室","456D","30分钟","9：40"};
            string formId = DataBaseHelper.ListLeftPop("A19140527");
            send2(formId, accessToken, touser, templateId, context);
        }
        public static void send1()
        {
            WeChatMessage wcm = new WeChatMessage();
            Dictionary<string, string> title = new Dictionary<string, string>();
            title.Add("touser", "oeWZDvymOiKK1FZT8d54seRnQbYw");
            title.Add("template_id", "g0QUjurQbbjVMqHyVGHUXrXqF0NM7o1V2R_3FH-ORmE");
            title.Add("topcolor", "#FF0000");
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("first", "A19140527\n");
            data.Add("keyword1", "二楼自习室\n");
            data.Add("keyword2", "650C\n");
            data.Add("keyword3", "30分钟\n");
            data.Add("keyword4", "16:35\n");
            string res = wcm.SendMessage(access_token, "https://api.weixin.qq.com/cgi-bin/message/template/send", Method.POST, title, data);
            if (res.ToLower() == "ok")
                Console.WriteLine("发送成功！");
            Console.ReadKey();
        }
        public static bool send2(string formId,string accessToken,string touser,string templateId,string[] context)
        {
            WeChatMessage wcm = new WeChatMessage();
            Dictionary<string, string> title = new Dictionary<string, string>();
            title.Add("touser", touser);
            title.Add("template_id", templateId);
            title.Add("form_id", formId);
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("keyword1",context[0]);
            data.Add("keyword2", context[1]);
            data.Add("keyword3", context[2]);
            data.Add("keyword4", context[3]);
            data.Add("keyword5", context[4]);
            string res = wcm.SendMessage(accessToken, "https://api.weixin.qq.com/cgi-bin/message/wxopen/template/send", Method.POST, title, data);
            if (res.ToLower() == "ok")
                return true;
            else
                return false;
        }
    }
}
