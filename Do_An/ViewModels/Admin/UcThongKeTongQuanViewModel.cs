using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Do_An.Model;

namespace Do_An.ViewModel
{
    public class UcThongKeTongQuanViewModel : INotifyPropertyChanged
    {
        // ==================== INotifyPropertyChanged ====================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ==================== Properties ====================

        private int _tongNhapKho;
        public int TongNhapKho
        {
            get => _tongNhapKho;
            set { _tongNhapKho = value; OnPropertyChanged(); UpdateChartData(); }
        }

        private int _tongXuatKho;
        public int TongXuatKho
        {
            get => _tongXuatKho;
            set { _tongXuatKho = value; OnPropertyChanged(); UpdateChartData(); }
        }

        private int _tongTonKho;
        public int TongTonKho
        {
            get => _tongTonKho;
            set { _tongTonKho = value; OnPropertyChanged(); UpdateChartData(); }
        }

        // ==================== Dữ liệu cho biểu đồ ====================

        private List<BarItem> _chartData;
        public List<BarItem> ChartData
        {
            get => _chartData;
            set { _chartData = value; OnPropertyChanged(); }
        }

        // Max để scale cột
        private double _maxValue = 1;
        public double MaxValue
        {
            get => _maxValue;
            set { _maxValue = value; OnPropertyChanged(); }
        }

        // ==================== Constructor ====================

        public UcThongKeTongQuanViewModel()
        {
            LoadData();
        }

        // ==================== Load dữ liệu mẫu ====================

        private void LoadData()
        {
            // TODO: thay bằng truy vấn database thực tế
            TongNhapKho = 320;
            TongXuatKho = 180;
            TongTonKho = 140;
        }

        // ==================== Cập nhật biểu đồ ====================

        private void UpdateChartData()
        {
            double max = System.Math.Max(TongNhapKho, System.Math.Max(TongXuatKho, TongTonKho));
            MaxValue = max < 1 ? 1 : max;

            ChartData = new List<BarItem>
            {
                new BarItem { Label = "Nhập kho",  Value = TongNhapKho, Color = "#FF6D8B74", MaxValue = MaxValue },
                new BarItem { Label = "Xuất kho",  Value = TongXuatKho, Color = "#FFB58A78", MaxValue = MaxValue },
                new BarItem { Label = "Tồn kho",   Value = TongTonKho,  Color = "#FF7D9BB5", MaxValue = MaxValue },
            };
        }
    }

    // ==================== Helper class cho từng cột ====================
    public class BarItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Label { get; set; }
        public int Value { get; set; }
        public string Color { get; set; }
        public double MaxValue { get; set; }

        /// <summary>Chiều cao cột tương đối (0.0 – 1.0) để dùng với Grid RowDefinitions *</summary>
        public double Ratio => MaxValue > 0 ? (double)Value / MaxValue : 0;
    }
}
