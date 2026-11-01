using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_65132692.Models
{
    public class HomeViewModel_65132692
    {
        public List<LoaiSanPham> ListLoai { get; set; }

        // Danh sách sản phẩm để hiện ở mục New Arrivals
        public List<SanPham> ListSanPhams { get; set; }
    }
}