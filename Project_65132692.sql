CREATE DATABASE QuanLyBanQuanAo_65132692;
GO
USE QuanLyBanQuanAo_65132692; 
GO
---------------------------------------------------
-- BẢNG LOẠI SẢN PHẨM
---------------------------------------------------
CREATE TABLE LoaiSanPham (
    MaLoai INT PRIMARY KEY IDENTITY(1,1),
    TenLoai NVARCHAR(100) NOT NULL,
);

---------------------------------------------------
-- BẢNG SẢN PHẨM (Thông tin chung)
---------------------------------------------------
CREATE TABLE SanPham (
    MaSanPham INT PRIMARY KEY IDENTITY(1,1),
    TenSanPham NVARCHAR(200) NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    HinhAnh NVARCHAR(300),
    MaLoai INT,
    MoTa NVARCHAR(500),
    TrangThai BIT DEFAULT 1,     -- 1: đang bán, 0: ngừng bán
    FOREIGN KEY (MaLoai) REFERENCES LoaiSanPham(MaLoai)
);

---------------------------------------------------
-- BẢNG BIẾN THỂ SẢN PHẨM (Size, màu, tồn kho riêng)
---------------------------------------------------
CREATE TABLE BienTheSanPham (
    MaBienThe INT PRIMARY KEY IDENTITY(1,1),
    MaSanPham INT NOT NULL,
    Size NVARCHAR(10) NOT NULL,        -- S, M, L, XL
    MauSac NVARCHAR(50) NOT NULL,      -- Đỏ, Xanh, Đen...
    SoLuong INT NOT NULL DEFAULT 0,    -- Tồn kho theo từng biến thể
    FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham)
);

---------------------------------------------------
-- BẢNG KHÁCH HÀNG
---------------------------------------------------
CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100),
    SDT NVARCHAR(20),
    DiaChi NVARCHAR(200),
    Email NVARCHAR(50),
    Username NVARCHAR(50) UNIQUE,
    Password NVARCHAR(200)  -- Sẽ lưu dạng hash (BCrypt/SHA256)
);

---------------------------------------------------
-- BẢNG NHÂN VIÊN
---------------------------------------------------
CREATE TABLE NhanVien (
    MaNhanVien INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100),
    SDT NVARCHAR(20),
    ChucVu NVARCHAR(50),            -- Nhân viên / Quản lý
    Username NVARCHAR(50) UNIQUE,
    Password NVARCHAR(200),         -- Sẽ lưu dạng hash
    Role NVARCHAR(20)               -- admin / staff
);

---------------------------------------------------
-- BẢNG KHUYẾN MÃI
---------------------------------------------------
CREATE TABLE KhuyenMai (
    MaKhuyenMai INT PRIMARY KEY IDENTITY(1,1),
    TenKhuyenMai NVARCHAR(200) NOT NULL,
    MoTa NVARCHAR(500),
    LoaiKhuyenMai NVARCHAR(50),      -- % hoặc VND
    GiaTriGiam DECIMAL(18,2),        -- Giá trị giảm chung cho tất cả sản phẩm
    NgayBatDau DATETIME,
    NgayKetThuc DATETIME,
    DieuKien NVARCHAR(500),
    TrangThai BIT DEFAULT 1,
    CONSTRAINT CK_NgayKhuyenMai CHECK (NgayKetThuc > NgayBatDau)
);

---------------------------------------------------
-- BẢNG CHI TIẾT KHUYẾN MÃI
---------------------------------------------------
CREATE TABLE ChiTietKhuyenMai (
    MaChiTietKM INT PRIMARY KEY IDENTITY(1,1),
    MaKhuyenMai INT NOT NULL,
    MaSanPham INT NOT NULL,
    GiaTriGiamRieng DECIMAL(18,2),   -- Nếu sản phẩm này có mức giảm riêng khác với giảm chung
    FOREIGN KEY (MaKhuyenMai) REFERENCES KhuyenMai(MaKhuyenMai),
    FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham)
);

