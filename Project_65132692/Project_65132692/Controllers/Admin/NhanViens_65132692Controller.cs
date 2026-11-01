using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Project_65132692.Models;

namespace Project_65132692.Controllers.Admin
{
    public class NhanViens_65132692Controller : Controller
    {
        private QuanLyBanQuanAo_65132692Entities db = new QuanLyBanQuanAo_65132692Entities();

        // 1. KIỂM TRA QUYỀN TRƯỚC KHI VÀO BẤT CỨ HÀM NÀO
        // Hàm này chạy trước các action để đỡ phải viết lại code check session
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = Session["NhanVien"] as NhanVien;
            if (session == null || session.Role != "admin")
            {
                // Nếu không phải admin, đá về trang đăng nhập
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new { controller = "AdminAuth_65132692", action = "Login" }));
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: Admin/NhanViens_65132692
        public ActionResult Index()
        {
            return View(db.NhanViens.ToList());
        }

        // GET: Admin/NhanViens_65132692/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/NhanViens_65132692/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "HoTen,SDT,ChucVu,Username,Password,Role")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                // Check trùng username
                if (db.NhanViens.Any(x => x.Username == nhanVien.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(nhanVien);
                }

                // Nếu chưa chọn role thì mặc định staff
                if (string.IsNullOrEmpty(nhanVien.Role)) nhanVien.Role = "staff";

                // (Optional) Hash password ở đây nếu cần

                db.NhanViens.Add(nhanVien);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(nhanVien);
        }

        // GET: Admin/NhanViens_65132692/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null) return HttpNotFound();
            return View(nhanVien);
        }

        // POST: Admin/NhanViens_65132692/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaNhanVien,HoTen,SDT,ChucVu,Username,Role")] NhanVien nhanVien, string NewPassword)
        {
            if (ModelState.IsValid)
            {
                var nvGoc = db.NhanViens.Find(nhanVien.MaNhanVien);

                // Cập nhật thông tin
                nvGoc.HoTen = nhanVien.HoTen;
                nvGoc.SDT = nhanVien.SDT;
                nvGoc.ChucVu = nhanVien.ChucVu;
                nvGoc.Role = nhanVien.Role; // Quan trọng: Cho phép sửa Role để hồi sinh tài khoản

                // Nếu có nhập pass mới thì đổi, không thì thôi
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    nvGoc.Password = NewPassword; // Nhớ Hash nếu cần
                }

                db.Entry(nvGoc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(nhanVien);
        }

        // POST: Xóa (Khóa tài khoản)
        [HttpPost]
        public ActionResult DeleteConfirmed(int id)
        {
            NhanVien nhanVien = db.NhanViens.Find(id);
            // Không cho xóa chính mình
            var currentAdmin = Session["NhanVien"] as NhanVien;
            if (currentAdmin.MaNhanVien == id)
            {
                // Báo lỗi nếu cần
                return RedirectToAction("Index");
            }

            if (nhanVien != null)
            {
                nhanVien.Role = null; // Khóa mềm
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}   