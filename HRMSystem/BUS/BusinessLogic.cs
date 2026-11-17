using System.Data;
using HRMSystem.DAL;

namespace HRMSystem.BUS
{
    public class NhanVienBUS
    {
        NhanVienDAL dal = new NhanVienDAL();
        public DataTable GetAll() => dal.GetAll();
        public DataTable Search(string kw) => dal.Search(kw);
    }

    public class ChamCongBUS
    {
        ChamCongDAL dal = new ChamCongDAL();
        public DataTable GetChamCongByThang(string thangNam) => dal.GetChamCongByThang(thangNam);
        public bool SaveChamCong(DataTable dt, string thangNam) => dal.SaveChamCong(dt, thangNam);
    }

    public class LuongBUS
    {
        LuongDAL dal = new LuongDAL();
        public bool ChotLuongThang(string thangNam) => dal.ChotLuongThang(thangNam);
    }
}