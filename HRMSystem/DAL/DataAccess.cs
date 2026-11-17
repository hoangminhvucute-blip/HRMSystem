using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace HRMSystem.DAL
{
    public class DatabaseAccess
    {
        // Thay đổi tên Server cho khớp với SQL Server trên máy bạn
        protected string connStr = @"Server=DESKTOP-CMSNBP3;Database=HRMDB;Integrated Security=True;";
    }

    public class NhanVienDAL : DatabaseAccess
    {
        public DataTable GetAll()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT nv.*, pb.TenPB FROM NhanVien nv JOIN PhongBan pb ON nv.MaPB = pb.MaPB";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Tìm kiếm nhân viên bằng Parameterized Query (chống SQL Injection)
        public DataTable Search(string keyword)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT nv.*, pb.TenPB FROM NhanVien nv JOIN PhongBan pb ON nv.MaPB = pb.MaPB 
                                 WHERE nv.HoTen LIKE @kw OR nv.CCCD LIKE @kw OR pb.TenPB LIKE @kw";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
    }

    public class ChamCongDAL : DatabaseAccess
    {
        // 1. Nạp chấm công theo tháng
        public DataTable GetChamCongByThang(string thangNam)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT nv.MaNV, nv.HoTen, 
                                        ISNULL(cc.MaCong, 0) AS MaCong, 
                                        ISNULL(cc.ThangNam, @thang) AS ThangNam, 
                                        ISNULL(cc.SoNgayCong, 0) AS SoNgayCong, 
                                        ISNULL(cc.SoGioOT, 0) AS SoGioOT 
                                 FROM NhanVien nv 
                                 LEFT JOIN BangChamCong cc ON nv.MaNV = cc.MaNV AND cc.ThangNam = @thang";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@thang", thangNam);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // 2. Lưu chấm công sử dụng cơ chế xử lý rõ ràng (Insert nếu chưa có, Update nếu đã tồn tại)
        public bool SaveChamCong(DataTable dtChanges, string thangNam)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                foreach (DataRow row in dtChanges.Rows)
                {
                    if (row.RowState == DataRowState.Deleted) continue;

                    int maNV = Convert.ToInt32(row["MaNV"]);
                    float soNgayCong = row["SoNgayCong"] != DBNull.Value ? Convert.ToSingle(row["SoNgayCong"]) : 0;
                    float soGioOT = row["SoGioOT"] != DBNull.Value ? Convert.ToSingle(row["SoGioOT"]) : 0;

                    // Kiểm tra xem nhân viên này đã có bản ghi chấm công trong tháng chưa
                    string checkQuery = "SELECT COUNT(*) FROM BangChamCong WHERE MaNV = @manv AND ThangNam = @thang";
                    SqlCommand cmdCheck = new SqlCommand(checkQuery, conn);
                    cmdCheck.Parameters.AddWithValue("@manv", maNV);
                    cmdCheck.Parameters.AddWithValue("@thang", thangNam);
                    int count = (int)cmdCheck.ExecuteScalar();

                    SqlCommand cmdSave;
                    if (count > 0)
                    {
                        // Nếu có rồi thì Update
                        string updateQuery = "UPDATE BangChamCong SET SoNgayCong = @ngay, SoGioOT = @ot WHERE MaNV = @manv AND ThangNam = @thang";
                        cmdSave = new SqlCommand(updateQuery, conn);
                    }
                    else
                    {
                        // Nếu chưa có thì Insert mới
                        string insertQuery = "INSERT INTO BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT) VALUES (@manv, @thang, @ngay, @ot)";
                        cmdSave = new SqlCommand(insertQuery, conn);
                    }

                    cmdSave.Parameters.AddWithValue("@manv", maNV);
                    cmdSave.Parameters.AddWithValue("@thang", thangNam);
                    cmdSave.Parameters.AddWithValue("@ngay", soNgayCong);
                    cmdSave.Parameters.AddWithValue("@ot", soGioOT);
                    cmdSave.ExecuteNonQuery();
                }
                return true;
            }
        }
    }
    public class LuongDAL : DatabaseAccess
    {
        // 3. Tính lương và Chốt lương tháng bằng SqlTransaction
        public bool ChotLuongThang(string thangNam)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    string queryData = @"SELECT nv.MaNV, nv.LuongCoBan, nv.HeSoLuong, 
                                                ISNULL(cc.SoNgayCong, 0) AS SoNgayCong, 
                                                ISNULL(cc.SoGioOT, 0) AS SoGioOT
                                         FROM NhanVien nv 
                                         JOIN BangChamCong cc ON nv.MaNV = cc.MaNV WHERE cc.ThangNam = @thang";
                    SqlCommand cmdGet = new SqlCommand(queryData, conn, trans);
                    cmdGet.Parameters.AddWithValue("@thang", thangNam);

                    using (SqlDataReader reader = cmdGet.ExecuteReader())
                    {
                        var listLuong = new List<Tuple<int, decimal>>();

                        while (reader.Read())
                        {
                            int maNV = reader.GetInt32(0);
                            decimal luongCB = reader.GetDecimal(1);
                            double heSo = reader.GetDouble(2);
                            double soNgayCong = reader.GetDouble(3);
                            double soGioOT = reader.GetDouble(4);

                            // Công thức: ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + (SoGioOT * 50000)
                            decimal thucLinh = (decimal)((double)(luongCB * (decimal)heSo) / 22.0 * soNgayCong) + (decimal)(soGioOT * 50000);
                            listLuong.Add(new Tuple<int, decimal>(maNV, thucLinh));
                        }
                        reader.Close();

                        // Ghi nhận vào bảng BangLuong
                        foreach (var item in listLuong)
                        {
                            string upsertQuery = @"IF EXISTS (SELECT 1 FROM BangLuong WHERE MaNV = @manv AND ThangNam = @thang)
                                                   UPDATE BangLuong SET ThucLinh = @tl WHERE MaNV = @manv AND ThangNam = @thang;
                                                   ELSE
                                                   INSERT INTO BangLuong (MaNV, ThangNam, ThucLinh) VALUES (@manv, @thang, @tl);";
                            SqlCommand cmdUpd = new SqlCommand(upsertQuery, conn, trans);
                            cmdUpd.Parameters.AddWithValue("@manv", item.Item1);
                            cmdUpd.Parameters.AddWithValue("@thang", thangNam);
                            cmdUpd.Parameters.AddWithValue("@tl", item.Item2);
                            cmdUpd.ExecuteNonQuery();
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception("Lỗi chốt lương: " + ex.Message);
                }
            }
        }
    }
}