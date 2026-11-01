using System;
using System.Linq;
using System.Web.Mvc;
using Project_65132692.Models;

namespace Project_65132692.Controllers
{
    public class Shop_65132692Controller : Controller
    {
        // Khởi tạo kết nối CSDL (Đổi tên entity theo đúng project của bạn)
        private QuanLyBanQuanAo_65132692Entities db = new QuanLyBanQuanAo_65132692Entities();

        // GET: Shop_65132692
        public ActionResult Index()
        {
            // Sử dụng ViewModel có đuôi ID như bạn đã tạo
            HomeViewModel_65132692 model = new HomeViewModel_65132692();

            // 1. Lấy danh sách Loại sản phẩm
            model.ListLoai = db.LoaiSanPhams.ToList();

            // 2. Lấy danh sách sản phẩm Random
            model.ListSanPhams = db.SanPhams
                                   .Where(s => s.TrangThai == true)
                                   .OrderBy(x => Guid.NewGuid())
                                   .Take(8)
                                   .ToList();

            return View(model);
        }
        // Action này chỉ dùng để render cái Menu thôi, không phải trang web
        [ChildActionOnly]
        public ActionResult Menu()
        {
            // Lấy list loại sản phẩm để hiển thị lên menu
            var listLoai = db.LoaiSanPhams.ToList();
            return PartialView("_MenuPartial", listLoai);
        }
    }
}