CREATE DATABASE HRMDB;
GO
USE HRMDB;
GO

-- 1. Phòng ban
CREATE TABLE PhongBan (
    MaPB INT IDENTITY(1,1) PRIMARY KEY,
    TenPB NVARCHAR(100) NOT NULL,
    Mota NVARCHAR(255)
);

-- 2. Hồ sơ nhân viên
CREATE TABLE NhanVien (
    MaNV INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATETIME,
    CCCD VARCHAR(20) UNIQUE,
    MaPB INT FOREIGN KEY REFERENCES PhongBan(MaPB),
    LuongCoBan DECIMAL(18,2) CHECK (LuongCoBan > 0),
    HeSoLuong FLOAT DEFAULT 1.0
);

-- 3. Bảng chấm công tháng
CREATE TABLE BangChamCong (
    MaCong INT IDENTITY(1,1) PRIMARY KEY,
    MaNV INT FOREIGN KEY REFERENCES NhanVien(MaNV),
    ThangNam VARCHAR(7), -- Định dạng: "MM/yyyy" (Ví dụ: "11/2026")
    SoNgayCong FLOAT CHECK (SoNgayCong <= 31),
    SoGioOT FLOAT DEFAULT 0
);

-- 4. Bảng lương chi tiết
CREATE TABLE BangLuong (
    MaLuong INT IDENTITY(1,1) PRIMARY KEY,
    MaNV INT FOREIGN KEY REFERENCES NhanVien(MaNV),
    ThangNam VARCHAR(7),
    ThucLinh DECIMAL(18,2),
    CONSTRAINT UQ_NhanVien_ThangNam UNIQUE (MaNV, ThangNam)
);
GO

-- Dữ liệu mẫu ban đầu
INSERT INTO PhongBan (TenPB, Mota) VALUES 
(N'Phòng Kỹ Thuật', N'Phát triển phần mềm & Hệ thống'), 
(N'Phòng Kế Toán', N'Quản lý tài chính & Thu chi');
GO

INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) VALUES 
(N'Phan Mạnh Trường', '2004-05-12', '034204001234', 1, 12000000, 1.2),
(N'Lê Thị Hương', '2005-08-20', '034205005678', 2, 10000000, 1.0);
GO