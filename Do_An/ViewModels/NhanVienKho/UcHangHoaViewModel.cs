using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.NhanVienKho
{
    public class UcHangHoaViewModel : BaseViewModel
    {
        private readonly Action _veTrangChu;

        private ObservableCollection<HangHoaItem> _danhSachHangHoa;
        public ObservableCollection<HangHoaItem> DanhSachHangHoa
        {
            get => _danhSachHangHoa;
            set
            {
                _danhSachHangHoa = value;
                OnPropertyChanged();
            }
        }

        private HangHoaItem _hangHoaDangChon;
        public HangHoaItem HangHoaDangChon
        {
            get => _hangHoaDangChon;
            set
            {
                _hangHoaDangChon = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<LoaiHangFilterItem> _danhSachLoaiHang;
        public ObservableCollection<LoaiHangFilterItem> DanhSachLoaiHang
        {
            get => _danhSachLoaiHang;
            set
            {
                _danhSachLoaiHang = value;
                OnPropertyChanged();
            }
        }

        private LoaiHangFilterItem _loaiHangDuocChon;
        public LoaiHangFilterItem LoaiHangDuocChon
        {
            get => _loaiHangDuocChon;
            set
            {
                _loaiHangDuocChon = value;
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
                TimKiemHangHoa();
            }
        }

        private string _tongHangHoa;
        public string TongHangHoa
        {
            get => _tongHangHoa;
            set
            {
                _tongHangHoa = value;
                OnPropertyChanged();
            }
        }

        public ICommand LocCommand { get; }
        public ICommand ThoatCommand { get; }

        public UcHangHoaViewModel(Action veTrangChu)
        {
            _veTrangChu = veTrangChu;

            LocCommand = new RelayCommand(p => LoadHangHoa());
            ThoatCommand = new RelayCommand(p => VeTrangChu());

            LoadLoaiHangFilter();
            LoadHangHoa();
        }

        public void LoadHangHoa()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                string maKho = LayMaKhoDangPhanCong(db);

                if (string.IsNullOrWhiteSpace(maKho))
                {
                    DanhSachHangHoa = new ObservableCollection<HangHoaItem>();
                    TongHangHoa = "0";
                    MessageBox.Show("Tài khoản hiện chưa được phân công kho!");
                    return;
                }

                string maLoai = LoaiHangDuocChon?.MaLoai;

                var ds = db.TONKHOes
                    .Where(x => x.MAKHO == maKho)
                    .ToList()
                    .Where(x =>
                        string.IsNullOrWhiteSpace(maLoai) ||
                        maLoai == "ALL" ||
                        x.SANPHAM.MALOAI == maLoai)
                    .Select((tonKho, index) => TaoHangHoaItem(tonKho, index, maKho))
                    .ToList();

                DanhSachHangHoa = new ObservableCollection<HangHoaItem>(ds);
                TongHangHoa = ds.Count.ToString();
            }
        }

        private void LoadLoaiHangFilter()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.LOAIHANGs
                    .ToList()
                    .Select(x => new LoaiHangFilterItem
                    {
                        MaLoai = x.MALOAI,
                        TenLoai = x.TENLOAI
                    })
                    .ToList();

                ds.Insert(0, new LoaiHangFilterItem
                {
                    MaLoai = "ALL",
                    TenLoai = "Tất cả loại hàng"
                });

                DanhSachLoaiHang = new ObservableCollection<LoaiHangFilterItem>(ds);
                LoaiHangDuocChon = DanhSachLoaiHang.FirstOrDefault();
            }
        }

        private string LayMaKhoDangPhanCong(QUANLI_KHOHANGEntities db)
        {
            return db.PHANCONG_KHO
                .Where(x =>
                    x.MATK == CurrentUser.MaTK &&
                    x.TRANGTHAI == true)
                .Select(x => x.MAKHO)
                .FirstOrDefault();
        }

        private HangHoaItem TaoHangHoaItem(TONKHO tonKho, int index, string maKho)
        {
            SANPHAM sp = tonKho.SANPHAM;

            return new HangHoaItem
            {
                STT = index + 1,
                MaHang = sp.MASP,
                TenHang = sp.TENSP,
                TenLoaiHang = sp.LOAIHANG?.TENLOAI ?? "",
                TenNSX = LayTenNSXGanNhat(sp, maKho),
                DonViTinh = sp.DONVITINH?.TENDVT ?? "",
                GiaNhap = DinhDangTien(sp.DONGIA),
                SoLuongTon = tonKho.SOLUONGTON.ToString()
            };
        }

        private string LayTenNSXGanNhat(SANPHAM sp, string maKho)
        {
            var ct = sp.CT_PHIEUNHAP
                .Where(x => x.PHIEUNHAP.MAKHO == maKho)
                .OrderByDescending(x => x.PHIEUNHAP.NGAYNHAP)
                .FirstOrDefault();

            if (ct == null)
                return "";

            return ct.PHIEUNHAP?.NHASANXUAT?.TENNSX ?? "";
        }

        private void TimKiemHangHoa()
        {
            string tuKhoa = SearchText?
                .Trim()
                .ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                string maKho = LayMaKhoDangPhanCong(db);

                if (string.IsNullOrWhiteSpace(maKho))
                {
                    DanhSachHangHoa = new ObservableCollection<HangHoaItem>();
                    TongHangHoa = "0";
                    return;
                }

                string maLoai = LoaiHangDuocChon?.MaLoai;

                var ds = db.TONKHOes
                    .Where(x => x.MAKHO == maKho)
                    .ToList()
                    .Where(x =>
                        KiemTraDungLoaiHang(x, maLoai) &&
                        KiemTraDungTuKhoa(x, tuKhoa))
                    .Select((tonKho, index) => TaoHangHoaItem(tonKho, index, maKho))
                    .ToList();

                DanhSachHangHoa = new ObservableCollection<HangHoaItem>(ds);
                TongHangHoa = ds.Count.ToString();
            }
        }

        private bool KiemTraDungLoaiHang(TONKHO tonKho, string maLoai)
        {
            if (string.IsNullOrWhiteSpace(maLoai) || maLoai == "ALL")
                return true;

            return tonKho.SANPHAM.MALOAI == maLoai;
        }

        private bool KiemTraDungTuKhoa(TONKHO tonKho, string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
                return true;

            SANPHAM sp = tonKho.SANPHAM;

            return
                sp.MASP.ToLower().Contains(tuKhoa) ||
                sp.TENSP.ToLower().Contains(tuKhoa) ||
                (sp.LOAIHANG != null &&
                 sp.LOAIHANG.TENLOAI.ToLower().Contains(tuKhoa)) ||
                (sp.DONVITINH != null &&
                 sp.DONVITINH.TENDVT.ToLower().Contains(tuKhoa));
        }

        private string DinhDangTien(decimal soTien)
        {
            return soTien.ToString(
                "N0",
                CultureInfo.GetCultureInfo("vi-VN"));
        }

        private void VeTrangChu()
        {
            _veTrangChu();
        }
    }

    public class LoaiHangFilterItem
    {
        public string MaLoai { get; set; }
        public string TenLoai { get; set; }
    }

    public class HangHoaItem
    {
        public int STT { get; set; }
        public string MaHang { get; set; }
        public string TenHang { get; set; }
        public string TenLoaiHang { get; set; }
        public string TenNSX { get; set; }
        public string DonViTinh { get; set; }
        public string GiaNhap { get; set; }
        public string SoLuongTon { get; set; }
    }
}