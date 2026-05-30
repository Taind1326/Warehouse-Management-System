using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Admin
{
    public class UcLoaiHangViewModel : BaseViewModel
    {
        private readonly Action _moForm;
        private readonly Action _quayLaiDanhSach;
        private readonly Action _veTrangChu;

        private ObservableCollection<LoaiHangItem> _danhSachLoaiHang;
        public ObservableCollection<LoaiHangItem> DanhSachLoaiHang
        {
            get => _danhSachLoaiHang;
            set
            {
                _danhSachLoaiHang = value;
                OnPropertyChanged();
            }
        }

        private LoaiHangItem _loaiHangDangChon;
        public LoaiHangItem LoaiHangDangChon
        {
            get => _loaiHangDangChon;
            set
            {
                _loaiHangDangChon = value;
                OnPropertyChanged();
            }
        }

        private string _tongLoaiHang;
        public string TongLoaiHang
        {
            get => _tongLoaiHang;
            set
            {
                _tongLoaiHang = value;
                OnPropertyChanged();
            }
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
                TimKiemLoaiHang();
            }
        }

        private string _errorMaLoaiHang;
        public string ErrorMaLoaiHang
        {
            get => _errorMaLoaiHang;
            set
            {
                _errorMaLoaiHang = value;
                OnPropertyChanged();
            }
        }

        private string _errorTenLoaiHang;
        public string ErrorTenLoaiHang
        {
            get => _errorTenLoaiHang;
            set
            {
                _errorTenLoaiHang = value;
                OnPropertyChanged();
            }
        }

        public string TieuDe => IsEdit ? "SỬA LOẠI HÀNG" : "THÊM LOẠI HÀNG";

        public string MaLoaiHang { get; set; }
        public string TenLoaiHang { get; set; }

        public ICommand ThemCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }
        public ICommand ThoatCommand { get; }

        public UcLoaiHangViewModel(
            Action moForm,
            Action quayLaiDanhSach,
            Action veTrangChu)
        {
            _moForm = moForm;
            _quayLaiDanhSach = quayLaiDanhSach;
            _veTrangChu = veTrangChu;

            ThemCommand = new RelayCommand(_ => MoThem());
            SuaCommand = new RelayCommand(_ => MoSua());
            XoaCommand = new RelayCommand(_ => XoaLoaiHang());

            LuuCommand = new RelayCommand(_ => Luu());
            HuyCommand = new RelayCommand(_ => QuayLaiDanhSach());
            ThoatCommand = new RelayCommand(_ => VeTrangChu());

            LoadLoaiHang();
        }

        public void LoadLoaiHang()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.LOAIHANGs
                    .ToList()
                    .Select((lh, index) => TaoLoaiHangItem(lh, index))
                    .ToList();

                DanhSachLoaiHang = new ObservableCollection<LoaiHangItem>(ds);
                TongLoaiHang = ds.Count.ToString();
            }
        }

        private LoaiHangItem TaoLoaiHangItem(LOAIHANG lh, int index)
        {
            return new LoaiHangItem
            {
                STT = index + 1,
                MaLoaiHang = lh.MALOAI,
                TenLoaiHang = lh.TENLOAI
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
            if (LoaiHangDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn loại hàng cần sửa!");
                return;
            }

            IsEdit = true;
            DoDuLieuLenForm(LoaiHangDangChon);
            _moForm();
        }

        private void Luu()
        {
            if (!KiemTraDuLieu())
                return;

            try
            {
                bool thanhCong = IsEdit ? SuaLoaiHang() : ThemLoaiHang();

                if (!thanhCong)
                    return;

                MessageBox.Show("Lưu thành công!");
                QuayLaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lưu thất bại!\n" + ex.Message);
            }
        }

        private bool ThemLoaiHang()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (db.LOAIHANGs.Any(x => x.MALOAI.Trim().ToLower() == MaLoaiHang.ToLower()))
                {
                    MessageBox.Show("Mã loại hàng đã tồn tại!");
                    return false;
                }

                if (db.LOAIHANGs.Any(x => x.TENLOAI.Trim().ToLower() == TenLoaiHang.ToLower()))
                {
                    MessageBox.Show("Tên loại hàng đã tồn tại!");
                    return false;
                }

                db.LOAIHANGs.Add(TaoLoaiHangMoi());

                GhiLog(
                    db,
                    "Thêm loại hàng",
                    MaLoaiHang,
                    "Thêm loại hàng " + TenLoaiHang
                );

                db.SaveChanges();
                return true;
            }
        }

        private bool SuaLoaiHang()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var lh = db.LOAIHANGs.FirstOrDefault(x => x.MALOAI == MaLoaiHang);

                if (lh == null)
                {
                    MessageBox.Show("Không tìm thấy loại hàng cần sửa!");
                    return false;
                }

                bool tenDaTonTai = db.LOAIHANGs.Any(x =>
                    x.MALOAI != MaLoaiHang &&
                    x.TENLOAI.Trim().ToLower() == TenLoaiHang.ToLower());

                if (tenDaTonTai)
                {
                    MessageBox.Show("Tên loại hàng đã tồn tại!");
                    return false;
                }

                CapNhatLoaiHang(lh);

                GhiLog(
                    db,
                    "Sửa loại hàng",
                    MaLoaiHang,
                    "Sửa loại hàng " + TenLoaiHang
                );

                db.SaveChanges();
                return true;
            }
        }

        private LOAIHANG TaoLoaiHangMoi()
        {
            return new LOAIHANG
            {
                MALOAI = MaLoaiHang,
                TENLOAI = TenLoaiHang
            };
        }

        private void CapNhatLoaiHang(LOAIHANG lh)
        {
            lh.TENLOAI = TenLoaiHang;
        }

        private void XoaLoaiHang()
        {
            if (LoaiHangDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn loại hàng cần xóa!");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa loại hàng này không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var lh = db.LOAIHANGs.FirstOrDefault(x =>
                        x.MALOAI == LoaiHangDangChon.MaLoaiHang);

                    if (lh == null)
                    {
                        MessageBox.Show("Không tìm thấy loại hàng cần xóa!");
                        return;
                    }

                    if (lh.SANPHAMs.Any())
                    {
                        MessageBox.Show("Không thể xóa vì loại hàng này đã có sản phẩm sử dụng!");
                        return;
                    }

                    db.LOAIHANGs.Remove(lh);

                    GhiLog(
                        db,
                        "Xóa loại hàng",
                        lh.MALOAI,
                        "Xóa loại hàng " + lh.TENLOAI
                    );

                    db.SaveChanges();
                }

                MessageBox.Show("Xóa loại hàng thành công!");
                LoadLoaiHang();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xóa loại hàng thất bại!\n" + ex.Message);
            }
        }

        private void TimKiemLoaiHang()
        {
            string tuKhoa = SearchText?.Trim().ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.LOAIHANGs
                    .ToList()
                    .Where(lh =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        lh.MALOAI.ToLower().Contains(tuKhoa) ||
                        lh.TENLOAI.ToLower().Contains(tuKhoa))
                    .Select((lh, index) => TaoLoaiHangItem(lh, index))
                    .ToList();

                DanhSachLoaiHang = new ObservableCollection<LoaiHangItem>(ds);
                TongLoaiHang = ds.Count.ToString();
            }
        }

        private void DoDuLieuLenForm(LoaiHangItem lh)
        {
            MaLoaiHang = lh.MaLoaiHang;
            TenLoaiHang = lh.TenLoaiHang;

            ErrorMaLoaiHang = "";
            ErrorTenLoaiHang = "";

            BaoThayDoiForm();
        }

        private void XoaForm()
        {
            MaLoaiHang = "";
            TenLoaiHang = "";

            ErrorMaLoaiHang = "";
            ErrorTenLoaiHang = "";

            BaoThayDoiForm();
        }

        private bool KiemTraDuLieu()
        {
            MaLoaiHang = MaLoaiHang?.Trim();
            TenLoaiHang = TenLoaiHang?.Trim();

            ErrorMaLoaiHang = "";
            ErrorTenLoaiHang = "";

            bool hopLe = true;

            if (string.IsNullOrWhiteSpace(MaLoaiHang))
            {
                ErrorMaLoaiHang = "Vui lòng nhập mã loại hàng!";
                hopLe = false;
            }
            else if (MaLoaiHang.Length < 2)
            {
                ErrorMaLoaiHang = "Mã loại hàng phải có ít nhất 2 ký tự!";
                hopLe = false;
            }
            else if (MaLoaiHang.Contains(" "))
            {
                ErrorMaLoaiHang = "Mã loại hàng không được chứa khoảng trắng!";
                hopLe = false;
            }

            if (string.IsNullOrWhiteSpace(TenLoaiHang))
            {
                ErrorTenLoaiHang = "Vui lòng nhập tên loại hàng!";
                hopLe = false;
            }
            else if (TenLoaiHang.Length < 2)
            {
                ErrorTenLoaiHang = "Tên loại hàng phải có ít nhất 2 ký tự!";
                hopLe = false;
            }
            else if (TenLoaiHang.Any(char.IsDigit))
            {
                ErrorTenLoaiHang = "Tên loại hàng không nên chứa số!";
                hopLe = false;
            }

            BaoThayDoiForm();

            if (!hopLe)
                MessageBox.Show("Vui lòng kiểm tra lại thông tin!");

            return hopLe;
        }

        private void GhiLog(
            QUANLI_KHOHANGEntities db,
            string hanhDong,
            string doiTuong,
            string ghiChu)
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
            LoadLoaiHang();
            _quayLaiDanhSach();
        }

        private void VeTrangChu()
        {
            _veTrangChu();
        }

        private void BaoThayDoiForm()
        {
            OnPropertyChanged(nameof(MaLoaiHang));
            OnPropertyChanged(nameof(TenLoaiHang));
            OnPropertyChanged(nameof(ErrorMaLoaiHang));
            OnPropertyChanged(nameof(ErrorTenLoaiHang));
        }
    }

    public class LoaiHangItem
    {
        public int STT { get; set; }
        public string MaLoaiHang { get; set; }
        public string TenLoaiHang { get; set; }
    }
}