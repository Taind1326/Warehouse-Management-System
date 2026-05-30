using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Admin
{
    public class UcKhoViewModel : BaseViewModel
    {
        private readonly Action _moForm;
        private readonly Action _quayLaiDanhSach;
        private readonly Action _veTrangChu;

        private ObservableCollection<KhoItem> _danhSachKho;
        public ObservableCollection<KhoItem> DanhSachKho
        {
            get => _danhSachKho;
            set { _danhSachKho = value; OnPropertyChanged(); }
        }

        private KhoItem _selectedItem;
        public KhoItem SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
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
                TimKiemKho();
            }
        }

        public string TieuDe => IsEdit ? "SỬA KHO" : "THÊM KHO";

        public string MaKho { get; set; }
        public string TenKho { get; set; }
        public string Hotline { get; set; }
        public string DiaChi { get; set; }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }
        public ICommand ThoatCommand { get; }

        public UcKhoViewModel(Action moForm, Action quayLaiDanhSach, Action veTrangChu)
        {
            _moForm = moForm;
            _quayLaiDanhSach = quayLaiDanhSach;
            _veTrangChu = veTrangChu;

            AddCommand = new RelayCommand(_ => MoThem());
            EditCommand = new RelayCommand(_ => MoSua());
            DeleteCommand = new RelayCommand(_ => XoaKho());

            LuuCommand = new RelayCommand(_ => Luu());
            HuyCommand = new RelayCommand(_ => QuayLaiDanhSach());
            ThoatCommand = new RelayCommand(_ => VeTrangChu());

            LoadKho();
        }

        public void LoadKho()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.KHOes
                    .ToList()
                    .Select((kho, index) => TaoKhoItem(kho, index))
                    .ToList();

                DanhSachKho = new ObservableCollection<KhoItem>(ds);
            }
        }

        private KhoItem TaoKhoItem(KHO kho, int index)
        {
            return new KhoItem
            {
                STT = index + 1,
                MaKho = kho.MAKHO,
                TenKho = kho.TENKHO,
                DiaChi = kho.DIADIEM,
                Hotline = kho.SDT
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
            if (SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn kho cần sửa!");
                return;
            }

            IsEdit = true;
            DoDuLieuLenForm(SelectedItem);
            _moForm();
        }

        private void Luu()
        {
            if (!KiemTraDuLieu())
                return;

            try
            {
                bool luuThanhCong = IsEdit ? SuaKho() : ThemKho();

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

        private bool ThemKho()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (!KiemTraTrungKhiThem(db))
                    return false;

                db.KHOes.Add(TaoKhoMoi());

                GhiLog(db, "Thêm kho", MaKho, "Thêm kho " + TenKho);

                db.SaveChanges();
                return true;
            }
        }

        private bool SuaKho()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var kho = db.KHOes.FirstOrDefault(x => x.MAKHO == MaKho);

                if (kho == null)
                {
                    MessageBox.Show("Không tìm thấy kho cần sửa!");
                    return false;
                }

                if (!KiemTraTrungKhiSua(db))
                    return false;

                CapNhatKho(kho);

                GhiLog(db, "Sửa kho", MaKho, "Sửa thông tin kho " + TenKho);

                db.SaveChanges();
                return true;
            }
        }

        private bool KiemTraTrungKhiThem(QUANLI_KHOHANGEntities db)
        {
            if (db.KHOes.Any(x => x.MAKHO == MaKho))
            {
                MessageBox.Show("Mã kho đã tồn tại!");
                return false;
            }

            if (db.KHOes.Any(x => x.TENKHO == TenKho))
            {
                MessageBox.Show("Tên kho đã tồn tại!");
                return false;
            }

            if (db.KHOes.Any(x => x.SDT == Hotline))
            {
                MessageBox.Show("Số điện thoại đã tồn tại!");
                return false;
            }

            return true;
        }

        private bool KiemTraTrungKhiSua(QUANLI_KHOHANGEntities db)
        {
            if (db.KHOes.Any(x => x.MAKHO != MaKho && x.TENKHO == TenKho))
            {
                MessageBox.Show("Tên kho đã tồn tại!");
                return false;
            }

            if (db.KHOes.Any(x => x.MAKHO != MaKho && x.SDT == Hotline))
            {
                MessageBox.Show("Số điện thoại đã tồn tại!");
                return false;
            }

            return true;
        }

        private KHO TaoKhoMoi()
        {
            return new KHO
            {
                MAKHO = MaKho,
                TENKHO = TenKho,
                DIADIEM = DiaChi,
                SDT = Hotline
            };
        }

        private void CapNhatKho(KHO kho)
        {
            kho.TENKHO = TenKho;
            kho.DIADIEM = DiaChi;
            kho.SDT = Hotline;
        }

        private void XoaKho()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn kho cần xóa!");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa kho này không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var kho = db.KHOes.FirstOrDefault(x => x.MAKHO == SelectedItem.MaKho);

                    if (kho == null)
                    {
                        MessageBox.Show("Không tìm thấy kho cần xóa!");
                        return;
                    }

                    if (KhoDangDuocSuDung(kho))
                        return;

                    db.KHOes.Remove(kho);

                    GhiLog(db, "Xóa kho", kho.MAKHO, "Xóa kho " + kho.TENKHO);

                    db.SaveChanges();
                }

                MessageBox.Show("Xóa kho thành công!");
                LoadKho();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xóa kho thất bại!\n" + ex.Message);
            }
        }

        private bool KhoDangDuocSuDung(KHO kho)
        {
            if (kho.PHANCONG_KHO.Any())
            {
                MessageBox.Show("Không thể xóa kho vì kho này đang có nhân viên được phân công!");
                return true;
            }

            if (kho.TONKHOes.Any())
            {
                MessageBox.Show("Không thể xóa kho vì kho này đang có dữ liệu tồn kho!");
                return true;
            }

            if (kho.PHIEUNHAPs.Any())
            {
                MessageBox.Show("Không thể xóa kho vì kho này đã có phiếu nhập!");
                return true;
            }

            if (kho.PHIEUXUATs.Any())
            {
                MessageBox.Show("Không thể xóa kho vì kho này đã có phiếu xuất!");
                return true;
            }

            if (kho.KIEMKEKHOes.Any())
            {
                MessageBox.Show("Không thể xóa kho vì kho này đã có dữ liệu kiểm kê!");
                return true;
            }

            return false;
        }

        private void TimKiemKho()
        {
            string tuKhoa = SearchText?.Trim().ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.KHOes
                    .ToList()
                    .Where(kho =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        kho.MAKHO.ToLower().Contains(tuKhoa) ||
                        kho.TENKHO.ToLower().Contains(tuKhoa) ||
                        (!string.IsNullOrWhiteSpace(kho.DIADIEM) && kho.DIADIEM.ToLower().Contains(tuKhoa)) ||
                        (!string.IsNullOrWhiteSpace(kho.SDT) && kho.SDT.ToLower().Contains(tuKhoa)))
                    .Select((kho, index) => TaoKhoItem(kho, index))
                    .ToList();

                DanhSachKho = new ObservableCollection<KhoItem>(ds);
            }
        }

        private bool KiemTraDuLieu()
        {
            MaKho = MaKho?.Trim();
            TenKho = TenKho?.Trim();
            Hotline = Hotline?.Trim();
            DiaChi = DiaChi?.Trim();

            if (string.IsNullOrWhiteSpace(MaKho))
            {
                MessageBox.Show("Vui lòng nhập mã kho!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TenKho))
            {
                MessageBox.Show("Vui lòng nhập tên kho!");
                return false;
            }

            if (TenKho.Length < 2)
            {
                MessageBox.Show("Tên kho phải có ít nhất 2 ký tự!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Hotline))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!");
                return false;
            }

            if (Hotline.Length != 10)
            {
                MessageBox.Show("Số điện thoại phải gồm 10 số!");
                return false;
            }

            if (!Hotline.StartsWith("0"))
            {
                MessageBox.Show("Số điện thoại phải bắt đầu bằng 0!");
                return false;
            }

            if (Hotline.Any(c => !char.IsDigit(c)))
            {
                MessageBox.Show("Số điện thoại chỉ được chứa số!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(DiaChi))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ!");
                return false;
            }

            if (DiaChi.Length < 3)
            {
                MessageBox.Show("Địa chỉ phải có ít nhất 3 ký tự!");
                return false;
            }

            BaoThayDoiForm();
            return true;
        }

        private void DoDuLieuLenForm(KhoItem kho)
        {
            MaKho = kho.MaKho;
            TenKho = kho.TenKho;
            Hotline = kho.Hotline;
            DiaChi = kho.DiaChi;

            BaoThayDoiForm();
        }

        private void XoaForm()
        {
            MaKho = "";
            TenKho = "";
            Hotline = "";
            DiaChi = "";

            BaoThayDoiForm();
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
            LoadKho();
            _quayLaiDanhSach();
        }

        private void VeTrangChu()
        {
            _veTrangChu();
        }

        private void BaoThayDoiForm()
        {
            OnPropertyChanged(nameof(MaKho));
            OnPropertyChanged(nameof(TenKho));
            OnPropertyChanged(nameof(Hotline));
            OnPropertyChanged(nameof(DiaChi));
        }
    }

    public class KhoItem
    {
        public int STT { get; set; }
        public string MaKho { get; set; }
        public string TenKho { get; set; }
        public string DiaChi { get; set; }
        public string Hotline { get; set; }
    }
}