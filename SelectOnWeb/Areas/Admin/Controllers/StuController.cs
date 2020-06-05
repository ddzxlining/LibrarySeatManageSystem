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
    public class StuController : Controller
    {
        private db_libraryEntities db = new db_libraryEntities();

        // GET: Admin/Stu
        public ActionResult Index()
        {
            var tb_student = db.tb_student.Include(t => t.tb_seat_student);
            return View(tb_student.ToList().Take(100));
        }

        // GET: Admin/Stu/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_student tb_student = db.tb_student.Find(id);
            if (tb_student == null)
            {
                return HttpNotFound();
            }
            return View(tb_student);
        }

        // GET: Admin/Stu/Create
        public ActionResult Create()
        {
            ViewBag.no = new SelectList(db.tb_seat_student, "no", "no");
            return View();
        }

        // POST: Admin/Stu/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "no,card,name,grade,wechat,department,college,class,weapp")] tb_student tb_student)
        {
            if (ModelState.IsValid)
            {
                db.tb_student.Add(tb_student);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.no = new SelectList(db.tb_seat_student, "no", "no", tb_student.no);
            return View(tb_student);
        }

        // GET: Admin/Stu/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_student tb_student = db.tb_student.Find(id);
            if (tb_student == null)
            {
                return HttpNotFound();
            }
            ViewBag.no = new SelectList(db.tb_seat_student, "no", "no", tb_student.no);
            return View(tb_student);
        }

        // POST: Admin/Stu/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "no,card,name,grade,wechat,department,college,class,weapp")] tb_student tb_student)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_student).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.no = new SelectList(db.tb_seat_student, "no", "no", tb_student.no);
            return View(tb_student);
        }

        // GET: Admin/Stu/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_student tb_student = db.tb_student.Find(id);
            if (tb_student == null)
            {
                return HttpNotFound();
            }
            return View(tb_student);
        }

        // POST: Admin/Stu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            tb_student tb_student = db.tb_student.Find(id);
            db.tb_student.Remove(tb_student);
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
