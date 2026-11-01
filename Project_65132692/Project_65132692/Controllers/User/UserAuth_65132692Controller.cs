using Project_65132692.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography; // Thư viện để mã hóa mật khẩu
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Project_65132692.Controllers.User // Hoặc bỏ .User nếu bạn để file ở ngoài
{
    public class UserAuth_65132692Controller : Controller
    {
        private readonly QuanLyBanQuanAo_65132692Entities db = new QuanLyBanQuanAo_65132692Entities();

        // --- 1. ĐĂNG KÝ (REGISTER) ---

        // GET: Hiển thị form đăng ký
        public ActionResult Register()
        {
            return View();
        }

        // POST: Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(KhachHang kh, string xacNhanMatKhau)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem Username đã tồn tại chưa
                var checkUser = db.KhachHangs.FirstOrDefault(k => k.Username == kh.Username);
                if (checkUser != null)
                {
                    ViewBag.Error = "Tên đăng nhập này đã được sử dụng!";
                    return View(kh);
                }

                // Kiểm tra mật khẩu xác nhận
                if (kh.Password != xacNhanMatKhau)
                {
                    ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                    return View(kh);
                }

                

                // Lưu vào CSDL
                db.KhachHangs.Add(kh);
                db.SaveChanges();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(kh);
        }

        // --- 2. ĐĂNG NHẬP (LOGIN) ---

        // GET: Hiển thị form đăng nhập
        public ActionResult Login()
        {
            return View();
        }

        // POST: Xử lý đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            

            // Tìm khách hàng trong DB
            var kh = db.KhachHangs.FirstOrDefault(k => k.Username == username && k.Password == password);

            if (kh != null)
            {
                // Đăng nhập thành công -> Lưu vào Session
                Session["KhachHang"] = kh;

                // Chuyển hướng về trang chủ Shop
                return RedirectToAction("Index", "Shop_65132692");
            }
            else
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }
        }

        // --- 3. ĐĂNG XUẤT (LOGOUT) ---
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa hết session
            return RedirectToAction("Login"); // Quay về trang đăng nhập
        }

        // --- HÀM MÃ HÓA MD5 (Helper) ---
        // Mục đích: Biến đổi "123456" thành chuỗi ký tự loằng ngoằng để bảo mật
        private string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");
            }
            return byte2String;
        }
        // --- PHẦN LỊCH SỬ ĐƠN HÀNG ---

        // 1. Xem danh sách các đơn đã mua
        public ActionResult LichSuDonHang()
        {
            // Kiểm tra đã đăng nhập chưa
            if (Session["KhachHang"] == null)
            {
                return RedirectToAction("DangNhap"); // Sửa lại tên hàm đăng nhập của bạn nếu khác
            }

            var kh = (Project_65132692.Models.KhachHang)Session["KhachHang"];

            // Lấy danh sách đơn hàng của khách này, đơn mới nhất lên đầu
            var listDonHang = db.DonHangs
                                .Where(n => n.MaKhachHang == kh.MaKhachHang)
                                .OrderByDescending(n => n.NgayDat)
                                .ToList();

            return View(listDonHang);
        }

        // 2. Xem chi tiết 1 đơn hàng cụ thể
        public ActionResult ChiTietDonHang(int id)
        {
            if (Session["KhachHang"] == null) return RedirectToAction("DangNhap");
            var kh = (Project_65132692.Models.KhachHang)Session["KhachHang"];

            // Tìm đơn hàng (Phải đúng ID và đúng là của khách này mới cho xem)
            var donHang = db.DonHangs.SingleOrDefault(n => n.MaDonHang == id && n.MaKhachHang == kh.MaKhachHang);

            if (donHang == null) return RedirectToAction("LichSuDonHang");

            return View(donHang);
        }

        // 3. Hủy đơn hàng (Chỉ hủy được khi chưa giao)
        public ActionResult HuyDonHang(int id)
        {
            if (Session["KhachHang"] == null) return RedirectToAction("DangNhap");
            var kh = (Project_65132692.Models.KhachHang)Session["KhachHang"];

            var donHang = db.DonHangs.SingleOrDefault(n => n.MaDonHang == id && n.MaKhachHang == kh.MaKhachHang);

            if (donHang != null && donHang.TrangThai == "Chờ xác nhận")
            {
                donHang.TrangThai = "Đã hủy";
                db.SaveChanges(); // Lưu lại, Trigger trong SQL sẽ tự trả hàng về kho
            }

            return RedirectToAction("LichSuDonHang");
        }
    }
}