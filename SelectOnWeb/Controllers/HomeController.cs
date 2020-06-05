using LibraryOperation;
using Microsoft.AspNet.Identity.Owin;
using SelectOnWeb.Infrastructure;
using SelectOnWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SelectOnWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        DateTime deadline = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 30, 0); //预约截止时间，每天早上7:30以前前一天预约的座位保留。
        int during = 2;//选择座位有效期，为了方便演示，设置为1-2分钟。
        SelectSeat _ss = new SelectSeat();
        db_libraryEntities db = new db_libraryEntities();
        [AllowAnonymous]
        public ActionResult Index()
        {
            IEnumerable<tb_room> model= db.tb_room.ToList();
            return View(model);
        }
        [HttpGet]
        public ActionResult Select(int room)
        {
            //_os.SetRoomSeats(room);
            ViewBag.room = room;           
            var res = db.tb_seat.Where(p => p.room == room).ToList();
            var count = from item in db.tb_room.ToList() select item;
            ViewBag.curroom = count.Where(m => m.no == room).FirstOrDefault();
            ViewData.Add("info", count);
            return View(res);
        }
        [HttpPost]
        public async Task<string> Select(int room, int id)
        {          
            var user = await HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByNameAsync(User.Identity.Name);
            string Sno=user.UserName;
            bool success = false;
            int seatno = id;
            var now = DateTime.Now;                      
            if (id == -1)
            {
                if (now <= deadline)//如果在预约有效时间内，尝试分配预约座位。
                {
                    _ss.IsBookSeat(room, Sno, now, during, out seatno, out success);
                }
                else
                {
                    _ss.RandomSelectSeat(room, Sno, now, during, out seatno, out success);
                }
            }
            else
                _ss.CustomSeatSelected(room, Sno, now, during, ref seatno, out success);          
            string result = string.Empty;
            if (success)
                result = "{\"status\":\"success\",\"seat\":\""+seatno+"\"}";
            else
                result = "{\"status\":\"fail\"}";
            return result;
        }
        [HttpGet]
        [BookTimeFilter(7,0)]
        public ActionResult Order(int room)
        {
                ViewBag.room = room;
                ViewBag.curcount = db.tb_seat.Count(p => p.available == true && p.room == room);
                var res = db.tb_seat.Where(p => p.room == room).ToList();
                var count = from item in db.tb_room.ToList() select item;
                ViewBag.curroom = count.Where(m => m.no == room).FirstOrDefault();
                ViewData.Add("info", count);
                return View(res);          
        }
        [HttpPost]
        public async Task<string> Order(int room,int id)
        {
            var user = await HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByNameAsync(User.Identity.Name);
            string Sno = user.UserName;
            bool success = false;
            int seatno = id;
            if (id == -1)
                _ss.RandomOrderSeat(room, Sno,out seatno, out success);
            else
                _ss.CustomOrderSeat(room, Sno, ref seatno, out success);
            string result = string.Empty;
            if (success)
                result = "{\"status\":\"success\",\"seat\":\"" + seatno + "\"}";
            else
                result = "{\"status\":\"fail\"}";
            return result;
        }
        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "本软件有李宁独自完成.";

            return View();
        }
        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "请联系13206653816.";

            return View();
        }
    }
}