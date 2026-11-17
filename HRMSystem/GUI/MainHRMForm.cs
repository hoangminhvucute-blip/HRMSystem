using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HRMSystem.BUS;

namespace HRMSystem.GUI
{
    public class MainHRMForm : Form
    {
        DataGridView dgvNhanVien, dgvChamCong;
        TextBox txtTimKiem, txtThangNam;
        Button btnTimKiem, btnLoadChamCong, btnLuuChamCong, btnChotLuong;
        TabControl tabControl;
        TabPage tabNV, tabCC;

        NhanVienBUS nvBUS = new NhanVienBUS();
        ChamCongBUS ccBUS = new ChamCongBUS();
        LuongBUS lBUS = new LuongBUS();

        DataTable dtChamCongCache; // Lưu cache DataTable cho mô hình ngắt kết nối

        public MainHRMForm()
        {
            this.Text = "Hệ Thống Quản Lý Nhân Sự & Tính Lương (HRM & Payroll)";
            this.Size = new Size(1050, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeCustomComponents();
            LoadNhanVien();
        }

        private void InitializeCustomComponents()
        {
            tabControl = new TabControl() { Dock = DockStyle.Fill };

            tabNV = new TabPage("1. Hồ Sơ Nhân Sự");
            tabCC = new TabPage("2. Chấm Công & Tính Lương (Disconnected & Transaction)");

            tabControl.TabPages.Add(tabNV);
            tabControl.TabPages.Add(tabCC);
            this.Controls.Add(tabControl);

            // --- TAB 1: NHÂN SỰ ---
            Label lblSearch = new Label() { Text = "Tìm kiếm (Tên/CCCD/Phòng):", Location = new Point(15, 15), AutoSize = true };
            txtTimKiem = new TextBox() { Location = new Point(190, 12), Width = 220 };
            btnTimKiem = new Button() { Text = "Tìm kiếm", Location = new Point(420, 10), Width = 90, Height = 25 };
            btnTimKiem.Click += (s, e) => { dgvNhanVien.DataSource = nvBUS.Search(txtTimKiem.Text); };

            dgvNhanVien = new DataGridView() { Location = new Point(15, 50), Width = 990, Height = 520, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true };

            tabNV.Controls.Add(lblSearch);
            tabNV.Controls.Add(txtTimKiem);
            tabNV.Controls.Add(btnTimKiem);
            tabNV.Controls.Add(dgvNhanVien);

            // --- TAB 2: CHẤM CÔNG & LƯƠNG ---
            Label lblThang = new Label() { Text = "Tháng/Năm (MM/yyyy):", Location = new Point(15, 20), AutoSize = true };
            txtThangNam = new TextBox() { Text = "11/2026", Location = new Point(160, 17), Width = 100 };

            btnLoadChamCong = new Button() { Text = "Nạp Bảng Chấm Công", Location = new Point(270, 15), Width = 150, Height = 28 };
            btnLoadChamCong.Click += BtnLoadChamCong_Click;

            dgvChamCong = new DataGridView() { Location = new Point(15, 60), Width = 990, Height = 450, SelectionMode = DataGridViewSelectionMode.CellSelect };

            btnLuuChamCong = new Button() { Text = "Lưu Chấm Công (Disconnected)", Location = new Point(15, 525), Width = 220, Height = 40, BackColor = Color.LightBlue };
            btnLuuChamCong.Click += BtnLuuChamCong_Click;

            btnChotLuong = new Button() { Text = "Chốt Lương Tháng (SqlTransaction)", Location = new Point(250, 525), Width = 240, Height = 40, BackColor = Color.LightGreen };
            btnChotLuong.Click += BtnChotLuong_Click;

            tabCC.Controls.Add(lblThang);
            tabCC.Controls.Add(txtThangNam);
            tabCC.Controls.Add(btnLoadChamCong);
            tabCC.Controls.Add(dgvChamCong);
            tabCC.Controls.Add(btnLuuChamCong);
            tabCC.Controls.Add(btnChotLuong);
        }

        private void LoadNhanVien()
        {
            dgvNhanVien.DataSource = nvBUS.GetAll();
        }

        private void BtnLoadChamCong_Click(object sender, EventArgs e)
        {
            string thang = txtThangNam.Text.Trim();
            dtChamCongCache = ccBUS.GetChamCongByThang(thang);
            dgvChamCong.DataSource = dtChamCongCache;

            // Khóa các cột định danh, chỉ cho phép chỉnh sửa Ngày Công và Giờ OT trên lưới
            if (dgvChamCong.Columns["MaNV"] != null) dgvChamCong.Columns["MaNV"].ReadOnly = true;
            if (dgvChamCong.Columns["HoTen"] != null) dgvChamCong.Columns["HoTen"].ReadOnly = true;
            if (dgvChamCong.Columns["MaCong"] != null) dgvChamCong.Columns["MaCong"].Visible = false;
            if (dgvChamCong.Columns["ThangNam"] != null) dgvChamCong.Columns["ThangNam"].Visible = false;
        }

        private void BtnLuuChamCong_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtChamCongCache == null) return;
                string thang = txtThangNam.Text.Trim();
                ccBUS.SaveChamCong(dtChamCongCache, thang);
                MessageBox.Show("Đồng bộ dữ liệu chấm công về SQL Server thành công bằng Disconnected Model!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu chấm công: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChotLuong_Click(object sender, EventArgs e)
        {
            try
            {
                string thang = txtThangNam.Text.Trim();
                bool result = lBUS.ChotLuongThang(thang);
                if (result)
                {
                    MessageBox.Show($"Chốt lương tháng {thang} thành công cho toàn bộ nhân viên bằng SqlTransaction!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi chốt lương (Đã Rollback giao dịch): " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}