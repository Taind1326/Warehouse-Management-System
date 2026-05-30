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
    public class UcLichSuNhapKhoViewModel : BaseViewModel
    {
        private readonly Action _thoat;

        // ================= DATA =================

        private ObservableCollection<LichSuNhapKhoItem> _danhSachLichSuNhapKho;
        public ObservableCollection<LichSuNhapKhoItem> DanhSachLichSuNhapKho
        {
            get => _danhSachLichSuNhapKho;
            set
            {
                _danhSachLichSuNhapKho = value;
                OnPropertyChanged();
            }
        }

        private LichSuNhapKhoItem _lichSuDangChon;
        public LichSuNhapKhoItem LichSuDangChon
        {
            get => _lichSuDangChon;
            set
            {
                _lichSuDangChon = value;
                OnPropertyChanged();
            }
        }

        private string _tongSoBanGhi;
        public string TongSoBanGhi
        {
            get => _tongSoBanGhi;
            set
            {
                _tongSoBanGhi = value;
                OnPropertyChanged();
            }
        }

        // ================= FILTER DATA =================

        public ObservableCollection<string> DanhSachTrangThai { get; set; }
            = new ObservableCollection<string>
            {
                "Tất cả",
                "Lưu tạm",
                "Đã nhập",
                "Đã hủy"
            };

        private string _trangThaiDuocChon = "Tất cả";
        public string TrangThaiDuocChon
        {
            get => _trangThaiDuocChon;
            set
            {
                _trangThaiDuocChon = value;
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
                LocLichSuNhapKho();
            }
        }

        // ================= COMMAND =================

        public ICommand LocCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CloseCommand { get; }

        // ================= CONSTRUCTOR =================

        public UcLichSuNhapKhoViewModel(Action thoat = null)
        {
            _thoat = thoat;

            LocCommand = new RelayCommand(_ => LocLichSuNhapKho());
            RefreshCommand = new RelayCommand(_ => LamMoi());
            CloseCommand = new RelayCommand(_ => Thoat());

            LoadLichSuNhapKho();
        }

        // ================= LOAD =================

        public void LoadLichSuNhapKho()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                string maKho = LayMaKhoDangPhanCong(db);

                if (string.IsNullOrWhiteSpace(maKho))
                {
                    GanDanhSachRong();
                    return;
                }

                var ds = db.PHIEUNHAPs
                    .Where(x => x.MAKHO == maKho)
                    .ToList()
                    .OrderByDescending(x => x.NGAYNHAP)
                    .Select((x, index) => TaoLichSuNhapKhoItem(x, index))
                    .ToList();

                CapNhatDanhSach(ds);
            }
        }

        // ================= FILTER =================

        private void LocLichSuNhapKho()
        {
            if (!KiemTraNgayHopLe())
                return;

            using (var db = new QUANLI_KHOHANGEntities())
            {
                string maKho = LayMaKhoDangPhanCong(db);

                if (string.IsNullOrWhiteSpace(maKho))
                {
                    GanDanhSachRong();
                    return;
                }

                var query = db.PHIEUNHAPs
                    .Where(x => x.MAKHO == maKho)
                    .AsQueryable();

                query = LocTheoTrangThai(query);
                query = LocTheoThoiGian(query);

                string tuKhoa = TuKhoaTimKiem?
                    .Trim()
                    .ToLower();

                var ds = query
                    .ToList()
                    .Where(x => KiemTraDungTuKhoa(x, tuKhoa))
                    .OrderByDescending(x => x.NGAYNHAP)
                    .Select((x, index) => TaoLichSuNhapKhoItem(x, index))
                    .ToList();

                CapNhatDanhSach(ds);
            }
        }

        private IQueryable<PHIEUNHAP> LocTheoTrangThai(IQueryable<PHIEUNHAP> query)
        {
            if (string.IsNullOrWhiteSpace(TrangThaiDuocChon)
                || TrangThaiDuocChon == "Tất cả")
            {
                return query;
            }

            return query.Where(x => x.TRANGTHAI == TrangThaiDuocChon);
        }

        private IQueryable<PHIEUNHAP> LocTheoThoiGian(IQueryable<PHIEUNHAP> query)
        {
            if (TuNgay != null)
            {
                DateTime tuNgay = TuNgay.Value.Date;
                query = query.Where(x => x.NGAYNHAP >= tuNgay);
            }

            if (DenNgay != null)
            {
                DateTime denNgay = DenNgay.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(x => x.NGAYNHAP <= denNgay);
            }

            return query;
        }

        private bool KiemTraDungTuKhoa(PHIEUNHAP phieu, string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
                return true;

            return
                (!string.IsNullOrWhiteSpace(phieu.MAPN) &&
                 phieu.MAPN.ToLower().Contains(tuKhoa)) ||

                (!string.IsNullOrWhiteSpace(phieu.MATK) &&
                 phieu.MATK.ToLower().Contains(tuKhoa)) ||

                (!string.IsNullOrWhiteSpace(phieu.TRANGTHAI) &&
                 phieu.TRANGTHAI.ToLower().Contains(tuKhoa));
        }

        // ================= ACTION =================

        private void LamMoi()
        {
            TrangThaiDuocChon = "Tất cả";
            TuNgay = null;
            DenNgay = null;
            TuKhoaTimKiem = "";

            OnPropertyChanged(nameof(TrangThaiDuocChon));
            OnPropertyChanged(nameof(TuNgay));
            OnPropertyChanged(nameof(DenNgay));
            OnPropertyChanged(nameof(TuKhoaTimKiem));

            LoadLichSuNhapKho();
        }

        private void Thoat()
        {
            _thoat?.Invoke();
        }

        // ================= HELPER =================

        private string LayMaKhoDangPhanCong(QUANLI_KHOHANGEntities db)
        {
            string maTK = CurrentUser.MaTK?.Trim();

            if (string.IsNullOrWhiteSpace(maTK))
                return null;

            return db.PHANCONG_KHO
                .Where(x =>
                    x.MATK == maTK &&
                    x.TRANGTHAI == true)
                .Select(x => x.MAKHO)
                .FirstOrDefault();
        }

        private bool KiemTraNgayHopLe()
        {
            if (TuNgay != null && DenNgay != null && TuNgay > DenNgay)
            {
                MessageBox.Show(
                    "Từ ngày không được lớn hơn đến ngày!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        private LichSuNhapKhoItem TaoLichSuNhapKhoItem(PHIEUNHAP phieu, int index)
        {
            return new LichSuNhapKhoItem
            {
                STT = index + 1,
                MaPhieu = phieu.MAPN,
                MaTK = phieu.MATK,
                NgayNhap = phieu.NGAYNHAP.ToString(
                    "dd/MM/yyyy HH:mm:ss",
                    new CultureInfo("vi-VN")),
                TrangThai = phieu.TRANGTHAI,
                TongSoLuong = TinhTongSoLuong(phieu).ToString(),
                GhiChu = ""
            };
        }

        private int TinhTongSoLuong(PHIEUNHAP phieu)
        {
            if (phieu.CT_PHIEUNHAP == null)
                return 0;

            return phieu.CT_PHIEUNHAP
                .Sum(x => x.SOLUONG);
        }

        private void CapNhatDanhSach(System.Collections.Generic.List<LichSuNhapKhoItem> ds)
        {
            DanhSachLichSuNhapKho = new ObservableCollection<LichSuNhapKhoItem>(ds);
            TongSoBanGhi = ds.Count.ToString();
        }

        private void GanDanhSachRong()
        {
            DanhSachLichSuNhapKho = new ObservableCollection<LichSuNhapKhoItem>();
            TongSoBanGhi = "0";
        }
    }

    public class LichSuNhapKhoItem
    {
        public int STT { get; set; }
        public string MaPhieu { get; set; }
        public string MaTK { get; set; }
        public string NgayNhap { get; set; }
        public string TrangThai { get; set; }
        public string TongSoLuong { get; set; }
        public string GhiChu { get; set; }
    }
}