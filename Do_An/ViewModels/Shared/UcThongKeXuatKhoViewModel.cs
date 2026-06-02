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
    public class UcThongKeXuatKhoViewModel : INotifyPropertyChanged
    {
        private readonly Action _thoat;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private int _tongPhieuXuat;
        public int TongPhieuXuat
        {
            get => _tongPhieuXuat;
            set
            {
                _tongPhieuXuat = value;
                OnPropertyChanged();
            }
        }

        private int _tongSoLuongXuat;
        public int TongSoLuongXuat
        {
            get => _tongSoLuongXuat;
            set
            {
                _tongSoLuongXuat = value;
                OnPropertyChanged();
            }
        }

        private decimal _tongGiaTriXuat;
        public decimal TongGiaTriXuat
        {
            get => _tongGiaTriXuat;
            set
            {
                _tongGiaTriXuat = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TongGiaTriXuatText));
            }
        }

        public string TongGiaTriXuatText =>
            TongGiaTriXuat.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";

        private List<BarItemXuatKho> _chartData;
        public List<BarItemXuatKho> ChartData
        {
            get => _chartData;
            set
            {
                _chartData = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TopSanPhamXuatItem> _danhSachTopSanPham;
        public ObservableCollection<TopSanPhamXuatItem> DanhSachTopSanPham
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

        public UcThongKeXuatKhoViewModel(Action thoat = null)
        {
            _thoat = thoat;
            ThoatCommand = new RelayCommand(_ => _thoat?.Invoke());

            ChartData = new List<BarItemXuatKho>();
            DanhSachTopSanPham = new ObservableCollection<TopSanPhamXuatItem>();

            LoadData();
        }

        private void LoadData()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var dsPhieuXuat = LocPhieuXuatTheoTaiKhoan(db)
                    .Where(px => px.TRANGTHAI != "Đã hủy")
                    .ToList();

                var dsMaPhieu = dsPhieuXuat
                    .Select(px => px.MAPX)
                    .ToList();

                var dsChiTiet = db.CT_PHIEUXUAT
                    .Where(ct => dsMaPhieu.Contains(ct.MAPX))
                    .ToList();

                TongPhieuXuat = dsPhieuXuat.Count;
                TongSoLuongXuat = dsChiTiet.Sum(ct => ct.SOLUONG);
                TongGiaTriXuat = dsChiTiet.Sum(ct => ct.SOLUONG * ct.DONGIA);

                LoadBieuDoTheoKho(dsPhieuXuat, dsChiTiet);
                LoadTopSanPham(dsChiTiet);
            }
        }

        private void LoadBieuDoTheoKho(
            List<PHIEUXUAT> dsPhieuXuat,
            List<CT_PHIEUXUAT> dsChiTiet)
        {
            var ds = dsPhieuXuat
                .GroupBy(px => new
                {
                    px.MAKHO,
                    TenKho = px.KHO?.TENKHO ?? px.MAKHO
                })
                .Select(g => new
                {
                    TenKho = g.Key.TenKho,
                    SoLuong = dsChiTiet
                        .Where(ct => g.Select(px => px.MAPX).Contains(ct.MAPX))
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
                .Select((x, index) => new BarItemXuatKho
                {
                    Label = x.TenKho,
                    Value = x.SoLuong,
                    Color = mauCot[index % mauCot.Length],
                    MaxValue = MaxValue
                })
                .ToList();
        }

        private void LoadTopSanPham(List<CT_PHIEUXUAT> dsChiTiet)
        {
            var ds = dsChiTiet
                .GroupBy(ct => new
                {
                    ct.MASP,
                    TenSP = ct.SANPHAM?.TENSP ?? ct.MASP
                })
                .Select(g => new TopSanPhamXuatItem
                {
                    TenSanPham = g.Key.TenSP,
                    SoLuongXuat = g.Sum(x => x.SOLUONG),
                    GiaTri = g.Sum(x => x.SOLUONG * x.DONGIA)
                })
                .OrderByDescending(x => x.SoLuongXuat)
                .Take(8)
                .ToList();

            for (int i = 0; i < ds.Count; i++)
            {
                ds[i].STT = i + 1;
            }

            DanhSachTopSanPham =
                new ObservableCollection<TopSanPhamXuatItem>(ds);
        }

        private IQueryable<PHIEUXUAT> LocPhieuXuatTheoTaiKhoan(QUANLI_KHOHANGEntities db)
        {
            if (LaAdmin(db))
                return db.PHIEUXUATs;

            string maTK = CurrentUser.MaTK?.Trim();

            return db.PHIEUXUATs.Where(px =>
                db.PHANCONG_KHO.Any(pc =>
                    pc.MATK == maTK &&
                    pc.MAKHO == px.MAKHO &&
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

    public class BarItemXuatKho : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

    public class TopSanPhamXuatItem
    {
        public int STT { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuongXuat { get; set; }
        public decimal GiaTri { get; set; }

        public string GiaTriText =>
            GiaTri.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
    }
}