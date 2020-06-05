using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SelectOnWeb.Controllers;
using SelectOnWeb.Models;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            AccountController controller = new AccountController();
            try
            {
                DataTable user = GetUsers();
               foreach(DataRow item in user.Rows)
                {
                    string sno = (string)item[0];
                    string email = sno + "@163.com";
                    string password = sno.Substring(1);
                    string name = (string)item[1];
                    RegisterViewModel model = new RegisterViewModel() { Email = email, Password = password, ConfirmPassword = password, Name = name, Sno = sno };
                    controller.Registers(model);
                }
                                
            }
            catch (Exception e)
            {
                string str = e.Message;
            }
        }
        public DataTable GetUsers()
        {
            DataTable dt = new DataTable();
            string connStr = "data source=123.206.26.20;initial catalog=db_library;user=sa;password=@0812wgczysyefd;";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlDataAdapter sda = new SqlDataAdapter("select no,name from tb_student", conn);
                sda.Fill(dt);
            }
            return dt;
        }
    }
}
