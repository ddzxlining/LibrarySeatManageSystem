using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SelectOnWeb.Infrastructure
{
    public class BookTimeFilter: FilterAttribute, IActionFilter
    {
        private int hour;
        private int minute;
        public BookTimeFilter(int hour,int minute)
        {
            this.hour = hour;
            this.minute = minute;
        }
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            if (now < start)
            {
                ViewDataDictionary data = new ViewDataDictionary();
                data.Add("errortitle", "无法显示");
                data.Add("errorcontext", "对不起，当前时间段暂不提供座位预约服务");
                filterContext.Result = new ViewResult
                {
                    ViewData = data,
                    ViewName = "Error"
                };
            }                          
        }
    }
}