---------------------------------------------------
-- BẢNG GIỎ HÀNG
---------------------------------------------------
CREATE TABLE GioHang (
    MaGioHang INT IDENTITY(1,1) PRIMARY KEY,
    MaKhachHang INT NOT NULL,
    MaBienThe INT NOT NULL,          -- Thay đổi: tham chiếu đến biến thể cụ thể
    SoLuong INT NOT NULL DEFAULT 1,
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    FOREIGN KEY (MaBienThe) REFERENCES BienTheSanPham(MaBienThe),
    -- Ràng buộc UNIQUE: 1 khách chỉ có 1 dòng cho 1 biến thể trong giỏ
    CONSTRAINT UQ_GioHang UNIQUE (MaKhachHang, MaBienThe)
);

---------------------------------------------------
-- BẢNG ĐỌN HÀNG
---------------------------------------------------
CREATE TABLE DonHang (
    MaDonHang INT IDENTITY(1,1) PRIMARY KEY,
    MaKhachHang INT NOT NULL,
    MaNhanVien INT,                  -- Nhân viên xử lý đơn
    NgayDat DATETIME DEFAULT GETDATE(),
    NgayGiao DATETIME,               -- Ngày giao hàng thực tế
    TongTien DECIMAL(18,2) NOT NULL,
    TrangThai NVARCHAR(30) DEFAULT N'Chờ xác nhận',   -- Chờ xác nhận / Đang giao / Giao thành công / Đã hủy
    DiaChiGiao NVARCHAR(300),
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien)
);

---------------------------------------------------
-- BẢNG CHI TIẾT ĐƠN HÀNG
---------------------------------------------------
CREATE TABLE CT_DonHang (
    MaCT INT IDENTITY(1,1) PRIMARY KEY,
    MaDonHang INT NOT NULL,
    MaBienThe INT NOT NULL,          -- Thay đổi: tham chiếu biến thể
    SoLuong INT NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    ThanhTien AS (SoLuong * DonGia) PERSISTED,
    FOREIGN KEY (MaDonHang) REFERENCES DonHang(MaDonHang),
    FOREIGN KEY (MaBienThe) REFERENCES BienTheSanPham(MaBienThe)
);

---------------------------------------------------
-- TẠO INDEX ĐỂ TỐI ƯU TÌM KIẾM
---------------------------------------------------
-- Index cho bảng SanPham
CREATE INDEX IX_SanPham_MaLoai ON SanPham(MaLoai);
CREATE INDEX IX_SanPham_TrangThai ON SanPham(TrangThai);

-- Index cho bảng BienTheSanPham
CREATE INDEX IX_BienThe_MaSanPham ON BienTheSanPham(MaSanPham);

-- Index cho bảng DonHang
CREATE INDEX IX_DonHang_MaKhachHang ON DonHang(MaKhachHang);
CREATE INDEX IX_DonHang_TrangThai ON DonHang(TrangThai);
CREATE INDEX IX_DonHang_NgayDat ON DonHang(NgayDat);

-- Index cho bảng GioHang
CREATE INDEX IX_GioHang_MaKhachHang ON GioHang(MaKhachHang);

---------------------------------------------------
-- TRIGGER 1: TỰ ĐỘNG GIẢM TỒN KHO KHI ĐẶT HÀNG
---------------------------------------------------
GO
CREATE TRIGGER trg_GiamTonKho
ON CT_DonHang
AFTER INSERT
AS
BEGIN
    UPDATE BienTheSanPham
    SET SoLuong = SoLuong - i.SoLuong
    FROM BienTheSanPham bt
    INNER JOIN inserted i ON bt.MaBienThe = i.MaBienThe;
END;
GO

---------------------------------------------------
-- TRIGGER 2: TỰ ĐỘNG TĂNG TỒN KHO KHI HỦY ĐƠN
---------------------------------------------------
CREATE TRIGGER trg_TangTonKho_HuyDon
ON DonHang
AFTER UPDATE
AS
BEGIN
    -- Chỉ thực hiện khi trạng thái chuyển sang "Đã hủy"
    IF EXISTS (SELECT 1 FROM inserted WHERE TrangThai = N'Đã hủy')
       AND NOT EXISTS (SELECT 1 FROM deleted WHERE TrangThai = N'Đã hủy')
    BEGIN
        UPDATE BienTheSanPham
        SET SoLuong = SoLuong + ct.SoLuong
        FROM BienTheSanPham bt
        INNER JOIN CT_DonHang ct ON bt.MaBienThe = ct.MaBienThe
        INNER JOIN inserted i ON ct.MaDonHang = i.MaDonHang
        WHERE i.TrangThai = N'Đã hủy';
    END
