using Project_65132692.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_65132692.Controllers
{
    public class GioHang_65132692Controller : Controller
    {
        private readonly QuanLyBanQuanAo_65132692Entities db = new QuanLyBanQuanAo_65132692Entities();

        // 1. HIỂN THỊ GIỎ HÀNG
        public ActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (Session["KhachHang"] == null)
            {
                return RedirectToAction("Login", "UserAuth_65132692");
            }

            var khach = (KhachHang)Session["KhachHang"];
            // Lấy danh sách sản phẩm trong giỏ của khách này, kèm thông tin biến thể và sản phẩm gốc
            var gioHang = db.GioHangs.Where(g => g.MaKhachHang == khach.MaKhachHang).ToList();

            return View(gioHang);
        }

        // 2. THÊM VÀO GIỎ HÀNG (Nhận dữ liệu từ trang Details)
        [HttpPost]
        public ActionResult ThemVaoGio(int maBienThe, int soLuong)
        {
            // Kiểm tra đăng nhập
            if (Session["KhachHang"] == null)
            {
                // Lưu lại URL để quay lại sau khi login (nếu muốn làm kỹ hơn)
                return RedirectToAction("Login", "UserAuth_65132692");
            }

            var khach = (KhachHang)Session["KhachHang"];

            // Kiểm tra tồn kho thực tế
            var bienThe = db.BienTheSanPhams.Find(maBienThe);
            if (bienThe == null || bienThe.SoLuong < soLuong)
            {
                TempData["Loi"] = "Sản phẩm không đủ số lượng tồn kho!";
                // Quay lại trang chi tiết sản phẩm gốc
                var spID = bienThe?.MaSanPham ?? 1;
                return RedirectToAction("Details", "Shop_65132692", new { id = spID });
            }

            // Kiểm tra xem sản phẩm này đã có trong giỏ của khách chưa
            var itemTrongGio = db.GioHangs.FirstOrDefault(g => g.MaKhachHang == khach.MaKhachHang && g.MaBienThe == maBienThe);

            if (itemTrongGio != null)
            {
                // Có rồi -> Cộng dồn số lượng
                itemTrongGio.SoLuong += soLuong;
            }
            else
            {
                // Chưa có -> Thêm mới
                GioHang moi = new GioHang();
                moi.MaKhachHang = khach.MaKhachHang;
                moi.MaBienThe = maBienThe;
                moi.SoLuong = soLuong;
                db.GioHangs.Add(moi);
            }

            db.SaveChanges();
            return RedirectToAction("Index"); // Chuyển đến trang xem giỏ hàng
        }

        // 3. XÓA SẢN PHẨM KHỎI GIỎ
        public ActionResult XoaKhoiGio(int id)
        {
            var item = db.GioHangs.Find(id);
            if (item != null)
            {
                db.GioHangs.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 4. CẬP NHẬT SỐ LƯỢNG (Khi khách sửa số lượng trong giỏ)
        [HttpPost]
        public ActionResult CapNhatGioHang(int maGioHang, int soLuongMoi)
        {
            var item = db.GioHangs.Find(maGioHang);
            if (item != null)
            {
                // Kiểm tra tồn kho
                if (item.BienTheSanPham.SoLuong >= soLuongMoi && soLuongMoi > 0)
                {
                    item.SoLuong = soLuongMoi;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        // 5. ĐẶT HÀNG (CHECKOUT)
        // 5. ĐẶT HÀNG (CHECKOUT) - SỬA ĐỔI: Chỉ đặt các món được chọn
        [HttpPost]
        public ActionResult DatHang(string diaChiGiao, string selectedIds)
        {
            // selectedIds sẽ là chuỗi dạng "1,3,5" (các mã giỏ hàng được chọn)

            if (Session["KhachHang"] == null) return RedirectToAction("Login", "UserAuth_65132692");
            var khach = (KhachHang)Session["KhachHang"];

            if (string.IsNullOrEmpty(selectedIds))
            {
                TempData["Loi"] = "Bạn chưa chọn sản phẩm nào để đặt!";
                return RedirectToAction("Index");
            }

            // Tách chuỗi ID thành mảng số nguyên
            var listIds = selectedIds.Split(',').Select(int.Parse).ToList();

            // Lấy đúng những món hàng mà khách đã tick chọn
            var items = db.GioHangs.Where(g => g.MaKhachHang == khach.MaKhachHang
                                            && listIds.Contains(g.MaGioHang)).ToList();

            if (items.Count == 0) return RedirectToAction("Index");

            // --- CÁC BƯỚC CÒN LẠI GIỮ NGUYÊN ---

            // A. Tạo đơn hàng
            DonHang dh = new DonHang();
            dh.MaKhachHang = khach.MaKhachHang;
            dh.NgayDat = DateTime.Now;
            dh.TrangThai = "Chờ xác nhận";
            dh.DiaChiGiao = diaChiGiao;
            dh.TongTien = 0;

            db.DonHangs.Add(dh);
            db.SaveChanges();

            // B. Chuyển sang CT_DonHang
            foreach (var item in items)
            {
                CT_DonHang ct = new CT_DonHang();
                ct.MaDonHang = dh.MaDonHang;
                ct.MaBienThe = item.MaBienThe;
                ct.SoLuong = item.SoLuong;
                ct.DonGia = item.BienTheSanPham.SanPham.DonGia;
                db.CT_DonHang.Add(ct);
            }
            db.SaveChanges();

            // C. Gọi Procedure tính tiền
            // C. Gọi Stored Procedure và ép nó đọc xong dữ liệu ngay lập tức
            // Thêm .FirstOrDefault() để đóng kết nối đang mở
            db.sp_TinhTongTienDonHang(dh.MaDonHang).FirstOrDefault();

            // D. Xóa NHỮNG MÓN ĐÃ MUA khỏi giỏ (Giữ lại món chưa chọn)
            db.GioHangs.RemoveRange(items);
            db.SaveChanges();

            TempData["ThongBao"] = "Đặt hàng thành công đơn: #" + dh.MaDonHang;
            return RedirectToAction("Index", "Shop_65132692");
        }
        
    }
}