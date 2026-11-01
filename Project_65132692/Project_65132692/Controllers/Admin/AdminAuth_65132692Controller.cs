using Project_65132692.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_65132692.Controllers.Admin
{
    public class AdminAuth_65132692Controller : Controller
    {
        // Khởi tạo DB Context
        private readonly QuanLyBanQuanAo_65132692Entities db = new QuanLyBanQuanAo_65132692Entities();

        // --- 1. ĐĂNG NHẬP (LOGIN) ---

        // GET: Hiển thị form đăng nhập
        public ActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì đá về trang quản lý luôn, không hiện form login nữa
            if (Session["NhanVien"] != null)
            {
                var nv = Session["NhanVien"] as NhanVien;
                if (nv.Role == "admin")
                {
                    return RedirectToAction("Index", "NhanViens_65132692");
                }
                else
                {
                    return RedirectToAction("Index", "DonHangs_65132692");
                }
            }
            return View();
        }

        // POST: Xử lý đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập tài khoản và mật khẩu!";
                return View();
            }

            // 2. Tìm nhân viên trong DB
            // Lưu ý: Trong thực tế password nên được mã hóa (ví dụ: MD5/SHA256)
            var nv = db.NhanViens.FirstOrDefault(x => x.Username == username && x.Password == password);

            if (nv != null)
            {
                // 3. Kiểm tra trạng thái tài khoản (Role = null nghĩa là đã bị khóa/xóa mềm)
                if (string.IsNullOrEmpty(nv.Role))
                {
                    ViewBag.Error = "Tài khoản này đã bị vô hiệu hóa! Vui lòng liên hệ Quản lý.";
                    return View();
                }

                // 4. Đăng nhập thành công -> Lưu session
                Session["NhanVien"] = nv;

                // 5. Điều hướng dựa trên quyền hạn
                if (nv.Role == "admin")
                {
                    // QUAN TRỌNG: Chuyển hướng sang Controller Quản lý Nhân viên
                    return RedirectToAction("Index", "NhanViens_65132692");
                }
                else
                {
                    // Nhân viên thường thì vào trang Đơn hàng
                    return RedirectToAction("Index", "DonHangs_65132692");
                }
            }
            else
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
                return View();
            }
        }

        // --- 2. ĐĂNG XUẤT (LOGOUT) ---
        public ActionResult Logout()
        { 
            // Xóa session
            Session.Remove("NhanVien");
            Session.Clear(); // Xóa sạch mọi session khác nếu có

            // Quay về trang đăng nhập
            return RedirectToAction("Login");
        }

        // Giải phóng tài nguyên DB khi Controller đóng
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