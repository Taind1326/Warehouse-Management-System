using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Do_An.ViewModels.Shared
{
    public class UcThongKeNhapKhoViewModel : INotifyPropertyChanged
    {
        private readonly Action _thoat;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private int _tongPhieuNhap;
        public int TongPhieuNhap
        {
            get => _tongPhieuNhap;
            set
            {
                _tongPhieuNhap = value;
                OnPropertyChanged();
            }
        }

        private int _tongSoLuongNhap;
        public int TongSoLuongNhap
        {
            get => _tongSoLuongNhap;
            set
            {
                _tongSoLuongNhap = value;
                OnPropertyChanged();
            }
        }

        private decimal _tongGiaTriNhap;
        public decimal TongGiaTriNhap
        {
            get => _tongGiaTriNhap;
            set
            {
                _tongGiaTriNhap = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TongGiaTriNhapText));
            }
        }

        public string TongGiaTriNhapText =>
            TongGiaTriNhap.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";

        private List<BarItemNhapKho> _chartData;
        public List<BarItemNhapKho> ChartData
        {
            get => _chartData;
            set
            {
                _chartData = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TopSanPhamNhapItem> _danhSachTopSanPham;
        public ObservableCollection<TopSanPhamNhapItem> DanhSachTopSanPham
        {
            get => _danhSachTopSanPham;
            set
            {
                _danhSachTopSanPham = value;
                OnPropertyChanged();
            }
        }

        private double _maxValue = 1;
        public double MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                OnPropertyChanged();
            }
        }

        public ICommand ThoatCommand { get; }

        public UcThongKeNhapKhoViewModel(Action thoat = null)
        {
            _thoat = thoat;

            ThoatCommand = new RelayCommand(_ => _thoat?.Invoke());

            ChartData = new List<BarItemNhapKho>();
            DanhSachTopSanPham = new ObservableCollection<TopSanPhamNhapItem>();

            LoadData();
        }

        private void LoadData()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var dsPhieuNhap = LocPhieuNhapTheoTaiKhoan(db)
                    .Where(pn => pn.TRANGTHAI != "Đã hủy")
                    .ToList();

                var dsMaPhieu = dsPhieuNhap
                    .Select(pn => pn.MAPN)
                    .ToList();

                var dsChiTiet = db.CT_PHIEUNHAP
                    .Where(ct => dsMaPhieu.Contains(ct.MAPN))
                    .ToList();

                TongPhieuNhap = dsPhieuNhap.Count;

                TongSoLuongNhap = dsChiTiet.Sum(ct => ct.SOLUONG);

                TongGiaTriNhap = dsChiTiet.Sum(ct => ct.SOLUONG * ct.DONGIA);

                LoadBieuDoTheoKho(dsPhieuNhap, dsChiTiet);

                LoadTopSanPham(dsChiTiet);
            }
        }

        private void LoadBieuDoTheoKho(
            List<PHIEUNHAP> dsPhieuNhap,
            List<CT_PHIEUNHAP> dsChiTiet)
        {
            var ds = dsPhieuNhap
                .GroupBy(pn => new
                {
                    pn.MAKHO,
                    TenKho = pn.KHO != null ? pn.KHO.TENKHO : pn.MAKHO
                })
                .Select(g => new
                {
                    TenKho = g.Key.TenKho,

                    SoLuong = dsChiTiet
                        .Where(ct => g.Select(pn => pn.MAPN).Contains(ct.MAPN))
                        .Sum(ct => ct.SOLUONG)
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToList();

            double max = ds.Count == 0 ? 1 : ds.Max(x => x.SoLuong);

            if (max < 10)
                max = 10;

            MaxValue = max;

            string[] mauCot =
            {
                "#FFB58A6A",
                "#FFC49478",
                "#FF8B6251",
                "#FFD0A48A",
                "#FF9C6F5A"
            };

            ChartData = ds
                .Select((x, index) => new BarItemNhapKho
                {
                    Label = x.TenKho,
                    Value = x.SoLuong,
                    Color = mauCot[index % mauCot.Length],
                    MaxValue = MaxValue
                })
                .ToList();
        }

        private void LoadTopSanPham(List<CT_PHIEUNHAP> dsChiTiet)
        {
            var ds = dsChiTiet
                .GroupBy(ct => new
                {
                    ct.MASP,
                    TenSP = ct.SANPHAM != null
                        ? ct.SANPHAM.TENSP
                        : ct.MASP
                })
                .Select(g => new TopSanPhamNhapItem
                {
                    TenSanPham = g.Key.TenSP,
                    SoLuongNhap = g.Sum(x => x.SOLUONG),
                    GiaTri = g.Sum(x => x.SOLUONG * x.DONGIA)
                })
                .OrderByDescending(x => x.SoLuongNhap)
                .Take(8)
                .ToList();

            for (int i = 0; i < ds.Count; i++)
            {
                ds[i].STT = i + 1;
            }

            DanhSachTopSanPham =
                new ObservableCollection<TopSanPhamNhapItem>(ds);
        }

        private IQueryable<PHIEUNHAP> LocPhieuNhapTheoTaiKhoan(QUANLI_KHOHANGEntities db)
        {
            if (LaAdmin(db))
                return db.PHIEUNHAPs;

            string maTK = CurrentUser.MaTK?.Trim();

            return db.PHIEUNHAPs.Where(pn =>
                db.PHANCONG_KHO.Any(pc =>
                    pc.MATK == maTK &&
                    pc.MAKHO == pn.MAKHO &&
                    pc.TRANGTHAI == true));
        }

        private bool LaAdmin(QUANLI_KHOHANGEntities db)
        {
            string maTK = CurrentUser.MaTK?.Trim();

            var taiKhoan = db.TAIKHOANs
                .FirstOrDefault(tk => tk.MATK == maTK);

            return taiKhoan != null &&
                   taiKhoan.VAITROes.Any(vt => vt.TENVT == "Admin");
        }
    }

    public class BarItemNhapKho
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public string Color { get; set; }
        public double MaxValue { get; set; }

        public double Ratio
        {
            get
            {
                if (MaxValue <= 0)
                    return 0;

                double ratio = (double)Value / MaxValue;

                if (Value > 0 && ratio < 0.12)
                    ratio = 0.12;

                return ratio;
            }
        }
    }

    public class TopSanPhamNhapItem
    {
        public int STT { get; set; }

        public string TenSanPham { get; set; }

        public int SoLuongNhap { get; set; }

        public decimal GiaTri { get; set; }

        public string GiaTriText =>
            GiaTri.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
    }
}