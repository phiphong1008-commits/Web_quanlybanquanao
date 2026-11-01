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
    public class DonHangs_65132692Controller : Controller
    {
        private QuanLyBanQuanAo_65132692Entities db = new QuanLyBanQuanAo_65132692Entities();

        // GET: Danh sách đơn hàng
        // 1. Sửa lại hàm Index (Để Shipper thấy đơn cần đi giao)
        public ActionResult Index()
        {
            if (Session["NhanVien"] == null) return RedirectToAction("Login", "AdminAuth_65132692");
            var nv = (NhanVien)Session["NhanVien"];

            var donHangs = db.DonHangs.Include(d => d.KhachHang).Include(d => d.NhanVien).OrderByDescending(d => d.NgayDat).AsQueryable();

            // Nếu là Shipper: Thấy đơn "Đã xác nhận" (để nhận), "Đang giao" (để chốt), và lịch sử giao
            if (nv.ChucVu == "Nhân viên giao hàng" && nv.Role != "admin")
            {
                donHangs = donHangs.Where(d => d.TrangThai == "Đã xác nhận"
                                            || d.TrangThai == "Đang giao"
                                            || d.TrangThai == "Giao thành công"
                                            || d.TrangThai == "Đã hủy");
            }

            return View(donHangs.ToList());
        }

        // 2. Sửa lại hàm Xử lý trạng thái (Thêm logic Đã xác nhận -> Đang giao)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatTrangThai(int id, string trangThaiMoi)
        {
            if (Session["NhanVien"] == null) return RedirectToAction("Login", "AdminAuth_65132692");
            var nv = (NhanVien)Session["NhanVien"];

            var donHang = db.DonHangs.Find(id);
            if (donHang == null) return HttpNotFound();

            bool isUpdated = false;

            // --- QUYỀN CỦA SALES/ADMIN: Duyệt đơn mới ---
            if (nv.ChucVu == "Nhân viên bán hàng" || nv.ChucVu == "Quản lý" || nv.Role == "admin")
            {
                // Duyệt đơn: Chờ xác nhận -> Đã xác nhận (Chứ KHÔNG chuyển ngay sang Đang giao)
                if (donHang.TrangThai == "Chờ xác nhận" && trangThaiMoi == "Đã xác nhận")
                {
                    donHang.TrangThai = "Đã xác nhận";
                    donHang.MaNhanVien = nv.MaNhanVien; // Lưu người duyệt
                    isUpdated = true;
                }
            }

            // --- QUYỀN CỦA SHIPPER/ADMIN: Đi giao & Chốt đơn ---
            if (nv.ChucVu == "Nhân viên giao hàng" || nv.ChucVu == "Quản lý" || nv.Role == "admin")
            {
                // Giai đoạn 1: Nhận đơn (Đã xác nhận -> Đang giao)
                if (donHang.TrangThai == "Đã xác nhận" && trangThaiMoi == "Đang giao")
                {
                    donHang.TrangThai = "Đang giao";
                    // Nếu cần lưu ai đi giao thì lưu đè vào MaNhanVien hoặc thêm cột MaShipper
                    if (nv.ChucVu == "Nhân viên giao hàng") donHang.MaNhanVien = nv.MaNhanVien;
                    isUpdated = true;
                }

                // Giai đoạn 2: Chốt đơn (Đang giao -> Thành công/Hủy)
                if (donHang.TrangThai == "Đang giao")
                {
                    if (trangThaiMoi == "Giao thành công")
                    {
                        donHang.TrangThai = "Giao thành công";
                        donHang.NgayGiao = DateTime.Now;
                        isUpdated = true;
                    }
                    else if (trangThaiMoi == "Đã hủy")
                    {
                        donHang.TrangThai = "Đã hủy";
                        isUpdated = true;
                    }
                }
            }

            if (isUpdated)
            {
                db.SaveChanges();
                TempData["ThongBao"] = $"Đơn hàng #{id} đã chuyển sang: {trangThaiMoi}";
            }
            else
            {
                TempData["Loi"] = "Trạng thái đơn hàng không hợp lệ hoặc bạn không có quyền!";
            }

            return RedirectToAction("Index");
        }

        // GET: Xem chi tiết (Ai cũng xem được)
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            DonHang donHang = db.DonHangs.Find(id);
            if (donHang == null) return HttpNotFound();
            return View(donHang);
        }

     

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}