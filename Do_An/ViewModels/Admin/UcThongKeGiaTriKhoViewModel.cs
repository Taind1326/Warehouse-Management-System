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

namespace Do_An.ViewModel
{
    public class UcThongKeGiaTriKhoViewModel : INotifyPropertyChanged
    {
        private readonly Action _thoat;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private decimal _tongGiaTriKho;
        public decimal TongGiaTriKho
        {
            get => _tongGiaTriKho;
            set
            {
                _tongGiaTriKho = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TongGiaTriKhoText));
            }
        }

        public string TongGiaTriKhoText =>
            TongGiaTriKho.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";

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

        private List<BarItemGiaTriKho> _chartData;
        public List<BarItemGiaTriKho> ChartData
        {
            get => _chartData;
            set
            {
                _chartData = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TopGiaTriItem> _danhSachTopGiaTri;
        public ObservableCollection<TopGiaTriItem> DanhSachTopGiaTri
        {
            get => _danhSachTopGiaTri;
            set
            {
                _danhSachTopGiaTri = value;
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

        public UcThongKeGiaTriKhoViewModel(Action thoat = null)
        {
            _thoat = thoat;
            ThoatCommand = new RelayCommand(_ => _thoat?.Invoke());

            ChartData = new List<BarItemGiaTriKho>();
            DanhSachTopGiaTri = new ObservableCollection<TopGiaTriItem>();

            LoadData();
        }

        private void LoadData()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                TongGiaTriNhap = db.CT_PHIEUNHAP
                    .Sum(x => (decimal?)(x.SOLUONG * x.DONGIA)) ?? 0;

                TongGiaTriXuat = db.CT_PHIEUXUAT
                    .Sum(x => (decimal?)(x.SOLUONG * x.DONGIA)) ?? 0;

                TongGiaTriKho = db.TONKHOes
                    .Sum(x => (decimal?)(x.SOLUONGTON * x.SANPHAM.DONGIA)) ?? 0;

                LoadBieuDo(db);
                LoadTopGiaTri(db);
            }
        }

        private void LoadBieuDo(QUANLI_KHOHANGEntities db)
        {
            var ds = db.TONKHOes
                .ToList()
                .GroupBy(x => new
                {
                    x.MAKHO,
                    TenKho = x.KHO != null ? x.KHO.TENKHO : x.MAKHO
                })
                .Select(g => new
                {
                    TenKho = g.Key.TenKho,
                    GiaTri = g.Sum(x =>
                        x.SANPHAM != null
                            ? x.SOLUONGTON * x.SANPHAM.DONGIA
                            : 0)
                })
                .OrderByDescending(x => x.GiaTri)
                .Take(3)
                .ToList();

            double max = ds.Count == 0 ? 1 : (double)ds.Max(x => x.GiaTri);

            if (max < 10)
                max = 10;

            max = max * 1.35;

            MaxValue = max;

            string[] mau =
            {
                "#FFB58A6A",
                "#FFC49478",
                "#FF8B6251",
                "#FFD0A48A",
                "#FF9C6F5A"
            };

            ChartData = ds
                .Select((x, index) => new BarItemGiaTriKho
                {
                    Label = x.TenKho,
                    Value = (double)x.GiaTri,
                    Color = mau[index % mau.Length],
                    MaxValue = MaxValue
                })
                .ToList();
        }

        private void LoadTopGiaTri(QUANLI_KHOHANGEntities db)
        {
            var ds = db.TONKHOes
                .ToList()
                .Select(x => new TopGiaTriItem
                {
                    TenSanPham = x.SANPHAM != null ? x.SANPHAM.TENSP : x.MASP,
                    TenKho = x.KHO != null ? x.KHO.TENKHO : x.MAKHO,
                    GiaTri = x.SANPHAM != null
                        ? x.SOLUONGTON * x.SANPHAM.DONGIA
                        : 0
                })
                .OrderByDescending(x => x.GiaTri)
                .Take(8)
                .ToList();

            for (int i = 0; i < ds.Count; i++)
                ds[i].STT = i + 1;

            DanhSachTopGiaTri = new ObservableCollection<TopGiaTriItem>(ds);
        }
    }

    public class BarItemGiaTriKho
    {
        public string Label { get; set; }
        public double Value { get; set; }
        public string Color { get; set; }
        public double MaxValue { get; set; }

        public double Ratio
        {
            get
            {
                if (MaxValue <= 0)
                    return 0;

                double ratio = Value / MaxValue;

                if (Value > 0 && ratio < 0.12)
                    ratio = 0.12;

                return ratio;
            }
        }
    }

    public class TopGiaTriItem
    {
        public int STT { get; set; }
        public string TenSanPham { get; set; }
        public string TenKho { get; set; }
        public decimal GiaTri { get; set; }

        public string GiaTriText =>
            GiaTri.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
    }
}