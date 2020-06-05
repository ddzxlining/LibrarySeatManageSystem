using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Wechat;

namespace LibraryOperation
{
    public  class  OperationSeat
    {
        private static string token;
        private static DateTime gettime;
        public delegate void RemoteSetColor(int seatnum, Brush color);
        /// <summary>
        /// 设置座位状态轮询的执行函数。每个自习室轮询自己的内容。
        /// </summary>
        /// <param name="roomNum">自习室编号</param>
        /// <param name="SetColor">设置颜色</param>
        /// <param name="now">当前时间</param>
        public void SetIsOutDateOnWeb(int roomNum,DateTime now)
        {
            SqlServerHelper.ExeProc_SetOutdate(roomNum, DateTime.Now);
        }
        public void SetIsOutDateClient(int roomNum,RemoteSetColor SetColor,DateTime now)
        {
            // string nw = now.ToString().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            DateTime nw = DateTime.Now;
            SqlServerHelper.ExeProc_SetOutdate(roomNum, nw);
            UpdateSeat(roomNum, SetColor);
        }
        public void UpdateSeat(int roomNum,RemoteSetColor SetColor)
        {
            DataTable dt = SqlServerHelper.ExeProc_GetOutdate(roomNum);
            //挨个修改颜色；
            List<int> list = new List<int>();
            foreach (DataRow item in dt.Rows)
            {
                list.Add((int)item[0]);
            }
            for(int i = 0; i < int.Parse(ConfigurationManager.AppSettings["tableCount"]) * 4; i++)
            {
                if(list.Contains(i))
                    SetColor(i, Brushes.Green);
                else
                    SetColor(i, Brushes.Red);
            }         
        }
        public void ReleaseSeat(int roomNum,int seatno,string sno)
        {
            string sql = @"update tb_seat set anyone=0 where no='"+seatno+"' and room='"+roomNum+"';" +
                "delete from tb_seat_student where no='"+sno+"';";
            SqlServerHelper.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 返回当前可用座位数。
        /// </summary>
        /// <param name="room">自习室编号</param>
        /// <returns></returns>
        public int GetRedundancyCur(int room)
        {
            return SqlServerHelper.ExeProc_GetRedundancyCur(room);
        }
         /// <summary>
        /// 指定时间阈值可用座位数
        /// </summary>
        /// <param name="room">自习室编号</param>
        /// <param name="time">时间阈值</param>
        /// <param name="now">当前时间</param>
        /// <returns>座位数</returns>
        public int SetRedundancy(int room, int time,DateTime now)
        {
            return SqlServerHelper.ExeProc_GetRedundancyAfter(room, now.AddMinutes(time));
        }
        public void SetRoomSeatsOnWeb(int roomNum)
        {
            DateTime now = DateTime.Now;
            SetIsOutDateOnWeb(roomNum, now);
            int remainCur = GetRedundancyCur(roomNum);
            int remain15min =SetRedundancy(roomNum, 15, now);
            remain15min += remainCur;
            SqlServerHelper.SetRoomSeats(roomNum, remainCur, remain15min);
        }
        public void SetRoomSeatsClient(int roomNum, int remainCur, int remain15min)
        {
            SqlServerHelper.SetRoomSeats(roomNum, remainCur, remain15min);
        }
        internal bool Send(string access_token,string no,string room,string seatno,string time1,string time2)
        {
            WeChatMessage wcm = new WeChatMessage();
            Dictionary<string, string> title = new Dictionary<string, string>();
            title.Add("touser", "oeWZDvymOiKK1FZT8d54seRnQbYw");
            title.Add("template_id", "g0QUjurQbbjVMqHyVGHUXrXqF0NM7o1V2R_3FH-ORmE");
            title.Add("topcolor", "#FF0000");
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("first", no+"\n");
            data.Add("keyword1", room+"\n");
            data.Add("keyword2", seatno+"\n");
            data.Add("keyword3", time1+"\n");
            data.Add("keyword4", time2+"\n");
            string res = wcm.SendMessage(access_token, "https://api.weixin.qq.com/cgi-bin/message/template/send", Method.POST, title, data);
            if (res.ToLower() == "ok")
                return true;
            else
                return false;
        }
        private string seat_no2tableseat(int seat_no)
        {
            int table = seat_no / 4 + 1;
            int seat = seat_no % 4;
            string str = string.Format("{0}{1}", table, ((Seat)seat).ToString());
            return str;
        }
        //获取操作微信公众平台需要的access_token信息。测试阶段，不必考虑安全性问题，直接在本地卡机上获取token即可。
        //等正式运行时在服务端获取token，然后存在数据库中。避免出现过多调用access_token而引起的次数超过微信限制或者其他安全问题。
        public  string GetAccessToken()
        {
            if (token==null||DateTime.Now - gettime > TimeSpan.FromMinutes(110))
            {
                WebClient client = new WebClient();
                string str=client.DownloadString(new Uri("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wx857e6ba0e902375f&secret=b0cf61ae49413c6235b2a9715fa4bc74"));
                JObject res = JObject.Parse(str);
               token=MyConvert.toString( res["access_token"]);
                gettime = DateTime.Now;
            }
            return token;
        }
        public bool IsHasSeat(TextBlock room, string sno,out int seatno)
        {
            seatno = -1;
            string sql = "select seat_no from tb_seat_student where no='"+sno+"' and isoutdate=0;";
            SqlParameter[] sp = { };
            var res=SqlServerHelper.ExecuteSclar(sql, sp);
            if (res != null) seatno = (int)res;
            return res != null;
        }
        /// <summary>
        /// 系统启动时扫描一次本自习室座位的使用情况。
        /// </summary>
        /// <param name="room"></param>
        /// <param name="SetColor"></param>
        public void Scan(int room, RemoteSetColor SetColor)
        {
            string sql = "select no from tb_seat where anyone=1";
            DataTable dt = SqlServerHelper.ExecuteDataTable(sql);
            foreach (DataRow item in dt.Rows)
            {
                SetColor((int)item[0], Brushes.Red);
            }
        }
        internal void SendMessage(int roomNum, int time,DateTime now)
        {
            string[] sql = { "select t1.no,t2.name,t1.seat_no,t1.time_end,t2.wechat from tb_seat_student as t1,tb_student as t2 where t1.room=@room and t1.time_end<@nw and t1.isoutdate=0 and t1.issend_"+time+"_message=0 and t1.no=t2.no",
                                    "update tb_seat_student set issend_"+time+"_message=1 where room=@room and time_end<@nw and isoutdate=0 and issend_" + time + "_message=0"};
            SqlParameter[] sp1 =
            {
                new SqlParameter("@room",roomNum),
                new SqlParameter("@nw",now.AddMinutes(time))
            };
            DataTable dt = SqlServerHelper.ExecuteDataTable(sql[0], sp1);
            string access_token = GetAccessToken();
            //发送消息
            foreach (DataRow item in dt.Rows)
            {
                Send(access_token, item[0].ToString(), roomNum.ToString(), seat_no2tableseat( MyConvert.toInt( item[2])), time.ToString(),MyConvert.toString(item[3]));
            }
            //发送消息结束。
            SqlParameter[] sp2 =
            {
                new SqlParameter("@room",roomNum),
                new SqlParameter("@nw",now.AddMinutes(time))
            };
            SqlServerHelper.ExecuteNonQuery(sql[1], sp2);
        }
    }
}