END;
GO

---------------------------------------------------
-- STORED PROCEDURE: TÍNH TỔNG TIỀN ĐƠN HÀNG
---------------------------------------------------
CREATE PROCEDURE sp_TinhTongTienDonHang
    @MaDonHang INT
AS
BEGIN
    DECLARE @TongTien DECIMAL(18,2);
    
    SELECT @TongTien = SUM(ThanhTien)
    FROM CT_DonHang
    WHERE MaDonHang = @MaDonHang;
    
    UPDATE DonHang
    SET TongTien = @TongTien
    WHERE MaDonHang = @MaDonHang;
    
    SELECT @TongTien AS TongTienDonHang;
END;
GO

---------------------------------------------------
-- STORED PROCEDURE: ÁP DỤNG KHUYẾN MÃI
---------------------------------------------------
CREATE PROCEDURE sp_ApDungKhuyenMai
    @MaDonHang INT,
    @MaKhuyenMai INT
AS
BEGIN
    DECLARE @LoaiKM NVARCHAR(50);
    DECLARE @GiaTriGiam DECIMAL(18,2);
    DECLARE @TongTien DECIMAL(18,2);
    
    -- Lấy thông tin khuyến mãi
    SELECT @LoaiKM = LoaiKhuyenMai, @GiaTriGiam = GiaTriGiam
    FROM KhuyenMai
    WHERE MaKhuyenMai = @MaKhuyenMai 
      AND TrangThai = 1
      AND GETDATE() BETWEEN NgayBatDau AND NgayKetThuc;
    
    IF @LoaiKM IS NOT NULL
    BEGIN
        SELECT @TongTien = TongTien FROM DonHang WHERE MaDonHang = @MaDonHang;
        
        -- Giảm theo %
        IF @LoaiKM = N'%'
            SET @TongTien = @TongTien * (1 - @GiaTriGiam/100);
        -- Giảm theo VND
        ELSE IF @LoaiKM = N'VND'
            SET @TongTien = @TongTien - @GiaTriGiam;
        
        UPDATE DonHang
        SET TongTien = @TongTien
        WHERE MaDonHang = @MaDonHang;
        
        SELECT N'Áp dụng khuyến mãi thành công' AS ThongBao, @TongTien AS TongTienMoi;
    END
    ELSE
    BEGIN
        SELECT N'Khuyến mãi không hợp lệ' AS ThongBao;
    END
END;
GO

---------------------------------------------------
-- STORED PROCEDURE: THỐNG KÊ DOANH THU THEO THÁNG
---------------------------------------------------
CREATE PROCEDURE sp_ThongKeDoanhThu
    @Thang INT,
    @Nam INT
AS
BEGIN
    SELECT 
        CONVERT(DATE, NgayDat) AS NgayBan,
        COUNT(MaDonHang) AS SoDonHang,
        SUM(TongTien) AS DoanhThu
    FROM DonHang
    WHERE MONTH(NgayDat) = @Thang 
      AND YEAR(NgayDat) = @Nam
      AND TrangThai != N'Đã hủy'
    GROUP BY CONVERT(DATE, NgayDat)
    ORDER BY NgayBan;
END;
GO

---------------------------------------------------
-- DỮ LIỆU MẪU
---------------------------------------------------

-- ===================================================
-- 1. LOẠI SẢN PHẨM
-- ===================================================
INSERT INTO LoaiSanPham (TenLoai) VALUES
(N'Áo khoác'),
(N'Áo thun'),
(N'Áo sơ mi'),
(N'Quần'),
(N'Phụ kiện');

