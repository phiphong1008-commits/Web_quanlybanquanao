using System.Linq;
using System.Net;
using System.Web.Mvc;
using Project_65132692.Models;

namespace Project_65132692.Controllers
{
    public class LoaiSanPham_65132692Controller : Controller
    {
        private QuanLyBanQuanAo_65132692Entities db = new QuanLyBanQuanAo_65132692Entities();

        // GET: LoaiSanPham_65132692/Index?maLoai=1
        public ActionResult Index(int? maLoai)
        {
            // 1. Nếu không truyền mã loại (khách gõ url bậy), đá về trang chủ
            if (maLoai == null)
            {
                return RedirectToAction("Index", "Shop_65132692");
            }

            // 2. Lọc sản phẩm theo mã loại
            // Include("LoaiSanPham") để lấy được tên loại (ví dụ "Áo Thun") hiện lên tiêu đề
            var products = db.SanPhams
                             .Where(p => p.MaLoai == maLoai && p.TrangThai == true) // Chỉ lấy hàng đang bán
                             .ToList();

            // 3. Lấy tên loại để hiển thị lên tiêu đề View (cho đẹp)
            var loai = db.LoaiSanPhams.Find(maLoai);
            if (loai != null)
            {
                ViewBag.TenLoai = loai.TenLoai;
            }

            return View(products);
        }

        // GET: LoaiSanPham_65132692/Details/5
        // Dùng khi khách bấm vào ảnh sản phẩm để xem chi tiết
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            return View(sanPham);
        }
    }
}