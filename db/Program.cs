using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace db
{
    class Program
    {
        static void Main(string[] args)
        {
            WriterStudent();
        }
        #region+初始化座位信息
        public static void WriterSeat()
        {
            string sql = "insert into tb_seat(no, room, desk, seat, available,anyone) values(@no,@room,@desk,@seat,@availabe,@anyone); ";
            string connStr = ConfigurationManager.ConnectionStrings["library"].ToString();
            int auto = 0, auto2 = 1;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                for (int floor = 1; floor <= 7; floor++)
                {
                    for (int i = 0; i < 800; i++)
                    {
                        SqlParameter[] sp =
                         {
                        new SqlParameter("@no",i),
                        new SqlParameter("@room",floor),
                        new SqlParameter("@desk",i/4),
                        new SqlParameter("@seat",i%4),
                        new SqlParameter("@availabe",auto2),
                        new SqlParameter("@anyone",auto)
                    };
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddRange(sp);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                conn.Dispose();
            }
        }
        #endregion
        #region +初始化学生信息
        public static void WriterStudent()
        {
            FileStream fs = new FileStream("D:/信息表.csv", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, Encoding.Default);
            string temp=string.Empty;
            string connStr = ConfigurationManager.ConnectionStrings["library"].ToString();
            using(SqlConnection conn=new SqlConnection(connStr))
            {
                conn.Open();
                while ((temp = sr.ReadLine()) != null)
                {
                    string[] str = temp.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string sql = "insert into tb_student(no,name) values('" + str[0] + "','" + str[1] + "');";
                    using (SqlCommand cmd=new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            

        }
        #endregion
    }
}