-- ===================================================
-- 2. SẢN PHẨM
-- ===================================================

INSERT INTO SanPham (TenSanPham, DonGia, HinhAnh, MaLoai, MoTa, TrangThai) VALUES
-- Áo khoác (MaLoai = 1)
(N'Áo khoác dù 2 lớp', 550000, N'khoac_du.jpg', 1, N'Áo khoác gió chống nước, giữ ấm tốt, phong cách outdoor', 1),
(N'Áo khoác hoodie zip form boxy', 300000, N'hoodie_zip_boxy.jpg', 1, N'Áo hoodie nỉ với form boxy hiện đại, chân gấu áo bo nhẹ', 1),
(N'Áo khoác hoodie zip', 480000, N'hoodie_zip_basic.jpg', 1, N'Áo hoodie zip cơ bản, chất nỉ bông dày dặn', 1),
(N'Áo khoác sweater', 690000, N'sweater_boxy.jpg', 1, N'Áo sweater nỉ form boxy, không mũ, trẻ trung', 1),
(N'Áo khoác polo sweater', 690000, N'polo_sweater.jpg', 1, N'Sự kết hợp giữa áo Polo và Sweater, lịch lãm mà ấm áp', 1),

-- Áo thun (MaLoai = 2)
(N'Áo thun trơn basic', 150000, N'aothun_basic.jpg', 2, N'Áo thun cotton 100%, thoáng mát, nhiều màu lựa chọn', 1),
(N'Áo thun polo nam', 250000, N'aothun_polo.jpg', 2, N'Áo polo có cổ, chất vải cá sấu cao cấp, đứng form', 1),
(N'Áo thun in form boxy', 200000, N'aothun_in_boxy.jpg', 2, N'Áo thun in hình Graphic độc đáo, vải cotton dày', 1),
(N'Áo thun thêu logo', 220000, N'aothun_theu.jpg', 2, N'Áo thun thêu logo tỉ mỉ ở ngực, co giãn 4 chiều', 1),

-- Áo sơ mi (MaLoai = 3)
(N'Áo sơ mi trắng tay dài', 320000, N'somi_trang.jpg', 3, N'Áo sơ mi trắng form slim-fit, chất vải chống nhăn', 1),
(N'Áo sơ mi form boxy', 280000, N'somi_boxy_ke.jpg', 3, N'Áo sơ mi kẻ sọc dáng ngắn, phong cách streetwear', 1),
(N'Áo sơ mi form rộng', 350000, N'somi_oversize.jpg', 3, N'Áo sơ mi oversize năng động, dễ phối đồ lớp ngoài', 1),

-- Quần (MaLoai = 4)
(N'Quần âu nam ống rộng', 380000, N'quan_au_rong.jpg', 4, N'Quần tây ống suông rộng, tạo hiệu ứng kéo dài chân', 1),
(N'Quần kaki nam basic', 360000, N'quan_kaki_nam.jpg', 4, N'Quần kaki dáng đứng, vải bền màu, bền form', 1),
(N'Quần short âu form lửng', 260000, N'short_au.jpg', 4, N'Quần short vải tây sang trọng, độ dài ngang gối', 1),
(N'Quần nỉ ống rộng', 340000, N'quan_ni_rong.jpg', 4, N'Quần sweatpants chất nỉ chân cua, thoải mái vận động', 1),

-- Phụ kiện (MaLoai = 5)
(N'Túi phao đeo chéo', 180000, N'tui_phao.jpg', 5, N'Túi đeo chéo chất liệu phao chần bông thời thượng', 1),
(N'Cà vạt thắt sẵn', 120000, N'cavat_san.jpg', 5, N'Cà vạt có dây kéo thắt sẵn, tiện lợi cho dân văn phòng', 1),
(N'Móc khoá cao su hình cú', 50000, N'mockhoa_cu.jpg', 5, N'Móc khóa silicon hình cú mèo dễ thương, bền bỉ', 1);


