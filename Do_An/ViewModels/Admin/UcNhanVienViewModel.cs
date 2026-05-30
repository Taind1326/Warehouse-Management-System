using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Admin
{
    public class UcNhanVienViewModel : BaseViewModel
    {
        private readonly Action _moForm;
        private readonly Action _veTrangChu;
        private readonly Action _quayLaiDanhSach;

        private ObservableCollection<NhanVienItem> _danhSachNhanVien;
        public ObservableCollection<NhanVienItem> DanhSachNhanVien
        {
            get => _danhSachNhanVien;
            set { _danhSachNhanVien = value; OnPropertyChanged(); }
        }

        private NhanVienItem _nhanVienDangChon;
        public NhanVienItem NhanVienDangChon
        {
            get => _nhanVienDangChon;
            set { _nhanVienDangChon = value; OnPropertyChanged(); }
        }

        private string _tongNhanVien;
        public string TongNhanVien
        {
            get => _tongNhanVien;
            set { _tongNhanVien = value; OnPropertyChanged(); }
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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                TimKiemNhanVien();
            }
        }

        public string TieuDe => IsEdit ? "SỬA NHÂN VIÊN" : "THÊM NHÂN VIÊN";

        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string TrangThai { get; set; } = "1";

        public ICommand ThemCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand ThoatCommand { get; }
        public ICommand HuyCommand { get; }
        public ICommand MoKhoaCommand { get; }

        public UcNhanVienViewModel(Action moForm, Action quayLaiDanhSach, Action veTrangChu)
        {
            _moForm = moForm;
            _quayLaiDanhSach = quayLaiDanhSach;
            _veTrangChu = veTrangChu;

            ThemCommand = new RelayCommand(_ => MoThem());
            SuaCommand = new RelayCommand(_ => MoSua());
            LuuCommand = new RelayCommand(_ => Luu());
            XoaCommand = new RelayCommand(_ => XoaNhanVien());

            HuyCommand = new RelayCommand(_ => QuayLaiDanhSach());   // 👈 về danh sách
            ThoatCommand = new RelayCommand(_ => VeTrangChu());      // 👈 về HOME

            MoKhoaCommand = new RelayCommand(_ => MoKhoaNhanVien());

            LoadNhanVien();
        }

        public void LoadNhanVien()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.NHANVIENs
                    .ToList()
                    .Select((nv, index) => TaoNhanVienItem(nv, index))
                    .ToList();

                DanhSachNhanVien = new ObservableCollection<NhanVienItem>(ds);
                TongNhanVien = ds.Count.ToString();
            }
        }

        private NhanVienItem TaoNhanVienItem(NHANVIEN nv, int index)
        {
            return new NhanVienItem
            {
                STT = index + 1,
                MaNV = nv.MANV,
                HoTen = nv.TENNV,
                NgaySinh = nv.NGAYSINH,
                SoDienThoai = nv.SDT,
                Email = nv.EMAIL,
                TrangThai = nv.TRANGTHAI ? "Đang hoạt động" : "Không hoạt động"
            };
        }
        private void VeTrangChu()
        {
            _veTrangChu();
        }

        private void MoThem()
        {
            IsEdit = false;
            XoaForm();
            _moForm();
        }

        private void MoSua()
        {
            if (NhanVienDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần sửa!");
                return;
            }

            IsEdit = true;
            DoDuLieuLenForm(NhanVienDangChon);
            _moForm();
        }

        private void Luu()
        {
            if (!KiemTraDuLieu())
                return;

            try
            {
                bool luuThanhCong = IsEdit ? SuaNhanVien() : ThemNhanVien();

                if (!luuThanhCong)
                    return;

                MessageBox.Show("Lưu thành công!");
                QuayLaiDanhSach();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Lưu thất bại!\n" + ex.Message);
            }
        }

        private bool ThemNhanVien()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (db.NHANVIENs.Any(x => x.MANV == MaNV))
                {
                    MessageBox.Show("Mã nhân viên đã tồn tại!");
                    return false;
                }

                if (db.NHANVIENs.Any(x => x.EMAIL == Email))
                {
                    MessageBox.Show("Email đã tồn tại!");
                    return false;
                }

                if (db.NHANVIENs.Any(x => x.SDT == SoDienThoai))
                {
                    MessageBox.Show("Số điện thoại đã tồn tại!");
                    return false;
                }

                db.NHANVIENs.Add(TaoNhanVienMoi());

                GhiLog(db, "Thêm nhân viên", MaNV, "Thêm nhân viên " + HoTen);

                db.SaveChanges();
                return true;
            }
        }
        private bool SuaNhanVien()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                MaNV = MaNV?.Trim();
                Email = Email?.Trim();
                SoDienThoai = SoDienThoai?.Trim();

                var nv = db.NHANVIENs.FirstOrDefault(x => x.MANV == MaNV);

                if (nv == null)
                {
                    MessageBox.Show("Không tìm thấy nhân viên cần sửa!");
                    return false;
                }

                bool laChinhMinh = nv.MANV.Trim().Equals(
                    CurrentUser.MaNV?.Trim(),
                    StringComparison.OrdinalIgnoreCase);

                bool muonKhoa = TrangThai == "0";

                if (laChinhMinh && muonKhoa)
                {
                    MessageBox.Show("Admin không thể tự khóa chính mình!");
                    TrangThai = "1";
                    OnPropertyChanged(nameof(TrangThai));
                    return false;
                }

                if (db.NHANVIENs.Any(x => x.MANV != MaNV && x.EMAIL == Email))
                {
                    MessageBox.Show("Email đã tồn tại!");
                    return false;
                }

                if (db.NHANVIENs.Any(x => x.MANV != MaNV && x.SDT == SoDienThoai))
                {
                    MessageBox.Show("Số điện thoại đã tồn tại!");
                    return false;
                }

                CapNhatNhanVien(nv);

                GhiLog(db, "Sửa nhân viên", MaNV, "Sửa thông tin nhân viên " + HoTen);

                db.SaveChanges();
                return true;
            }
        }

        private NHANVIEN TaoNhanVienMoi()
        {
            return new NHANVIEN
            {
                MANV = MaNV,
                TENNV = HoTen,
                NGAYSINH = NgaySinh.Value,
                SDT = SoDienThoai,
                EMAIL = Email,
                TRANGTHAI = TrangThai == "1"
            };
        }

        private void CapNhatNhanVien(NHANVIEN nv)
        {
            nv.TENNV = HoTen;
            nv.NGAYSINH = NgaySinh.Value;
            nv.SDT = SoDienThoai;
            nv.EMAIL = Email;
            nv.TRANGTHAI = TrangThai == "1";
        }
        private void TimKiemNhanVien()
        {
            string tuKhoa = SearchText?.Trim().ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.NHANVIENs
                    .ToList()
                    .Where(nv =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        nv.MANV.ToLower().Contains(tuKhoa) ||
                        nv.TENNV.ToLower().Contains(tuKhoa) ||
                        nv.SDT.ToLower().Contains(tuKhoa) ||
                        nv.EMAIL.ToLower().Contains(tuKhoa))
                    .Select((nv, index) => TaoNhanVienItem(nv, index))
                    .ToList();

                DanhSachNhanVien = new ObservableCollection<NhanVienItem>(ds);
                TongNhanVien = ds.Count.ToString();
            }
        }

        private void XoaNhanVien()
        {
            if (NhanVienDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa!");
                return;
            }

            if (NhanVienDangChon.TrangThai == "Không hoạt động")
            {
                MessageBox.Show("Nhân viên này đã không hoạt động rồi!");
                return;
            }

            if (NhanVienDangChon.MaNV.Trim().Equals(
                CurrentUser.MaNV?.Trim(),
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Bạn không thể tự xóa chính mình!");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa nhân viên này không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var nv = db.NHANVIENs.FirstOrDefault(x => x.MANV == NhanVienDangChon.MaNV);

                    if (nv == null)
                    {
                        MessageBox.Show("Không tìm thấy nhân viên cần xóa!");
                        return;
                    }

                    if (nv.TRANGTHAI == false)
                    {
                        MessageBox.Show("Nhân viên này đã không hoạt động rồi!");
                        return;
                    }

                    nv.TRANGTHAI = false;

                    GhiLog(db, "Xóa nhân viên", nv.MANV, "Khóa trạng thái nhân viên " + nv.TENNV);

                    db.SaveChanges();
                }

                MessageBox.Show("Xóa nhân viên thành công!");
                LoadNhanVien();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xóa nhân viên thất bại!\n" + ex.Message);
            }
        }
        private void MoKhoaNhanVien()
        {
            if (NhanVienDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên!");
                return;
            }

            if (NhanVienDangChon.MaNV.Trim().Equals(
                CurrentUser.MaNV?.Trim(),
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Bạn không thể tự khóa chính mình!");
                return;
            }

            bool dangHoatDong = NhanVienDangChon.TrangThai == "Đang hoạt động";

            string noiDung = dangHoatDong
                ? "Bạn có chắc muốn khóa nhân viên này không?"
                : "Bạn có chắc muốn mở khóa nhân viên này không?";

            var result = MessageBox.Show(
                noiDung,
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var nv = db.NHANVIENs.FirstOrDefault(x => x.MANV == NhanVienDangChon.MaNV);

                    if (nv == null)
                    {
                        MessageBox.Show("Không tìm thấy nhân viên!");
                        return;
                    }

                    string hanhDong = nv.TRANGTHAI ? "Khóa nhân viên" : "Mở khóa nhân viên";

                    string ghiChu = nv.TRANGTHAI
                        ? "Đã khóa nhân viên " + nv.TENNV
                        : "Đã mở khóa nhân viên " + nv.TENNV;

                    nv.TRANGTHAI = !nv.TRANGTHAI;

                    GhiLog(db, hanhDong, nv.MANV, ghiChu);

                    db.SaveChanges();
                }

                MessageBox.Show("Cập nhật trạng thái thành công!");
                LoadNhanVien();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cập nhật trạng thái thất bại!\n" + ex.Message);
            }
        }

        private void DoDuLieuLenForm(NhanVienItem nv)
        {
            MaNV = nv.MaNV;
            HoTen = nv.HoTen;
            NgaySinh = nv.NgaySinh;
            SoDienThoai = nv.SoDienThoai;
            Email = nv.Email;
            TrangThai = nv.TrangThai == "Đang hoạt động" ? "1" : "0";

            BaoThayDoiForm();
        }

        private void XoaForm()
        {
            MaNV = "";
            HoTen = "";
            NgaySinh = null;
            SoDienThoai = "";
            Email = "";
            TrangThai = "1";

            BaoThayDoiForm();
        }

        private bool KiemTraDuLieu()
        {
            MaNV = MaNV?.Trim();
            HoTen = HoTen?.Trim();
            SoDienThoai = SoDienThoai?.Trim();
            Email = Email?.Trim();

            if (string.IsNullOrWhiteSpace(MaNV))
            {
                MessageBox.Show("Vui lòng nhập mã nhân viên!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(HoTen))
            {
                MessageBox.Show("Vui lòng nhập họ tên nhân viên!");
                return false;
            }

            if (HoTen.Length < 2)
            {
                MessageBox.Show("Họ tên phải có ít nhất 2 ký tự!");
                return false;
            }

            if (HoTen.Any(char.IsDigit))
            {
                MessageBox.Show("Họ tên không được chứa số!");
                return false;
            }

            if (NgaySinh == null)
            {
                MessageBox.Show("Ngày sinh không hợp lệ!");
                return false;
            }

            if (NgaySinh.Value > DateTime.Now.AddYears(-18))
            {
                MessageBox.Show("Nhân viên phải đủ 18 tuổi!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Vui lòng nhập email!");
                return false;
            }

            if (!Email.Contains("@") || !Email.Contains(".") ||
                Email.Contains(" ") || Email.StartsWith("@") ||
                Email.Contains(".@") || Email.Count(c => c == '@') != 1)
            {
                MessageBox.Show("Email không đúng định dạng!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SoDienThoai))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!");
                return false;
            }

            if (SoDienThoai.Length != 10)
            {
                MessageBox.Show("Số điện thoại phải gồm 10 số!");
                return false;
            }

            if (!SoDienThoai.StartsWith("0"))
            {
                MessageBox.Show("Số điện thoại phải bắt đầu bằng 0!");
                return false;
            }

            if (SoDienThoai.Any(c => !char.IsDigit(c)))
            {
                MessageBox.Show("Số điện thoại chỉ được chứa số!");
                return false;
            }

            BaoThayDoiForm();
            return true;
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
            LoadNhanVien();
            _quayLaiDanhSach();
        }

        private void BaoThayDoiForm()
        {
            OnPropertyChanged(nameof(MaNV));
            OnPropertyChanged(nameof(HoTen));
            OnPropertyChanged(nameof(NgaySinh));
            OnPropertyChanged(nameof(SoDienThoai));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(TrangThai));
        }
    }

    public class NhanVienItem
    {
        public int STT { get; set; }
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string TrangThai { get; set; }
    }
}