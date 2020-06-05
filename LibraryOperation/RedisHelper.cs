using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryOperation
{
    public class RedisHelper
    {
        private IDatabase db;        
        public RedisHelper()
        {
            if (db == null)
            {
                string address = ConfigurationManager.AppSettings["RedisServer"];
                if (address == null || string.IsNullOrWhiteSpace(address.ToString()))
                    throw new ApplicationException("配置文件中未找到有效的Redis服务器！");
                ConnectionMultiplexer conn = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(address));
                db = conn.GetDatabase();
            }  
        }
       public string GetString(string key)
        {
           return  db.StringGet(key);
        }
        public bool SetString(string key,string value,double timeout)
        {
            return db.StringSet(key, value,TimeSpan.FromSeconds(timeout));
        }
    }
}
