using Project_65132692.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_65132692.Controllers.Admin
{
    public class ThongKe_65132692Controller : Controller
    {
        private QuanLyBanQuanAo_65132692Entities db = new QuanLyBanQuanAo_65132692Entities();

        // Kiểm tra quyền Admin
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var nv = Session["NhanVien"] as NhanVien;
            if (nv == null || (nv.ChucVu != "Admin" && nv.ChucVu != "Quản lý"))
            {
                filterContext.Result = RedirectToAction("Index", "DonHang_65132692");
            }
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            // 1. Thống kê 3 ô tổng quan
            // Lưu ý: Dòng này vẫn giữ (decimal?) và ?? 0m để phòng trường hợp chưa có đơn nào trong DB (SQL trả về null)
            ViewBag.TongDoanhThu = db.DonHangs.Where(d => d.TrangThai == "Giao thành công").Sum(d => (decimal?)d.TongTien) ?? 0m;

            ViewBag.DonHangMoi = db.DonHangs.Count(d => d.TrangThai == "Chờ xác nhận");
            ViewBag.TongSanPham = db.SanPhams.Count(s => s.TrangThai == true);

            // 2. Lấy dữ liệu chi tiết
            var dataThongKe = db.DonHangs
                .Where(d => d.TrangThai == "Giao thành công")
                .ToList() // Dữ liệu đã được tải về RAM
                .GroupBy(d => d.NgayDat.Value.ToString("dd/MM/yyyy"))
                .Select(g => new BaoCaoDoanhThu
                {
                    Ngay = g.Key,
                    SoDonHang = g.Count(),

                    // SỬA LỖI Ở ĐÂY: Xóa bỏ "?? 0m" vì TongTien không bao giờ null
                    TongTien = g.Sum(x => x.TongTien)
                })
                .OrderByDescending(x => x.Ngay)
                .ToList();

            return View(dataThongKe);
        }
    }

    public class BaoCaoDoanhThu
    {
        public string Ngay { get; set; }
        public int SoDonHang { get; set; }
        public decimal TongTien { get; set; }
    }
}