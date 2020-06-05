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
    public  class OperationSeat
    { 
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
        
        private string seat_no2tableseat(int seat_no)
        {
            int table = seat_no / 4 + 1;
            int seat = seat_no % 4;
            string str = string.Format("{0}{1}", table, ((Seat)seat).ToString());
            return str;
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
        public void SendSeatOutdateMessage(int roomNum,string roomName, int time,DateTime now)
        {
            string[] sql = { "select t1.no,t1.seat_no,t1.time_end,t2.wechat from tb_seat_student as t1,tb_student as t2 where t1.room=@room and t1.time_end<@nw and t1.isoutdate=0 and t1.issend_"+time+"_message=0 and t1.no=t2.no",
                                    "update tb_seat_student set issend_"+time+"_message=1 where room=@room and time_end<@nw and isoutdate=0 and issend_" + time + "_message=0"};
            SqlParameter[] sp1 =
            {
                new SqlParameter("@room",roomNum),
                new SqlParameter("@nw",now.AddMinutes(time))
            };
            DataTable dt = SqlServerHelper.ExecuteDataTable(sql[0], sp1);
            SendMessage sd = new SendMessage();
            //发送消息
            foreach (DataRow item in dt.Rows)
            {
                sd.SendSeatOutDateMessage(item[0].ToString(),MyConvert.toString(item[3]), roomName, seat_no2tableseat( MyConvert.toInt( item[1])), time.ToString(),MyConvert.toString(item[2]));
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