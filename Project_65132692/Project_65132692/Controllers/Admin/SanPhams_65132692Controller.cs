using Project_65132692.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity; // Nhớ dòng này
using System.IO; // Nhớ dòng này để xử lý ảnh
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_65132692.Controllers.Admin
{
    public class SanPhams_65132692Controller : Controller
    {
        private readonly QuanLyBanQuanAo_65132692Entities db = new QuanLyBanQuanAo_65132692Entities();

        // Check login Admin cho chắc
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["NhanVien"] == null)
            {
                filterContext.Result = RedirectToAction("Login", "AdminAuth_65132692");
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: Index (Danh sách sản phẩm)
        public ActionResult Index()
        {
            var sanPhams = db.SanPhams.Include(s => s.LoaiSanPham);
            return View(sanPhams.ToList());
        }

        // --- 1. GET: Hiển thị form thêm mới ---
        public ActionResult Create()
        {
            ViewBag.MaLoai = new SelectList(db.LoaiSanPhams, "MaLoai", "TenLoai");
            return View();
        }

        // --- 2. POST: Xử lý lưu dữ liệu (Quan trọng) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Lưu ý tham số: HinhAnhUpload (file), và 3 mảng Size, MauSac, SoLuong
        public ActionResult Create(SanPham sanPham, HttpPostedFileBase HinhAnhUpload, string[] Size, string[] MauSac, int[] SoLuong)
        {
            if (ModelState.IsValid)
            {
                // A. Xử lý lưu Hình ảnh
                if (HinhAnhUpload != null && HinhAnhUpload.ContentLength > 0)
                {
                    // Lấy tên file
                    string fileName = Path.GetFileName(HinhAnhUpload.FileName);
                    // Lưu vào thư mục ~/Images/
                    string path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                    HinhAnhUpload.SaveAs(path);

                    // Gán tên file vào đối tượng để lưu xuống DB
                    sanPham.HinhAnh = fileName;
                }
                else
                {
                    sanPham.HinhAnh = "default.jpg"; // Ảnh mặc định nếu không up
                }

                // B. Lưu sản phẩm chung (Master)
                sanPham.TrangThai = true; // Mặc định đang bán
                db.SanPhams.Add(sanPham);
                db.SaveChanges(); // Lưu xong để lấy MaSanPham vừa tạo

                // C. Lưu danh sách biến thể (Detail)
                // Kiểm tra nếu người dùng có nhập biến thể
                if (Size != null && MauSac != null && SoLuong != null)
                {
                    for (int i = 0; i < Size.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(Size[i]) && !string.IsNullOrEmpty(MauSac[i]))
                        {
                            BienTheSanPham bt = new BienTheSanPham();
                            bt.MaSanPham = sanPham.MaSanPham; // Lấy ID vừa tạo ở trên
                            bt.Size = Size[i];
                            bt.MauSac = MauSac[i];
                            bt.SoLuong = SoLuong[i];

                            db.BienTheSanPhams.Add(bt);
                        }
                    }
                    db.SaveChanges(); // Lưu tất cả biến thể
                }

                return RedirectToAction("Index");
            }

            ViewBag.MaLoai = new SelectList(db.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            return View(sanPham);
        }
        // --- CODE TIẾP THEO TRONG SanPhams_65132692Controller.cs ---

        // ===================== CHỨC NĂNG SỬA (EDIT) =====================

        // 1. GET: Hiển thị form sửa
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null) return HttpNotFound();

            ViewBag.MaLoai = new SelectList(db.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);

            // Lấy danh sách biến thể hiện có để hiển thị ra View
            ViewBag.ListBienThe = db.BienTheSanPhams.Where(bt => bt.MaSanPham == id).ToList();

            return View(sanPham);
        }

        // 2. POST: Xử lý cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SanPham sanPham, HttpPostedFileBase HinhAnhUpload, string[] Size, string[] MauSac, int[] SoLuong)
        {
            if (ModelState.IsValid)
            {
                var spGoc = db.SanPhams.Find(sanPham.MaSanPham);

                // 1. CẬP NHẬT THÔNG TIN CHUNG (Bao gồm việc Tắt/Mở bán)
                spGoc.TenSanPham = sanPham.TenSanPham;
                spGoc.DonGia = sanPham.DonGia;
                spGoc.MaLoai = sanPham.MaLoai;
                spGoc.MoTa = sanPham.MoTa;

                // Đây là dòng quan trọng nhất theo ý bạn: 
                // Nếu bạn bỏ tick "Đang kinh doanh", nó sẽ lưu False và ẩn khỏi trang chủ.
                // Việc này không hề ảnh hưởng đến các size số bên dưới.
                spGoc.TrangThai = sanPham.TrangThai;

                // 2. XỬ LÝ ẢNH (Giữ nguyên)
                if (HinhAnhUpload != null && HinhAnhUpload.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(HinhAnhUpload.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Images/"), fileName);
                    HinhAnhUpload.SaveAs(path);
                    spGoc.HinhAnh = fileName;
                }

                // 3. XỬ LÝ BIẾN THỂ (SIZE/MÀU) - CƠ CHẾ MỚI: UPDATE OR ADD
                // Chúng ta KHÔNG XÓA bất cứ thứ gì trong DB để tránh lỗi "Đã có người mua"

                if (Size != null && MauSac != null && SoLuong != null)
                {
                    for (int i = 0; i < Size.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(Size[i]) && !string.IsNullOrEmpty(MauSac[i]))
                        {
                            // Gán ra biến tạm để tránh lỗi LINQ ArrayIndex
                            string curSize = Size[i];
                            string curMau = MauSac[i];
                            int curSL = SoLuong[i];

                            // Kiểm tra xem combo Size+Màu này đã có trong kho chưa
                            var existing = db.BienTheSanPhams.FirstOrDefault(x =>
                                x.MaSanPham == sanPham.MaSanPham &&
                                x.Size == curSize &&
                                x.MauSac == curMau);

                            if (existing != null)
                            {
                                // TRƯỜNG HỢP 1: Đã có -> Chỉ cập nhật số lượng mới
                                // (Dù sản phẩm này đã bán cả nghìn đơn cũng không sao, vì ta chỉ sửa số lượng tồn)
                                existing.SoLuong = curSL;
                            }
                            else
                            {
                                // TRƯỜNG HỢP 2: Chưa có -> Thêm size mới vào
                                BienTheSanPham bt = new BienTheSanPham();
                                bt.MaSanPham = sanPham.MaSanPham;
                                bt.Size = curSize;
                                bt.MauSac = curMau;
                                bt.SoLuong = curSL;
                                db.BienTheSanPhams.Add(bt);
                            }
                        }
                    }
                }

                db.SaveChanges(); // Lưu thành công 100%
                return RedirectToAction("Index");
            }

            ViewBag.MaLoai = new SelectList(db.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            ViewBag.ListBienThe = db.BienTheSanPhams.Where(bt => bt.MaSanPham == sanPham.MaSanPham).ToList();
            return View(sanPham);
        }

        // ===================== CHỨC NĂNG XÓA (DELETE) =====================

        // 1. GET: Xác nhận xóa (Khi bấm nút Xóa ở Index)
        // Xóa 2 hàm cũ đi và thay bằng hàm này:
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // --- BẮT ĐẦU XÓA ---

            // 1. Xóa các biến thể (Size/Màu) trước để tránh lỗi khóa ngoại
            var bienThes = db.BienTheSanPhams.Where(b => b.MaSanPham == id).ToList();
            if (bienThes.Any())
            {
                db.BienTheSanPhams.RemoveRange(bienThes);
            }

            // 2. Xóa file ảnh trên Server để dọn rác
            if (!string.IsNullOrEmpty(sanPham.HinhAnh))
            {
                string path = System.IO.Path.Combine(Server.MapPath("~/Images/"), sanPham.HinhAnh);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            // 3. Xóa sản phẩm chính
            db.SanPhams.Remove(sanPham);
            db.SaveChanges();

            // 4. Quay lại trang danh sách (Thay vì return View)
            return RedirectToAction("Index");
        }
        // ... Các hàm Edit, Details, Delete cũ giữ nguyên (hoặc xóa nếu chưa dùng)
    }
}