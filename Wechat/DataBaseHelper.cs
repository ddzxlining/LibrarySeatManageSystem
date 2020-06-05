using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wechat
{
    public static class DataBaseHelper
    {
        public static ConfigurationOptions config = ConfigurationOptions.Parse("127.0.0.1" + ":" + "6379");
        public static readonly object Locker = new object();
        public static ConnectionMultiplexer redis;
        public static ConnectionMultiplexer Redis
        {
            get
            {
                if (redis == null)
                {
                    lock (Locker)
                    {
                        if(redis==null||!redis.IsConnected)
                        {
                            redis = ConnectionMultiplexer.Connect(config);
                        }
                    }
                }
                return redis;
            }
        }
        public static string ListLeftPop(string key)
        {
            IDatabase db = Redis.GetDatabase();
            return  db.ListLeftPop(key);            
        }
        public static string Get(string key)
        {
            IDatabase db = Redis.GetDatabase();
            return db.StringGet(key);
        }
        
    }
}
