using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace LibrarySeatManageSystemAdmin
{
    public class SqlServerHelper
    {
        private static string _connStr = ConfigurationManager.ConnectionStrings["db_library"].ConnectionString;
        //private static string _connStr = "server=(local);database=db_library;user=db_library_wr;password=@0812wgczysyefd";
        public SqlServerHelper(string connStr = null)
        {
           // _connStr = connStr ?? System.Configuration.ConfigurationManager.ConnectionStrings["library"].ToString();
        }
        static SqlServerHelper()
        {
           // _connStr = System.Configuration.ConfigurationManager.ConnectionStrings["library"].ToString();
        }

        public static object ExecuteSclar(string sql, params SqlParameter[] sp)
        {
            object res = null;
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (sp.Count() > 0)
                        cmd.Parameters.AddRange(sp);
                    res = cmd.ExecuteScalar();
                }
            }
            return res;
        }
        public static DataTable ExecuteDataTable(string sql, params SqlParameter[] sp)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                SqlDataAdapter sda = new SqlDataAdapter(sql, conn);
                if (sp != null)
                    sda.SelectCommand.Parameters.AddRange(sp);
                sda.Fill(dt);
            }
            return dt;
        }
        public static int ExecuteNonQuery(string sql,params SqlParameter[] sp)
        {
            int res = -1;
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (sp.Count() > 0)
                        cmd.Parameters.AddRange(sp);
                    res = cmd.ExecuteNonQuery();
                }
            }
            return res;
        }

        #region+执行存储过程
        public static int ExeProc_SetOutdate(int roomNum,DateTime now)
        {
            int lines = -1;
            using (SqlConnection conn=new SqlConnection(_connStr))
            {
                conn.Open();
                using(SqlCommand cmd=new SqlCommand("SetOutDate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@roomNum", roomNum);
                    cmd.Parameters.AddWithValue("@now", now);
                    cmd.UpdatedRowSource = UpdateRowSource.OutputParameters;     
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Bit) { Direction = ParameterDirection.Output,Size=1 });
                    cmd.Parameters.Add(new SqlParameter("@lines", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    string res=cmd.Parameters["@result"].Value.ToString();
                     lines= (int)cmd.Parameters["@lines"].Value;
                }
            }
            return lines;
        }
        public static DataTable ExeProc_GetOutdate(int roomNum)
        {
            DataTable dt = new DataTable();
            using(SqlConnection conn=new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("return_outdate", conn))
                {
                    cmd.Parameters.AddWithValue("@room", roomNum);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    sda.Dispose();
                }
            }
            return dt;
        }
        public static int ExeProc_GetRedundancyCur(int roomNum)
        {
            int count = -1;
            using(SqlConnection conn=new SqlConnection(_connStr))
            {
                conn.Open();
                using(SqlCommand cmd=new SqlCommand("GetRedundancyCur", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@room", roomNum);
                    count = (int)cmd.ExecuteScalar();
                }
            }
            return count;
        }

        internal static void SetRoomSeats(int roomNum, int remainCur, int remain15min)
        {
            string sql = string.Format("update tb_room set cur={0} ,after15min={1} where no={2}", remainCur, remain15min, roomNum);
            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using(SqlCommand cmd=new SqlCommand(sql,conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }catch(Exception)
            {
            }
        }

        public static int ExeProc_GetRedundancyAfter(int roomNum,DateTime time)
        {
            int count = -1;
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GetRedundancyAfter", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@room", roomNum);
                    cmd.Parameters.AddWithValue("@nw", time);
                    count = (int)cmd.ExecuteScalar();
                }
            }
            return count;
        }
        public static void ExeProc_Select_Seat_Random(int roomNum, string sno, DateTime start, int during, out int seatnum, out bool success)
        {
            success = false;
            seatnum = -1;
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select_Seat_Random", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@roomNum", roomNum);
                    cmd.Parameters.AddWithValue("@sno", sno);
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", start.AddMinutes(during));
                    cmd.Parameters.Add(new SqlParameter("@seatnum", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    success = (bool)cmd.Parameters["@status"].Value;
                    seatnum = (int)cmd.Parameters["@seatnum"].Value;
                }
            }
        }
        public static void ExePorc_Select_Seat_Custom(int roomNum, string sno, DateTime start, int during, ref int seatnum, out bool success)
        {
            success = false;
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select_Seat_Custom", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@roomNum", roomNum);
                    cmd.Parameters.AddWithValue("@sno", sno);
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", start.AddMinutes(during));
                    cmd.Parameters.Add(new SqlParameter("@seatnum", SqlDbType.Int) { Value=seatnum,Direction = ParameterDirection.InputOutput });
                    cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    success = (bool)cmd.Parameters["@status"].Value;
                    seatnum = (int)cmd.Parameters["@seatnum"].Value;
                }
            }
        }

        public  static void IsBookSeat(int roomNum,string sno, DateTime start, int during, out int seatnum, out bool success)
        {
            success = false;
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Book_Seat", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@roomNum", roomNum);
                    cmd.Parameters.AddWithValue("@sno", sno);
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", start.AddMinutes(during));
                    cmd.Parameters.Add(new SqlParameter("@seatnum", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    success = (bool)cmd.Parameters["@status"].Value;
                    seatnum = (int)cmd.Parameters["@seatnum"].Value;
                }
            }
        }
        #endregion
    }
}
