using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Admin
{
    public class UcNhatKyThaoTacViewModel : BaseViewModel
    {
        public ObservableCollection<NhatKyItem> DanhSachNhatKy { get; set; }
            = new ObservableCollection<NhatKyItem>();

        public NhatKyItem NhatKyDangChon { get; set; }

        private string _tongSoBanGhi;
        public string TongSoBanGhi
        {
            get => _tongSoBanGhi;
            set { _tongSoBanGhi = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> DanhSachHanhDong { get; set; }
        = new ObservableCollection<string>
        {
            "Tất cả",
            "Thêm",
            "Sửa",
            "Xóa",
            "Khóa / Mở khóa",
            "Tạo phiếu",
            "Hủy phiếu",
            "Nhập / Xuất kho",
            "Kiểm kê"
        };

        public string HanhDongDuocChon { get; set; } = "Tất cả";

        private DateTime? _tuNgay;
        public DateTime? TuNgay
        {
            get => _tuNgay;
            set
            {
                _tuNgay = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _denNgay;
        public DateTime? DenNgay
        {
            get => _denNgay;
            set
            {
                _denNgay = value;
                OnPropertyChanged();
            }
        }

        private string _tuKhoaTimKiem;
        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set
            {
                _tuKhoaTimKiem = value;
                OnPropertyChanged();
                LocNhatKy();
            }
        }
        private readonly Action _thoat;
        public ICommand LocCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CloseCommand { get; }

        public UcNhatKyThaoTacViewModel(Action thoat = null)
        {
            _thoat = thoat;

            LocCommand = new RelayCommand(_ => LocNhatKy());
            RefreshCommand = new RelayCommand(_ => LamMoi());
            CloseCommand = new RelayCommand(_ => Thoat());

            LoadNhatKy();
        }

        // ================= LOAD =================
        private void LoadNhatKy()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.V_NHATKY_THAOTAC
                    .OrderByDescending(x => x.THOIGIAN)
                    .ToList()
                    .Select((x, index) => new NhatKyItem
                    {
                        STT = index + 1,
                        MaTK = x.MATK,
                        HanhDong = x.HANHDONG,
                        DoiTuong = x.DOITUONG,
                        ThoiGian = x.THOIGIAN.ToString("dd/MM/yyyy HH:mm:ss"),
                        TrangThai = x.TRANGTHAI,
                        GhiChu = x.GHICHU
                    })
                    .ToList();

                DanhSachNhatKy = new ObservableCollection<NhatKyItem>(ds);
                OnPropertyChanged(nameof(DanhSachNhatKy));

                TongSoBanGhi = ds.Count.ToString();
            }
        }

        // ================= FILTER =================
        private void LocNhatKy()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var query = db.V_NHATKY_THAOTAC.AsQueryable();

                if (TuNgay != null && DenNgay != null && TuNgay > DenNgay)
                {
                    MessageBox.Show("Từ ngày không được lớn hơn đến ngày!");
                    return;
                }

                if (TuNgay != null)
                {
                    DateTime tuNgay = TuNgay.Value.Date;
                    query = query.Where(x => x.THOIGIAN >= tuNgay);
                }

                if (DenNgay != null)
                {
                    DateTime denNgay = DenNgay.Value.Date.AddDays(1).AddSeconds(-1);
                    query = query.Where(x => x.THOIGIAN <= denNgay);
                }

                if (HanhDongDuocChon == "Thêm")
                {
                    query = query.Where(x => x.HANHDONG.StartsWith("Thêm"));
                }
                else if (HanhDongDuocChon == "Sửa")
                {
                    query = query.Where(x => x.HANHDONG.StartsWith("Sửa"));
                }
                else if (HanhDongDuocChon == "Xóa")
                {
                    query = query.Where(x => x.HANHDONG.StartsWith("Xóa"));
                }
                else if (HanhDongDuocChon == "Khóa / Mở khóa")
                {
                    query = query.Where(x =>
                        x.HANHDONG.Contains("Khóa nhân viên") ||
                        x.HANHDONG.Contains("Mở khóa nhân viên"));
                }
                else if (HanhDongDuocChon == "Tạo phiếu")
                {
                    query = query.Where(x => x.HANHDONG.Contains("Tạo phiếu"));
                }
                else if (HanhDongDuocChon == "Hủy phiếu")
                {
                    query = query.Where(x => x.HANHDONG.Contains("Hủy phiếu"));
                }
                else if (HanhDongDuocChon == "Nhập / Xuất kho")
                {
                    query = query.Where(x =>
                        x.HANHDONG.Contains("Nhập kho") ||
                        x.HANHDONG.Contains("Xuất kho"));
                }
                else if (HanhDongDuocChon == "Kiểm kê")
                {
                    query = query.Where(x => x.HANHDONG.Contains("kiểm kê"));
                }

                var tuKhoa = TuKhoaTimKiem?.ToLower();

                var ds = query
                    .ToList()
                    .Where(x =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        (x.MATK != null && x.MATK.ToLower().Contains(tuKhoa)) ||
                        (x.DOITUONG != null && x.DOITUONG.ToLower().Contains(tuKhoa)) ||
                        (x.GHICHU != null && x.GHICHU.ToLower().Contains(tuKhoa))
                    )
                    .OrderByDescending(x => x.THOIGIAN)
                    .Select((x, index) => new NhatKyItem
                    {
                        STT = index + 1,
                        MaTK = x.MATK,
                        HanhDong = x.HANHDONG,
                        DoiTuong = x.DOITUONG,
                        ThoiGian = x.THOIGIAN.ToString("dd/MM/yyyy HH:mm:ss"),
                        TrangThai = x.TRANGTHAI,
                        GhiChu = x.GHICHU
                    })
                    .ToList();

                DanhSachNhatKy = new ObservableCollection<NhatKyItem>(ds);
                OnPropertyChanged(nameof(DanhSachNhatKy));

                TongSoBanGhi = ds.Count.ToString();
            }
        }

        private void LamMoi()
        {
            HanhDongDuocChon = "Tất cả";
            TuNgay = null;
            DenNgay = null;
            TuKhoaTimKiem = "";

            OnPropertyChanged(nameof(HanhDongDuocChon));
            OnPropertyChanged(nameof(TuNgay));
            OnPropertyChanged(nameof(DenNgay));
            OnPropertyChanged(nameof(TuKhoaTimKiem));

            LoadNhatKy();
        }

        private void Thoat()
        {
            _thoat?.Invoke();
        }

        // ================= MODEL =================
        public class NhatKyItem
        {
            public int STT { get; set; }
            public string MaTK { get; set; }
            public string HanhDong { get; set; }
            public string DoiTuong { get; set; }
            public string ThoiGian { get; set; }
            public string TrangThai { get; set; }
            public string GhiChu { get; set; }
        }
    }
}