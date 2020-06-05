using System;
using System.Data.SqlClient;

namespace LibraryOperation
{
    public class SelectSeat
    {
        /// <summary>
        /// 给定一个学号，在学生信息表中找到他的姓名并且返回
        /// </summary>
        /// <param name="str">学号字符串</param>
        /// <returns>返回该学号对应的姓名</returns>
        internal string FindName(string str)
        {
            string sql = "select name from tb_student where no='" + str + "'";
            return (string)SqlServerHelper.ExecuteSclar(sql);
        }
        internal string FindNo(string str)
        {
            string sql= "select no from tb_student where card='" + str + "'";
            return (string)SqlServerHelper.ExecuteSclar(sql);
        }

        /// <summary>
        /// 随机分配座位的业务流程
        /// </summary>
        /// <param name="roomNum">自习室号码</param>
        /// <param name="sno">学号</param>
        /// <param name="seatnum">返回分配的座位号，当success为false时，该值没有意义</param>
        /// <param name="success">是否成功</param>
        public void RandomSelectSeat(int roomNum, string sno, DateTime start, int during, out int seatnum, out bool success)
        {
            seatnum = -1; //赋默认值，以防特殊情况出现失败。
            SqlServerHelper.ExeProc_Select_Seat_Random(roomNum,sno,start,during, out seatnum, out success);
        }
        /// <summary>
        /// 自定义分配座位的业务流程
        /// </summary>
        /// <param name="roomNum">自习室号码</param>
        /// <param name="sno">学号</param>
        /// <param name="seatnum">当前选择的座位号，当success为false时，该值没有意义</param>
        /// <param name="success">是否成功</param>
        public void CustomSeatSelected(int roomNum, string sno, DateTime start, int during, ref int seatnum, out bool success)
        {
            SqlServerHelper.ExePorc_Select_Seat_Custom(roomNum,sno,start,during,ref seatnum,out success);
        }
        public void RandomOrderSeat(int roomNum,string sno,out int seatnum,out bool success)
        {
            seatnum = -1;
            SqlServerHelper.ExeProc_Order_Seat_Random(roomNum, sno, out seatnum, out success);
        }

        public void IsBookSeat(int room, string sno, DateTime now, int during, out int seatno, out bool success)
        {
            SqlServerHelper.IsBookSeat(room, sno, now, during, out seatno, out success);
        }
        public void CustomOrderSeat(int roomNum,string sno,ref int seatnum,out bool success)
        {
            SqlServerHelper.ExePorc_Order_Seat_Custom(roomNum, sno, ref seatnum, out success);
        }     
    }
}

