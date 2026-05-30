using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Do_An.ViewModels.Admin
{
    public class UcTonKhoViewModel : BaseViewModel
    {
        private readonly Action _veTrangChu;
        
        private ObservableCollection<TonKhoItem> _danhSachTonKho;
        public ObservableCollection<TonKhoItem> DanhSachTonKho
        {
            get => _danhSachTonKho;
            set
            {
                _danhSachTonKho = value;
                OnPropertyChanged();
            }
        }

        private TonKhoItem _selectedItem;
        public TonKhoItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
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
                TimKiemTonKho();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ThoatCommand { get; }

        public UcTonKhoViewModel(Action veTrangChu)
        {
            _veTrangChu = veTrangChu;

            DanhSachTonKho = new ObservableCollection<TonKhoItem>();

            RefreshCommand = new RelayCommand(_ => LoadTonKho());
            ThoatCommand = new RelayCommand(_ => VeTrangChu());

            LoadTonKho();
        }

        public void LoadTonKho()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.TONKHOes
                    .ToList()
                    .Select((tk, index) => TaoTonKhoItem(tk, index))
                    .ToList();

                DanhSachTonKho = new ObservableCollection<TonKhoItem>(ds);
            }
        }

        private TonKhoItem TaoTonKhoItem(TONKHO tonKho, int index)
        {
            int soLuongTon = tonKho.SOLUONGTON;
            int mucToiThieu = tonKho.SANPHAM?.MUCTONTOITHIEU ?? 0;

            return new TonKhoItem
            {
                STT = index + 1,
                MaKho = tonKho.MAKHO,
                TenKho = tonKho.KHO?.TENKHO ?? "",
                MaSanPham = tonKho.MASP,
                TenSanPham = tonKho.SANPHAM?.TENSP ?? "",
                SoLuongTon = soLuongTon,
                SoLuongToiThieu = mucToiThieu,
                DonViTinh = tonKho.SANPHAM?.DONVITINH?.TENDVT ?? "",
                TrangThaiTon = LayTrangThaiTon(soLuongTon, mucToiThieu)
            };
        }

        private void TimKiemTonKho()
        {
            string tuKhoa = SearchText?.Trim().ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.TONKHOes
                    .ToList()
                    .Where(tk =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        tk.MAKHO.ToLower().Contains(tuKhoa) ||
                        tk.MASP.ToLower().Contains(tuKhoa) ||
                        (tk.KHO != null &&
                         tk.KHO.TENKHO.ToLower().Contains(tuKhoa)) ||
                        (tk.SANPHAM != null &&
                         tk.SANPHAM.TENSP.ToLower().Contains(tuKhoa)) ||
                        (tk.SANPHAM != null &&
                         tk.SANPHAM.DONVITINH != null &&
                         tk.SANPHAM.DONVITINH.TENDVT.ToLower().Contains(tuKhoa)))
                    .Select((tk, index) => TaoTonKhoItem(tk, index))
                    .ToList();

                DanhSachTonKho = new ObservableCollection<TonKhoItem>(ds);
            }
        }

        private string LayTrangThaiTon(int soLuongTon, int mucToiThieu)
        {
            if (soLuongTon <= 0)
                return "Hết hàng";

            if (soLuongTon <= mucToiThieu)
                return "Sắp hết";

            return "Ổn định";
        }

        private void VeTrangChu()
        {
            _veTrangChu?.Invoke();
        }

    }

    public class TonKhoItem
    {
        public int STT { get; set; }
        public string MaKho { get; set; }
        public string TenKho { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuongTon { get; set; }
        public int SoLuongToiThieu { get; set; }
        public string DonViTinh { get; set; }
        public string TrangThaiTon { get; set; }
    }
}