-- ===================================================
-- 3. BIẾN THỂ SẢN PHẨM (Size + Màu + Tồn kho)
-- ===================================================
INSERT INTO BienTheSanPham (MaSanPham, Size, MauSac, SoLuong) VALUES
-- Áo khoác dù (ID 1)
(1, N'M', N'Đen', 25), (1, N'L', N'Đen', 30), (1, N'XL', N'Đen', 20),
-- Hoodie zip boxy (ID 2)
(2, N'M', N'Xám', 20), (2, N'L', N'Xám', 25), (2, N'XL', N'Xám', 22),(2, N'M', N'Đen', 20), (2, N'L', N'Đen', 25), (2, N'XL', N'Đen', 22),
-- Hoodie zip basic (ID 3)
(3, N'M', N'Xám', 40), (3, N'L', N'Xám', 45), (3, N'XL', N'Xám', 38),
-- Sweater (ID 4)
(4, N'M', N'Đen', 18), (4, N'L', N'Đen', 22), (4, N'XL', N'Đen', 15),
-- Polo Sweater (ID 5)
-- Áo hoodie zip (ID 5)
(5, N'M', N'Đen', 20), (5, N'L', N'Đen', 18),

-- Áo thun basic (ID 6)
(6, N'S', N'Trắng', 50), (6, N'M', N'Trắng', 80), (6, N'L', N'Trắng', 60),

-- Áo thun polo (ID 7)
(7, N'M', N'Xanh navy', 35), (7, N'L', N'Xanh navy', 45),

-- Áo thun in boxy (ID 8)
(8, N'S', N'Hồng', 60), (8, N'M', N'Hồng', 70),

-- Áo thun thêu (ID 9)
(9, N'S', N'Trắng', 30), (9, N'M', N'Trắng', 40),

-- Sơ mi trắng (ID 10)
(10, N'M', N'Trắng', 40), (10, N'L', N'Trắng', 50), (10, N'XL', N'Trắng', 30),

-- Sơ mi kẻ boxy (ID 11)
(11, N'M', N'Xanh trắng', 30), (11, N'L', N'Xanh trắng', 35),

-- Sơ mi form rộng (ID 12)
(12, N'M', N'Trắng', 45), (12, N'L', N'Trắng', 30),

-- Quần âu ống rộng (ID 13) - Đã đổi size số sang chữ
(13, N'M', N'Đen', 35), (13, N'L', N'Đen', 45), (13, N'XL', N'Đen', 40),

-- Quần kaki (ID 14) - Đã đổi size số sang chữ
(14, N'M', N'Be', 40), (14, N'L', N'Be', 45),

-- Quần short (ID 15) - Đã đổi size số sang chữ
(15, N'S', N'Xanh', 45), (15, N'M', N'Xanh', 50),

-- Quần nỉ (ID 16) - Chia Free size thành M và L
(16, N'M', N'Đen', 20), (16, N'L', N'Đen', 22),

-- Túi phao (ID 17)
(17, N'Free', N'Đen', 55),

-- Cà vạt (ID 18)
(18, N'Free', N'Đen', 80),

-- Móc khóa (ID 19)
(19, N'Free', N'Mặc định', 85);-- ===================================================
-- 4. KHÁCH HÀNG
-- ===================================================
INSERT INTO KhachHang (HoTen, SDT, DiaChi, Email, Username, Password) VALUES
(N'Nguyễn Văn An', '0901234567', N'123 Lý Thường Kiệt, Q.10, TP.HCM', 'nguyenvanan@gmail.com', 'vanan', 'hash_password_1'),
(N'Trần Thị Bình', '0912345678', N'456 Trần Hưng Đạo, Q.1, TP.HCM', 'tranbinhtt@gmail.com', 'thibinh', 'hash_password_2'),
(N'Lê Minh Cường', '0923456789', N'789 Nguyễn Huệ, Q.1, TP.HCM', 'leminhcuong@gmail.com', 'minhcuong', 'hash_password_3'),
(N'Phạm Thu Dung', '0934567890', N'321 Hai Bà Trưng, Q.3, TP.HCM', 'phamthudung@gmail.com', 'thudung', 'hash_password_4'),
(N'Hoàng Văn Em', '0945678901', N'654 Võ Văn Tần, Q.3, TP.HCM', 'hoangvanem@gmail.com', 'vanem', 'hash_password_5'),
(N'Vũ Thị Phương', '0956789012', N'987 Lê Lợi, Q.1, TP.HCM', 'vuthiphuong@gmail.com', 'thiphuong', 'hash_password_6'),
(N'Đỗ Minh Quân', '0967890123', N'147 Cách Mạng Tháng 8, Q.10, TP.HCM', 'dominhquan@gmail.com', 'minhquan', 'hash_password_7'),
(N'Bùi Thu Hương', '0978901234', N'258 Điện Biên Phủ, Q.Bình Thạnh, TP.HCM', 'buithuhuong@gmail.com', 'thuhuong', 'hash_password_8');

