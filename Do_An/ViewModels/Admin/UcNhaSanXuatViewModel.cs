using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Admin
{
    public class UcNhaSanXuatViewModel : BaseViewModel
    {
        private readonly Action _moForm;
        private readonly Action _quayLaiDanhSach;
        private readonly Action _veTrangChu;

        private ObservableCollection<NhaSanXuatItem> _danhSachNhaSanXuat;
        public ObservableCollection<NhaSanXuatItem> DanhSachNhaSanXuat
        {
            get => _danhSachNhaSanXuat;
            set { _danhSachNhaSanXuat = value; OnPropertyChanged(); }
        }

        private NhaSanXuatItem _nhaSanXuatDangChon;
        public NhaSanXuatItem NhaSanXuatDangChon
        {
            get => _nhaSanXuatDangChon;
            set { _nhaSanXuatDangChon = value; OnPropertyChanged(); }
        }

        private string _tongNhaSanXuat;
        public string TongNhaSanXuat
        {
            get => _tongNhaSanXuat;
            set { _tongNhaSanXuat = value; OnPropertyChanged(); }
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
                TimKiemNhaSanXuat();
            }
        }

        public string TieuDe => IsEdit ? "SỬA NHÀ SẢN XUẤT" : "THÊM NHÀ SẢN XUẤT";

        public string MaNSX { get; set; }
        public string TenNSX { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }

        public ICommand ThemCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }
        public ICommand ThoatCommand { get; }

        public UcNhaSanXuatViewModel(Action moForm, Action quayLaiDanhSach, Action veTrangChu)
        {
            _moForm = moForm;
            _quayLaiDanhSach = quayLaiDanhSach;
            _veTrangChu = veTrangChu;

            ThemCommand = new RelayCommand(_ => MoThem());
            SuaCommand = new RelayCommand(_ => MoSua());
            XoaCommand = new RelayCommand(_ => XoaNhaSanXuat());
            LuuCommand = new RelayCommand(_ => Luu());

            HuyCommand = new RelayCommand(_ => QuayLaiDanhSach());
            ThoatCommand = new RelayCommand(_ => VeTrangChu());

            LoadNhaSanXuat();
        }

        public void LoadNhaSanXuat()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.NHASANXUATs
                    .ToList()
                    .Select((nsx, index) => TaoNhaSanXuatItem(nsx, index))
                    .ToList();

                DanhSachNhaSanXuat = new ObservableCollection<NhaSanXuatItem>(ds);
                TongNhaSanXuat = ds.Count.ToString();
            }
        }

        private NhaSanXuatItem TaoNhaSanXuatItem(NHASANXUAT nsx, int index)
        {
            return new NhaSanXuatItem
            {
                STT = index + 1,
                MaNSX = nsx.MANSX,
                TenNSX = nsx.TENNSX,
                SoDienThoai = nsx.SDT,
                Email = nsx.EMAIL,
                DiaChi = nsx.DIACHI
            };
        }

        private void MoThem()
        {
            IsEdit = false;
            XoaForm();
            _moForm();
        }

        private void MoSua()
        {
            if (NhaSanXuatDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn nhà sản xuất cần sửa!");
                return;
            }

            IsEdit = true;
            DoDuLieuLenForm(NhaSanXuatDangChon);
            _moForm();
        }

        private void Luu()
        {
            if (!KiemTraDuLieu())
                return;

            try
            {
                bool luuThanhCong = IsEdit ? SuaNhaSanXuat() : ThemNhaSanXuat();

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

        private bool ThemNhaSanXuat()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (db.NHASANXUATs.Any(x => x.MANSX == MaNSX))
                {
                    MessageBox.Show("Mã nhà sản xuất đã tồn tại!");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(Email) &&
                    db.NHASANXUATs.Any(x => x.EMAIL == Email))
                {
                    MessageBox.Show("Email đã tồn tại!");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(SoDienThoai) &&
                    db.NHASANXUATs.Any(x => x.SDT == SoDienThoai))
                {
                    MessageBox.Show("Số điện thoại đã tồn tại!");
                    return false;
                }

                db.NHASANXUATs.Add(TaoNhaSanXuatMoi());

                GhiLog(db, "Thêm nhà sản xuất", MaNSX, "Thêm nhà sản xuất " + TenNSX);

                db.SaveChanges();
                return true;
            }
        }

        private bool SuaNhaSanXuat()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var nsx = db.NHASANXUATs.FirstOrDefault(x => x.MANSX == MaNSX);

                if (nsx == null)
                {
                    MessageBox.Show("Không tìm thấy nhà sản xuất cần sửa!");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(Email) &&
                    db.NHASANXUATs.Any(x => x.MANSX != MaNSX && x.EMAIL == Email))
                {
                    MessageBox.Show("Email đã tồn tại!");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(SoDienThoai) &&
                    db.NHASANXUATs.Any(x => x.MANSX != MaNSX && x.SDT == SoDienThoai))
                {
                    MessageBox.Show("Số điện thoại đã tồn tại!");
                    return false;
                }

                CapNhatNhaSanXuat(nsx);

                GhiLog(db, "Sửa nhà sản xuất", MaNSX, "Sửa thông tin nhà sản xuất " + TenNSX);

                db.SaveChanges();
                return true;
            }
        }

        private NHASANXUAT TaoNhaSanXuatMoi()
        {
            return new NHASANXUAT
            {
                MANSX = MaNSX,
                TENNSX = TenNSX,
                SDT = SoDienThoai,
                EMAIL = Email,
                DIACHI = DiaChi
            };
        }

        private void CapNhatNhaSanXuat(NHASANXUAT nsx)
        {
            nsx.TENNSX = TenNSX;
            nsx.SDT = SoDienThoai;
            nsx.EMAIL = Email;
            nsx.DIACHI = DiaChi;
        }

        private void XoaNhaSanXuat()
        {
            if (NhaSanXuatDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn nhà sản xuất cần xóa!");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa nhà sản xuất này không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var nsx = db.NHASANXUATs.FirstOrDefault(x => x.MANSX == NhaSanXuatDangChon.MaNSX);

                    if (nsx == null)
                    {
                        MessageBox.Show("Không tìm thấy nhà sản xuất cần xóa!");
                        return;
                    }

                    bool dangCoPhieuNhap = db.PHIEUNHAPs.Any(x => x.MANSX == nsx.MANSX);

                    if (dangCoPhieuNhap)
                    {
                        MessageBox.Show("Không thể xóa vì nhà sản xuất này đã phát sinh phiếu nhập!");
                        return;
                    }

                    db.NHASANXUATs.Remove(nsx);

                    GhiLog(db, "Xóa nhà sản xuất", nsx.MANSX, "Xóa nhà sản xuất " + nsx.TENNSX);

                    db.SaveChanges();
                }

                MessageBox.Show("Xóa nhà sản xuất thành công!");
                LoadNhaSanXuat();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xóa nhà sản xuất thất bại!\n" + ex.Message);
            }
        }

        private void TimKiemNhaSanXuat()
        {
            string tuKhoa = SearchText?.Trim().ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.NHASANXUATs
                    .ToList()
                    .Where(nsx =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        nsx.MANSX.ToLower().Contains(tuKhoa) ||
                        nsx.TENNSX.ToLower().Contains(tuKhoa) ||
                        (!string.IsNullOrWhiteSpace(nsx.SDT) && nsx.SDT.ToLower().Contains(tuKhoa)) ||
                        (!string.IsNullOrWhiteSpace(nsx.EMAIL) && nsx.EMAIL.ToLower().Contains(tuKhoa)) ||
                        (!string.IsNullOrWhiteSpace(nsx.DIACHI) && nsx.DIACHI.ToLower().Contains(tuKhoa)))
                    .Select((nsx, index) => TaoNhaSanXuatItem(nsx, index))
                    .ToList();

                DanhSachNhaSanXuat = new ObservableCollection<NhaSanXuatItem>(ds);
                TongNhaSanXuat = ds.Count.ToString();
            }
        }

        private void DoDuLieuLenForm(NhaSanXuatItem nsx)
        {
            MaNSX = nsx.MaNSX;
            TenNSX = nsx.TenNSX;
            SoDienThoai = nsx.SoDienThoai;
            Email = nsx.Email;
            DiaChi = nsx.DiaChi;

            BaoThayDoiForm();
        }

        private void XoaForm()
        {
            MaNSX = "";
            TenNSX = "";
            SoDienThoai = "";
            Email = "";
            DiaChi = "";

            BaoThayDoiForm();
        }

        private bool KiemTraDuLieu()
        {
            MaNSX = MaNSX?.Trim();
            TenNSX = TenNSX?.Trim();
            SoDienThoai = SoDienThoai?.Trim();
            Email = Email?.Trim();
            DiaChi = DiaChi?.Trim();

            if (string.IsNullOrWhiteSpace(MaNSX))
            {
                MessageBox.Show("Vui lòng nhập mã nhà sản xuất!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TenNSX))
            {
                MessageBox.Show("Vui lòng nhập tên nhà sản xuất!");
                return false;
            }

            if (TenNSX.Length < 2)
            {
                MessageBox.Show("Tên nhà sản xuất phải có ít nhất 2 ký tự!");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(SoDienThoai))
            {
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
            }

            if (!string.IsNullOrWhiteSpace(Email))
            {
                if (!Email.Contains("@") || !Email.Contains(".") ||
                    Email.Contains(" ") || Email.StartsWith("@") ||
                    Email.Contains(".@") || Email.Count(c => c == '@') != 1)
                {
                    MessageBox.Show("Email không đúng định dạng!");
                    return false;
                }
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
            LoadNhaSanXuat();
            _quayLaiDanhSach();
        }

        private void VeTrangChu()
        {
            _veTrangChu();
        }

        private void BaoThayDoiForm()
        {
            OnPropertyChanged(nameof(MaNSX));
            OnPropertyChanged(nameof(TenNSX));
            OnPropertyChanged(nameof(SoDienThoai));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(DiaChi));
        }
    }

    public class NhaSanXuatItem
    {
        public int STT { get; set; }
        public string MaNSX { get; set; }
        public string TenNSX { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
    }
}