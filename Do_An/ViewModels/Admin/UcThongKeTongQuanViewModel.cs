using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Do_An.ViewModel
{
    public class UcThongKeTongQuanViewModel : INotifyPropertyChanged
    {
        private readonly Action _thoat;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private int _tongNhapKho;
        public int TongNhapKho
        {
            get => _tongNhapKho;
            set
            {
                _tongNhapKho = value;
                OnPropertyChanged();
                UpdateChartData();
            }
        }

        private int _tongXuatKho;
        public int TongXuatKho
        {
            get => _tongXuatKho;
            set
            {
                _tongXuatKho = value;
                OnPropertyChanged();
                UpdateChartData();
            }
        }

        private int _tongTonKho;
        public int TongTonKho
        {
            get => _tongTonKho;
            set
            {
                _tongTonKho = value;
                OnPropertyChanged();
                UpdateChartData();
            }
        }

        private List<BarItem> _chartData;
        public List<BarItem> ChartData
        {
            get => _chartData;
            set
            {
                _chartData = value;
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

        public UcThongKeTongQuanViewModel(Action thoat = null)
        {
            _thoat = thoat;
            ThoatCommand = new RelayCommand(_ => _thoat?.Invoke());

            LoadData();
        }

        private void LoadData()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                TongNhapKho = db.PHIEUNHAPs.Count();
                TongXuatKho = db.PHIEUXUATs.Count();
                TongTonKho = db.TONKHOes.Sum(x => (int?)x.SOLUONGTON) ?? 0;
            }
        }

        private void UpdateChartData()
        {
            double max = Math.Max(TongNhapKho, Math.Max(TongXuatKho, TongTonKho));

            if (max < 10)
                max = 10;

            MaxValue = max;

            ChartData = new List<BarItem>
            {
                new BarItem
                {
                    Label = "Nhập kho",
                    Value = TongNhapKho,
                    Color = "#FFB58A6A",
                    MaxValue = MaxValue
                },
                new BarItem
                {
                    Label = "Xuất kho",
                    Value = TongXuatKho,
                    Color = "#FFC49478",
                    MaxValue = MaxValue
                },
                new BarItem
                {
                    Label = "Tồn kho",
                    Value = TongTonKho,
                    Color = "#FF8B6251",
                    MaxValue = MaxValue
                }
            };
        }
    }

    public class BarItem : INotifyPropertyChanged
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
}