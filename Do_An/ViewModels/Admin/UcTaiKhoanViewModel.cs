using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Admin
{
    public class UcTaiKhoanViewModel : BaseViewModel
    {
        private readonly Action _moForm;
        private readonly Action _quayLaiDanhSach;
        private readonly Action _veTrangChu;

        private ObservableCollection<TaiKhoanItem> _danhSachTaiKhoan;
        public ObservableCollection<TaiKhoanItem> DanhSachTaiKhoan
        {
            get => _danhSachTaiKhoan;
            set { _danhSachTaiKhoan = value; OnPropertyChanged(); }
        }

        private ObservableCollection<NhanVienComboItem> _danhSachNhanVien;
        public ObservableCollection<NhanVienComboItem> DanhSachNhanVien
        {
            get => _danhSachNhanVien;
            set { _danhSachNhanVien = value; OnPropertyChanged(); }
        }

        private ObservableCollection<KhoComboItem> _danhSachKho;
        public ObservableCollection<KhoComboItem> DanhSachKho
        {
            get => _danhSachKho;
            set { _danhSachKho = value; OnPropertyChanged(); }
        }

        private KhoComboItem _khoDuocChon;
        public KhoComboItem KhoDuocChon
        {
            get => _khoDuocChon;
            set
            {
                _khoDuocChon = value;
                OnPropertyChanged();

                if (value != null)
                    MaKho = value.MaKho;
            }
        }

        private string _maKho;
        public string MaKho
        {
            get => _maKho;
            set { _maKho = value; OnPropertyChanged(); }
        }

        private NhanVienComboItem _nhanVienDuocChon;
        public NhanVienComboItem NhanVienDuocChon
        {
            get => _nhanVienDuocChon;
            set
            {
                _nhanVienDuocChon = value;
                OnPropertyChanged();

                if (value != null)
                    MaNV = value.MaNV;
            }
        }

        private TaiKhoanItem _taiKhoanDangChon;
        public TaiKhoanItem TaiKhoanDangChon
        {
            get => _taiKhoanDangChon;
            set { _taiKhoanDangChon = value; OnPropertyChanged(); }
        }

        private string _tongTaiKhoan;
        public string TongTaiKhoan
        {
            get => _tongTaiKhoan;
            set { _tongTaiKhoan = value; OnPropertyChanged(); }
        }

        private bool _isEdit;
        public bool IsEdit
        {
            get => _isEdit;
            set
            {
                _isEdit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieuDe));
            }
        }

        public string TieuDe => IsEdit ? "SỬA TÀI KHOẢN" : "THÊM TÀI KHOẢN";

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                LoadTaiKhoan();
            }
        }

        private string _maTK;
        public string MaTK
        {
            get => _maTK;
            set { _maTK = value; OnPropertyChanged(); }
        }

        private string _maNV;
        public string MaNV
        {
            get => _maNV;
            set { _maNV = value; OnPropertyChanged(); }
        }

        private string _tenTK;
        public string TenTK
        {
            get => _tenTK;
            set { _tenTK = value; OnPropertyChanged(); }
        }

        private string _loaiTaiKhoan;
        public string LoaiTaiKhoan
        {
            get => _loaiTaiKhoan;
            set { _loaiTaiKhoan = value; OnPropertyChanged(); }
        }

        private string _trangThai = "1";
        public string TrangThai
        {
            get => _trangThai;
            set { _trangThai = value; OnPropertyChanged(); }
        }

        public ICommand ThemCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand ResetMatKhauCommand { get; }
        public ICommand ThoatCommand { get; }

        public UcTaiKhoanViewModel(Action moForm, Action quayLaiDanhSach, Action veTrangChu)
        {
            _moForm = moForm;
            _quayLaiDanhSach = quayLaiDanhSach;
            _veTrangChu = veTrangChu;

            ThemCommand = new RelayCommand(_ => MoThem());
            SuaCommand = new RelayCommand(_ => MoSua());
            LuuCommand = new RelayCommand(_ => Luu());
            HuyCommand = new RelayCommand(_ => QuayLaiDanhSach());
            XoaCommand = new RelayCommand(_ => XoaTaiKhoan());
            ResetMatKhauCommand = new RelayCommand(_ => ResetMatKhau());
            ThoatCommand = new RelayCommand(_ => VeTrangChu());

            LoadTaiKhoan();
        }

        public void LoadTaiKhoan()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                string tuKhoa = SearchText?.Trim().ToLower();

                var ds = db.TAIKHOANs
                    .ToList()
                    .Select((tk, index) => new TaiKhoanItem
                    {
                        STT = index + 1,
                        MaTK = tk.MATK,
                        MaNV = tk.MANV,
                        ChiNhanh = LayTenChiNhanh(tk),
                        TenTK = tk.TENTK,
                        TrangThai = tk.TRANGTHAI ? "Đang hoạt động" : "Không hoạt động"
                    })
                    .ToList();

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    ds = ds.Where(x =>
                        ChuaTuKhoa(x.MaTK, tuKhoa) ||
                        ChuaTuKhoa(x.MaNV, tuKhoa) ||
                        ChuaTuKhoa(x.ChiNhanh, tuKhoa) ||
                        ChuaTuKhoa(x.TenTK, tuKhoa) ||
                        ChuaTuKhoa(x.TrangThai, tuKhoa)
                    ).ToList();
                }

                DanhSachTaiKhoan = new ObservableCollection<TaiKhoanItem>(ds);
                TongTaiKhoan = ds.Count.ToString();
            }
        }

        private void LoadNhanVienChoCombo()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.NHANVIENs
                    .Where(nv => nv.TRANGTHAI == true)
                    .ToList();

                if (!IsEdit)
                {
                    ds = ds
                        .Where(nv => !db.TAIKHOANs.Any(tk => tk.MANV == nv.MANV))
                        .ToList();
                }

                if (IsEdit && !string.IsNullOrWhiteSpace(MaNV))
                {
                    var nvHienTai = db.NHANVIENs
                        .FirstOrDefault(nv => nv.MANV == MaNV);

                    if (nvHienTai != null && nvHienTai.TRANGTHAI == false)
                    {
                        ds.Add(nvHienTai);
                    }
                }

                DanhSachNhanVien = new ObservableCollection<NhanVienComboItem>(
                    ds.Select(nv => new NhanVienComboItem
                    {
                        MaNV = nv.MANV,
                        TenNV = nv.TENNV
                    })
                );
            }
        }

        private void LoadKhoChoCombo()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.KHOes
                    .ToList()
                    .Select(k => new KhoComboItem
                    {
                        MaKho = k.MAKHO,
                        TenKho = k.TENKHO
                    })
                    .ToList();

                DanhSachKho = new ObservableCollection<KhoComboItem>(ds);
            }
        }

        private bool ChuaTuKhoa(string noiDung, string tuKhoa)
        {
            return !string.IsNullOrWhiteSpace(noiDung) && noiDung.ToLower().Contains(tuKhoa);
        }

        private string LayTenChiNhanh(TAIKHOAN tk)
        {
            var tenKho = tk.PHANCONG_KHO
                .Where(pc => pc.TRANGTHAI == true)
                .Select(pc => pc.KHO.TENKHO)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(tenKho) ? "Chưa phân công" : tenKho;
        }

        private void MoThem()
        {
            IsEdit = false;
            XoaForm();
            LoadNhanVienChoCombo();
            LoadKhoChoCombo();
            _moForm();
        }

        private void MoSua()
        {
            if (TaiKhoanDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần sửa!");
                return;
            }

            IsEdit = true;

            MaNV = TaiKhoanDangChon.MaNV;

            LoadNhanVienChoCombo();
            LoadKhoChoCombo();

            DoDuLieuLenForm(TaiKhoanDangChon);
            _moForm();
        }

        private void Luu()
        {
            if (!KiemTraDuLieu())
                return;

            try
            {
                bool ok = IsEdit ? SuaTaiKhoan() : ThemTaiKhoan();

                if (!ok)
                    return;

                MessageBox.Show("Lưu tài khoản thành công!");
                QuayLaiDanhSach();
            }
            catch (Exception ex)
            {
                string loi = "❌ Lỗi ngoài:\n" + ex.Message;

                Exception inner = ex.InnerException;
                int level = 1;

                while (inner != null)
                {
                    loi += $"\n\n👉 Inner Level {level}:\n{inner.Message}";
                    inner = inner.InnerException;
                    level++;
                }

                MessageBox.Show(loi, "Chi tiết lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ThemTaiKhoan()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                MaTK = MaTK?.Trim();
                MaNV = MaNV?.Trim();
                TenTK = TenTK?.Trim();

                if (db.TAIKHOANs.Any(x => x.MATK == MaTK))
                {
                    MessageBox.Show("Mã tài khoản đã tồn tại!");
                    return false;
                }

                if (db.TAIKHOANs.Any(x => x.TENTK == TenTK))
                {
                    MessageBox.Show("Tên tài khoản đã tồn tại!");
                    return false;
                }

                if (db.TAIKHOANs.Any(x => x.MANV == MaNV))
                {
                    MessageBox.Show("Nhân viên này đã có tài khoản!");
                    return false;
                }

                var vaiTro = LayVaiTroTheoLoai(db);
                if (vaiTro == null)
                {
                    MessageBox.Show("Không tìm thấy vai trò phù hợp!");
                    return false;
                }

                var tk = TaoTaiKhoanMoi();
                db.TAIKHOANs.Add(tk);
                tk.VAITROes.Add(vaiTro);
                db.PHANCONG_KHO.Add(new PHANCONG_KHO
                {
                    MATK = MaTK,
                    MAKHO = MaKho,
                    TRANGTHAI = true
                });

                GhiLog(db, "Thêm tài khoản", MaTK, "Thêm tài khoản " + TenTK);

                db.SaveChanges();
                return true;
            }
        }

        private bool SuaTaiKhoan()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                MaTK = MaTK?.Trim();
                MaNV = MaNV?.Trim();
                TenTK = TenTK?.Trim();

                var tk = db.TAIKHOANs.FirstOrDefault(x => x.MATK == MaTK);

                if (tk == null)
                {
                    MessageBox.Show("Không tìm thấy tài khoản cần sửa!");
                    return false;
                }

                if (db.TAIKHOANs.Any(x => x.MATK != MaTK && x.TENTK == TenTK))
                {
                    MessageBox.Show("Tên tài khoản đã tồn tại!");
                    return false;
                }

                if (db.TAIKHOANs.Any(x => x.MATK != MaTK && x.MANV == MaNV))
                {
                    MessageBox.Show("Nhân viên này đã có tài khoản khác!");
                    return false;
                }

                var vaiTro = LayVaiTroTheoLoai(db);
                if (vaiTro == null)
                {
                    MessageBox.Show("Không tìm thấy vai trò phù hợp!");
                    return false;
                }

                var nv = db.NHANVIENs.FirstOrDefault(x => x.MANV == MaNV);

                if (nv == null)
                {
                    MessageBox.Show("Không tìm thấy nhân viên!");
                    return false;
                }

                if (nv.TRANGTHAI == false)
                {
                    MessageBox.Show("Nhân viên này đang bị khóa, không thể sửa hoặc kích hoạt tài khoản!");
                    return false;
                }


                CapNhatTaiKhoan(tk);
                var pcCu = db.PHANCONG_KHO
                    .FirstOrDefault(x => x.MATK == MaTK && x.TRANGTHAI == true);

                if (pcCu != null)
                {
                    pcCu.TRANGTHAI = false;
                }

                db.PHANCONG_KHO.Add(new PHANCONG_KHO
                {
                    MATK = MaTK,
                    MAKHO = MaKho,
                    TRANGTHAI = true
                });

                tk.VAITROes.Clear();
                tk.VAITROes.Add(vaiTro);

                GhiLog(db, "Sửa tài khoản", MaTK, "Sửa thông tin tài khoản " + TenTK);

                db.SaveChanges();
                return true;
            }
        }

        private TAIKHOAN TaoTaiKhoanMoi()
        {
            return new TAIKHOAN
            {
                MATK = MaTK,
                TENTK = TenTK,
                MANV = MaNV,
                MATKHAU = LayMatKhauTheoLoaiTaiKhoan(),
                TRANGTHAI = TrangThai == "1"
            };
        }

        private void CapNhatTaiKhoan(TAIKHOAN tk)
        {
            tk.TENTK = TenTK;
            tk.MANV = MaNV;
            tk.TRANGTHAI = TrangThai == "1";
            tk.MATKHAU = LayMatKhauTheoLoaiTaiKhoan();
        }

        private VAITRO LayVaiTroTheoLoai(QUANLI_KHOHANGEntities db)
        {
            string tenVT = "";

            if (LoaiTaiKhoan == "NhanVienKho")
                tenVT = "NhanVienKho";

            if (LoaiTaiKhoan == "KeToan")
                tenVT = "KeToan";

            return db.VAITROes.FirstOrDefault(x => x.TENVT == tenVT);
        }

        private string LayMatKhauTheoLoaiTaiKhoan()
        {
            if (LoaiTaiKhoan == "NhanVienKho")
                return "Kho@123";

            if (LoaiTaiKhoan == "KeToan")
                return "Ketoan@123";

            return "";
        }

        private bool KiemTraDuLieu()
        {
            MaTK = MaTK?.Trim();
            MaNV = MaNV?.Trim();
            TenTK = TenTK?.Trim();

            if (string.IsNullOrWhiteSpace(MaTK))
            {
                MessageBox.Show("Vui lòng nhập mã tài khoản!");
                return false;
            }

            if (NhanVienDuocChon == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TenTK))
            {
                MessageBox.Show("Vui lòng nhập tên tài khoản!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(LoaiTaiKhoan))
            {
                MessageBox.Show("Vui lòng chọn loại tài khoản!");
                return false;
            }

            if (KhoDuocChon == null)
            {
                MessageBox.Show("Vui lòng chọn chi nhánh/kho!");
                return false;
            }

            BaoThayDoiForm();
            return true;
        }

        private void DoDuLieuLenForm(TaiKhoanItem tk)
        {
            MaTK = tk.MaTK;
            MaNV = tk.MaNV;
            TenTK = tk.TenTK;
            TrangThai = tk.TrangThai == "Đang hoạt động" ? "1" : "0";

            NhanVienDuocChon = DanhSachNhanVien
                .FirstOrDefault(x => x.MaNV.Trim() == MaNV.Trim());

            using (var db = new QUANLI_KHOHANGEntities())
            {
                LoaiTaiKhoan = db.TAIKHOANs
                    .Where(x => x.MATK == MaTK)
                    .SelectMany(x => x.VAITROes)
                    .Select(x => x.TENVT)
                    .FirstOrDefault();

                MaKho = db.PHANCONG_KHO
                    .Where(x => x.MATK == MaTK && x.TRANGTHAI == true)
                    .Select(x => x.MAKHO)
                    .FirstOrDefault();

                KhoDuocChon = !string.IsNullOrWhiteSpace(MaKho)
                    ? DanhSachKho.FirstOrDefault(x => x.MaKho.Trim() == MaKho.Trim())
                    : null;
            }

            BaoThayDoiForm();
        }

        private void XoaForm()
        {
            MaTK = "";
            MaNV = "";
            TenTK = "";
            LoaiTaiKhoan = "";
            TrangThai = "1";
            NhanVienDuocChon = null;

            BaoThayDoiForm();
        }

        private void BaoThayDoiForm()
        {
            OnPropertyChanged(nameof(MaTK));
            OnPropertyChanged(nameof(MaNV));
            OnPropertyChanged(nameof(TenTK));
            OnPropertyChanged(nameof(LoaiTaiKhoan));
            OnPropertyChanged(nameof(TrangThai));
            OnPropertyChanged(nameof(NhanVienDuocChon));
            OnPropertyChanged(nameof(MaKho));
            OnPropertyChanged(nameof(KhoDuocChon));
        }

        private void XoaTaiKhoan()
        {
            if (TaiKhoanDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần xóa!");
                return;
            }

            if (TaiKhoanDangChon.TrangThai == "Không hoạt động")
            {
                MessageBox.Show("Tài khoản này đã không hoạt động rồi!");
                return;
            }


            if (LaTaiKhoanDangDangNhap())
            {
                MessageBox.Show("Bạn không thể tự xóa tài khoản của chính mình!");
                return;
            }

            var hoi = MessageBox.Show(
                "Bạn có chắc muốn xóa tài khoản này không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (hoi != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var tk = db.TAIKHOANs.FirstOrDefault(x => x.MATK == TaiKhoanDangChon.MaTK);

                    if (tk == null)
                    {
                        MessageBox.Show("Không tìm thấy tài khoản!");
                        return;
                    }

                    if (tk.TRANGTHAI == false)
                    {
                        MessageBox.Show("Tài khoản này đã không hoạt động rồi!");
                        return;
                    }

                    tk.TRANGTHAI = false;

                    GhiLog(db, "Xóa tài khoản", tk.MATK, "Khóa trạng thái tài khoản " + tk.TENTK);

                    db.SaveChanges();
                }

                MessageBox.Show("Xóa tài khoản thành công!");
                LoadTaiKhoan();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xóa tài khoản thất bại!\n" + ex.Message);
            }
        }

        private void ResetMatKhau()
        {
            if (TaiKhoanDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần reset mật khẩu!");
                return;
            }

            var hoi = MessageBox.Show(
                "Bạn có chắc muốn reset mật khẩu tài khoản này không?",
                "Xác nhận reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (hoi != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var tk = db.TAIKHOANs.FirstOrDefault(x => x.MATK == TaiKhoanDangChon.MaTK);

                    if (tk == null)
                    {
                        MessageBox.Show("Không tìm thấy tài khoản!");
                        return;
                    }

                    string matKhauMoi = LayMatKhauTheoVaiTro(tk);
                    tk.MATKHAU = matKhauMoi;

                    GhiLog(db, "Đổi mật khẩu", tk.MATK, "Reset mật khẩu tài khoản " + tk.TENTK);

                    db.SaveChanges();

                    MessageBox.Show("Reset mật khẩu thành công!\nMật khẩu mặc định: " + matKhauMoi);
                }

                LoadTaiKhoan();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reset mật khẩu thất bại!\n" + ex.Message);
            }
        }

        private string LayMatKhauTheoVaiTro(TAIKHOAN tk)
        {
            string tenVT = tk.VAITROes.Select(vt => vt.TENVT).FirstOrDefault();

            if (tenVT == "Admin")
                return "Admin@123";

            if (tenVT == "NhanVienKho")
                return "Kho@123";

            if (tenVT == "KeToan")
                return "Ketoan@123";

            return "Abc@123";
        }

        private bool LaTaiKhoanDangDangNhap()
        {
            if (TaiKhoanDangChon == null)
                return false;

            return TaiKhoanDangChon.MaTK.Trim().Equals(
                CurrentUser.MaTK?.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        private void GhiLog(QUANLI_KHOHANGEntities db, string hanhDong, string doiTuong, string ghiChu)
        {
            db.NHATKies.Add(new NHATKY
            {
                MATK = CurrentUser.MaTK,
                HANHDONG = hanhDong,
                DOITUONG = doiTuong,
                THOIGIAN = DateTime.Now,
                TRANGTHAI = "Thành công",
                GHICHU = ghiChu
            });
        }

        private void QuayLaiDanhSach()
        {
            LoadTaiKhoan();
            _quayLaiDanhSach();
        }

        private void VeTrangChu()
        {
            _veTrangChu();
        }
    }

    public class TaiKhoanItem
    {
        public int STT { get; set; }
        public string MaTK { get; set; }
        public string MaNV { get; set; }
        public string ChiNhanh { get; set; }
        public string TenTK { get; set; }
        public string TrangThai { get; set; }
    }

    public class NhanVienComboItem
    {
        public string MaNV { get; set; }
        public string TenNV { get; set; }
        public string HienThi => MaNV + " - " + TenNV;
    }
}

public class KhoComboItem
{
    public string MaKho { get; set; }
    public string TenKho { get; set; }

    public string HienThi => MaKho + " - " + TenKho;
}