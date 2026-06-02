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
    public class UcThongKeTonKhoViewModel : INotifyPropertyChanged
    {
        private readonly Action _thoat;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private int _tongMatHang;
        public int TongMatHang
        {
            get => _tongMatHang;
            set { _tongMatHang = value; OnPropertyChanged(); }
        }

        private int _tongSoLuongTon;
        public int TongSoLuongTon
        {
            get => _tongSoLuongTon;
            set { _tongSoLuongTon = value; OnPropertyChanged(); }
        }

        private int _tongCanhBao;
        public int TongCanhBao
        {
            get => _tongCanhBao;
            set { _tongCanhBao = value; OnPropertyChanged(); }
        }

        private List<BarItemTonKho> _chartData;
        public List<BarItemTonKho> ChartData
        {
            get => _chartData;
            set { _chartData = value; OnPropertyChanged(); }
        }

        private ObservableCollection<TopTonKhoItem> _danhSachTopTonKho;
        public ObservableCollection<TopTonKhoItem> DanhSachTopTonKho
        {
            get => _danhSachTopTonKho;
            set { _danhSachTopTonKho = value; OnPropertyChanged(); }
        }

        private double _maxValue = 1;
        public double MaxValue
        {
            get => _maxValue;
            set { _maxValue = value; OnPropertyChanged(); }
        }

        public ICommand ThoatCommand { get; }

        public UcThongKeTonKhoViewModel(Action thoat = null)
        {
            _thoat = thoat;
            ThoatCommand = new RelayCommand(_ => _thoat?.Invoke());

            ChartData = new List<BarItemTonKho>();
            DanhSachTopTonKho = new ObservableCollection<TopTonKhoItem>();

            LoadData();
        }

        private void LoadData()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var dsTonKho = db.TONKHOes.ToList();

                TongMatHang = dsTonKho
                    .Select(x => x.MASP)
                    .Distinct()
                    .Count();

                TongSoLuongTon = dsTonKho.Sum(x => x.SOLUONGTON);

                TongCanhBao = dsTonKho.Count(x =>
                    x.SOLUONGTON <= (x.SANPHAM?.MUCTONTOITHIEU ?? 0));

                LoadBieuDoTheoKho(dsTonKho);
                LoadTopTonKho(dsTonKho);
            }
        }

        private void LoadBieuDoTheoKho(List<TONKHO> dsTonKho)
        {
            var ds = dsTonKho
                .GroupBy(tk => new
                {
                    tk.MAKHO,
                    TenKho = tk.KHO != null ? tk.KHO.TENKHO : tk.MAKHO
                })
                .Select(g => new
                {
                    TenKho = g.Key.TenKho,
                    SoLuong = g.Sum(x => x.SOLUONGTON)
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToList();

            double max = ds.Count == 0 ? 1 : ds.Max(x => x.SoLuong);

            if (max < 10)
                max = 10;

            max = max * 1.35;

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
                .Select((x, index) => new BarItemTonKho
                {
                    Label = x.TenKho,
                    Value = x.SoLuong,
                    Color = mauCot[index % mauCot.Length],
                    MaxValue = MaxValue
                })
                .ToList();
        }

        private void LoadTopTonKho(List<TONKHO> dsTonKho)
        {
            var ds = dsTonKho
                .Select(tk => new TopTonKhoItem
                {
                    TenSanPham = tk.SANPHAM != null ? tk.SANPHAM.TENSP : tk.MASP,
                    TenKho = tk.KHO != null ? tk.KHO.TENKHO : tk.MAKHO,
                    SoLuongTon = tk.SOLUONGTON,
                    MucToiThieu = tk.SANPHAM?.MUCTONTOITHIEU ?? 0
                })
                .OrderByDescending(x => x.SoLuongTon)
                .Take(8)
                .ToList();

            for (int i = 0; i < ds.Count; i++)
            {
                ds[i].STT = i + 1;
            }

            DanhSachTopTonKho = new ObservableCollection<TopTonKhoItem>(ds);
        }
    }

    public class BarItemTonKho
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

    public class TopTonKhoItem
    {
        public int STT { get; set; }
        public string TenSanPham { get; set; }
        public string TenKho { get; set; }
        public int SoLuongTon { get; set; }
        public int MucToiThieu { get; set; }

        public string TrangThai
        {
            get
            {
                if (SoLuongTon <= 0)
                    return "Hết hàng";

                if (SoLuongTon <= MucToiThieu)
                    return "Sắp hết";

                return "Ổn định";
            }
        }
    }
}