-- ===================================================
-- 5. NHÂN VIÊN
-- ===================================================
INSERT INTO NhanVien (HoTen, SDT, ChucVu, Username, Password, Role) VALUES
(N'Trần Văn Quản', '0909111222', N'Quản lý', 'admin', 'hash_admin_pass', 'admin'),
(N'Nguyễn Thị Hoa', '0909222333', N'Nhân viên bán hàng', 'nvhoa', 'hash_nv_pass1', 'staff'),
(N'Lê Văn Bình', '0909333444', N'Nhân viên bán hàng', 'nvbinh', 'hash_nv_pass2', 'staff'),
(N'Phạm Thị Lan', '0909444555', N'Nhân viên kho', 'nvlan', 'hash_nv_pass3', 'staff'),
(N'Hoàng Minh Tuấn', '0909555666', N'Nhân viên giao hàng', 'nvtuan', 'hash_nv_pass4', 'staff');

-- ===================================================
-- 6. KHUYẾN MÃI
-- ===================================================
INSERT INTO KhuyenMai (TenKhuyenMai, MoTa, LoaiKhuyenMai, GiaTriGiam, NgayBatDau, NgayKetThuc, DieuKien, TrangThai) VALUES
(N'Giảm giá mùa hè', N'Giảm 15% tất cả sản phẩm áo thun', N'%', 15, '2024-06-01', '2024-08-31', N'Áp dụng cho đơn hàng từ 300k', 1),
(N'Khuyến mãi khai trương', N'Giảm 100k cho đơn hàng đầu tiên', N'VND', 100000, '2024-01-01', '2024-12-31', N'Khách hàng mới', 1),
(N'Sale cuối tuần', N'Giảm 20% các sản phẩm áo khoác', N'%', 20, '2024-11-01', '2024-11-30', N'Áp dụng thứ 7, chủ nhật', 1),
(N'Ưu đãi Black Friday', N'Giảm 300k cho đơn từ 2 triệu', N'VND', 300000, '2024-11-24', '2024-11-26', N'Đơn hàng từ 2.000.000đ', 0),
(N'Giảm giá đầu năm', N'Giảm 10% toàn bộ cửa hàng', N'%', 10, '2025-01-01', '2025-01-15', N'Không giới hạn', 1);

-- ===================================================
-- 7. CHI TIẾT KHUYẾN MÃI
-- ===================================================
-- Khuyến mãi mùa hè áp dụng cho các áo thun (MaLoai = 2)
INSERT INTO ChiTietKhuyenMai (MaKhuyenMai, MaSanPham, GiaTriGiamRieng) VALUES
(1, 6, NULL),  -- Áo thun basic
(1, 7, NULL),  -- Áo polo
(1, 8, NULL),  -- Áo thun nữ
(1, 9, NULL),  -- Áo thun họa tiết
(1, 10, NULL); -- Áo thun thể thao

-- Khuyến mãi sale cuối tuần cho áo khoác (MaLoai = 1)
INSERT INTO ChiTietKhuyenMai (MaKhuyenMai, MaSanPham, GiaTriGiamRieng) VALUES
(3, 1, NULL),  -- Áo khoác dù
(3, 2, NULL),  -- Áo khoác jean
(3, 3, NULL),  -- Áo hoodie
(3, 4, NULL),  -- Áo bomber
(3, 5, NULL);  -- Blazer nữ
