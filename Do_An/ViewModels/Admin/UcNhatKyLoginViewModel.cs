using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Admin
{
    public class UcNhatKyLoginViewModel : BaseViewModel
    {
        private ObservableCollection<NhatKyLoginItem> _danhSachNhatKy;
        public ObservableCollection<NhatKyLoginItem> DanhSachNhatKy
        {
            get => _danhSachNhatKy;
            set { _danhSachNhatKy = value; OnPropertyChanged(); }
        }

        private NhatKyLoginItem _nhatKyDangChon;
        public NhatKyLoginItem NhatKyDangChon
        {
            get => _nhatKyDangChon;
            set { _nhatKyDangChon = value; OnPropertyChanged(); }
        }

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
                "Đăng nhập",
                "Đăng xuất"
            };

        private string _hanhDongDuocChon = "Tất cả";
        public string HanhDongDuocChon
        {
            get => _hanhDongDuocChon;
            set
            {
                _hanhDongDuocChon = value;
                OnPropertyChanged();
            }
        }

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
                TimKiemNhanh();
            }
        }

        public ICommand LocCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CloseCommand { get; }

        private readonly Action _thoat;

        public UcNhatKyLoginViewModel(Action thoat = null)
        {
            _thoat = thoat;

            LocCommand = new RelayCommand(_ => LocNhatKy());
            RefreshCommand = new RelayCommand(_ => LamMoi());
            CloseCommand = new RelayCommand(_ => Thoat());

            LoadNhatKy();
        }

        private void LoadNhatKy()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.V_NHATKY_DANGNHAP
                    .ToList()
                    .OrderByDescending(x => x.THOIGIAN)
                    .Select((x, index) => TaoNhatKyItem(x, index))
                    .ToList();

                DanhSachNhatKy = new ObservableCollection<NhatKyLoginItem>(ds);
                TongSoBanGhi = ds.Count.ToString();
            }
        }

        private void LocNhatKy()
        {
            if (TuNgay != null && DenNgay != null && TuNgay > DenNgay)
            {
                MessageBox.Show("Từ ngày không được lớn hơn đến ngày!");
                return;
            }

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var query = db.V_NHATKY_DANGNHAP.AsQueryable();

                if (!string.IsNullOrWhiteSpace(HanhDongDuocChon)
                    && HanhDongDuocChon != "Tất cả")
                {
                    query = query.Where(x => x.HANHDONG == HanhDongDuocChon);
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

                string tuKhoa = TuKhoaTimKiem?.Trim().ToLower();

                var ds = query
                    .ToList()
                    .Where(x =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        (x.MATK != null && x.MATK.ToLower().Contains(tuKhoa)) ||
                        (x.TENTK != null && x.TENTK.ToLower().Contains(tuKhoa)) ||
                        (x.HANHDONG != null && x.HANHDONG.ToLower().Contains(tuKhoa)) ||
                        (x.DOITUONG != null && x.DOITUONG.ToLower().Contains(tuKhoa)) ||
                        (x.TRANGTHAI != null && x.TRANGTHAI.ToLower().Contains(tuKhoa)) ||
                        (x.GHICHU != null && x.GHICHU.ToLower().Contains(tuKhoa))
                    )
                    .OrderByDescending(x => x.THOIGIAN)
                    .Select((x, index) => TaoNhatKyItem(x, index))
                    .ToList();

                DanhSachNhatKy = new ObservableCollection<NhatKyLoginItem>(ds);
                TongSoBanGhi = ds.Count.ToString();
            }
        }

        private void TimKiemNhanh()
        {
            LocNhatKy();
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

        private NhatKyLoginItem TaoNhatKyItem(V_NHATKY_DANGNHAP nk, int index)
        {
            return new NhatKyLoginItem
            {
                STT = index + 1,
                MaTK = nk.MATK,
                HanhDong = nk.HANHDONG,
                DoiTuong = nk.DOITUONG,
                ThoiGian = nk.THOIGIAN.ToString("dd/MM/yyyy HH:mm:ss", new CultureInfo("vi-VN")),
                TrangThai = nk.TRANGTHAI,
                GhiChu = nk.GHICHU
            };
        }
    }

    public class NhatKyLoginItem
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
