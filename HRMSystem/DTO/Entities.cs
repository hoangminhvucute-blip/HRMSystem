namespace HRMSystem.DTO
{
    public class NhanVienDTO
    {
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public System.DateTime NgaySinh { get; set; }
        public string CCCD { get; set; }
        public int MaPB { get; set; }
        public decimal LuongCoBan { get; set; }
        public float HeSoLuong { get; set; }
    }

    public class BangLuongDTO
    {
        public int MaLuong { get; set; }
        public int MaNV { get; set; }
        public string ThangNam { get; set; }
        public decimal ThucLinh { get; set; }
    }
}