using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SelectOnWeb.Models;

namespace SelectOnWeb.Areas.Admin.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        private db_libraryEntities db = new db_libraryEntities();

        // GET: Admin/Room
        public ActionResult Index()
        {
            return View(db.tb_room.ToList());
        }

        // GET: Admin/Room/Details/5
        public ActionResult Details(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_room tb_room = db.tb_room.Find(id);
            if (tb_room == null)
            {
                return HttpNotFound();
            }
            return View(tb_room);
        }

        // GET: Admin/Room/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Room/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "no,name,tables,cur,after15min")] tb_room tb_room)
        {
            if (ModelState.IsValid)
            {
                db.tb_room.Add(tb_room);
                for (int i = 0; i < tb_room.tables * 4; i++)
                {
                    tb_seat seat = new tb_seat() { no = i, anyone = false, available = true, room = tb_room.no, seat = (short)(i % 4), desk = i / 4 };
                    db.tb_seat.Add(seat);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tb_room);
        }

        // GET: Admin/Room/Edit/5
        public ActionResult Edit(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_room tb_room = db.tb_room.Find(id);
            if (tb_room == null)
            {
                return HttpNotFound();
            }
            return View(tb_room);
        }

        // POST: Admin/Room/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "no,name,tables,cur,after15min")] tb_room tb_room)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_room).State = EntityState.Modified;
                db.tb_seat_student.RemoveRange(db.tb_seat_student.Where(m => m.room == tb_room.no));
                db.tb_seat.RemoveRange(db.tb_seat.Where(m => m.room == tb_room.no));
                for(int i = 0; i < tb_room.tables * 4; i++)
                {
                    tb_seat seat = new tb_seat() { no = i, anyone = false, available = true, room = tb_room.no, seat =(short) (i % 4), desk = i / 4 };
                    db.tb_seat.Add(seat);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tb_room);
        }

        // GET: Admin/Room/Delete/5
        public ActionResult Delete(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_room tb_room = db.tb_room.Find(id);
            if (tb_room == null)
            {
                return HttpNotFound();
            }
            return View(tb_room);
        }

        // POST: Admin/Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(short id)
        {
            tb_room tb_room = db.tb_room.Find(id);
            db.tb_room.Remove(tb_room);
            db.tb_seat.RemoveRange(db.tb_seat.Where(m => m.room == id));
            db.tb_seat_student.RemoveRange(db.tb_seat_student.Where(m => m.room == tb_room.no));
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Reset(short id)
        {
            tb_room room = db.tb_room.Find(id);
            room.cur = room.total;
            room.after15min = room.total;
            room.book = room.total;
            db.Entry(room).State = EntityState.Modified;
            db.Database.ExecuteSqlCommand("update tb_seat set anyone=0,available=1 where room=@p0",id);
            db.Database.ExecuteSqlCommand("delete from tb_seat_student where room=@p0", id);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
