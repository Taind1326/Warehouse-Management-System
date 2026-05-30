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
    public class UcLichSuXuatKhoViewModel : BaseViewModel
    {
        private readonly Action _thoat;

        // ================= DATA =================

        private ObservableCollection<LichSuXuatKhoItem> _danhSachLichSuXuatKho;
        public ObservableCollection<LichSuXuatKhoItem> DanhSachLichSuXuatKho
        {
            get => _danhSachLichSuXuatKho;
            set
            {
                _danhSachLichSuXuatKho = value;
                OnPropertyChanged();
            }
        }

        private LichSuXuatKhoItem _lichSuDangChon;
        public LichSuXuatKhoItem LichSuDangChon
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
                "Đã xuất",
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

                LocLichSuXuatKho();
            }
        }

        // ================= COMMAND =================

        public ICommand LocCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CloseCommand { get; }

        // ================= CONSTRUCTOR =================

        public UcLichSuXuatKhoViewModel(Action thoat = null)
        {
            _thoat = thoat;

            LocCommand = new RelayCommand(_ => LocLichSuXuatKho());
            RefreshCommand = new RelayCommand(_ => LamMoi());
            CloseCommand = new RelayCommand(_ => Thoat());

            LoadLichSuXuatKho();
        }

        // ================= LOAD =================

        public void LoadLichSuXuatKho()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                string maKho = LayMaKhoDangPhanCong(db);

                if (string.IsNullOrWhiteSpace(maKho))
                {
                    GanDanhSachRong();
                    return;
                }

                var ds = db.PHIEUXUATs
                    .Where(x => x.MAKHO == maKho)
                    .ToList()
                    .OrderByDescending(x => x.NGAYXUAT)
                    .Select((x, index) => TaoLichSuXuatKhoItem(x, index))
                    .ToList();

                CapNhatDanhSach(ds);
            }
        }

        // ================= FILTER =================

        private void LocLichSuXuatKho()
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

                var query = db.PHIEUXUATs
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
                    .OrderByDescending(x => x.NGAYXUAT)
                    .Select((x, index) => TaoLichSuXuatKhoItem(x, index))
                    .ToList();

                CapNhatDanhSach(ds);
            }
        }

        private IQueryable<PHIEUXUAT> LocTheoTrangThai(IQueryable<PHIEUXUAT> query)
        {
            if (string.IsNullOrWhiteSpace(TrangThaiDuocChon)
                || TrangThaiDuocChon == "Tất cả")
            {
                return query;
            }

            return query.Where(x => x.TRANGTHAI == TrangThaiDuocChon);
        }

        private IQueryable<PHIEUXUAT> LocTheoThoiGian(IQueryable<PHIEUXUAT> query)
        {
            if (TuNgay != null)
            {
                DateTime tuNgay = TuNgay.Value.Date;

                query = query.Where(x => x.NGAYXUAT >= tuNgay);
            }

            if (DenNgay != null)
            {
                DateTime denNgay = DenNgay.Value.Date
                    .AddDays(1)
                    .AddSeconds(-1);

                query = query.Where(x => x.NGAYXUAT <= denNgay);
            }

            return query;
        }

        private bool KiemTraDungTuKhoa(PHIEUXUAT phieu, string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
                return true;

            return
                (!string.IsNullOrWhiteSpace(phieu.MAPX) &&
                 phieu.MAPX.ToLower().Contains(tuKhoa))

                ||

                (!string.IsNullOrWhiteSpace(phieu.MATK) &&
                 phieu.MATK.ToLower().Contains(tuKhoa))

                ||

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

            LoadLichSuXuatKho();
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
            if (TuNgay != null &&
                DenNgay != null &&
                TuNgay > DenNgay)
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

        private LichSuXuatKhoItem TaoLichSuXuatKhoItem(
            PHIEUXUAT phieu,
            int index)
        {
            return new LichSuXuatKhoItem
            {
                STT = index + 1,

                MaPhieu = phieu.MAPX,

                MaTK = phieu.MATK,

                NgayXuat = phieu.NGAYXUAT.ToString(
                    "dd/MM/yyyy HH:mm:ss",
                    new CultureInfo("vi-VN")),

                TrangThai = phieu.TRANGTHAI,

                TongSoLuong = TinhTongSoLuong(phieu).ToString(),

                GhiChu = ""
            };
        }

        private int TinhTongSoLuong(PHIEUXUAT phieu)
        {
            if (phieu.CT_PHIEUXUAT == null)
                return 0;

            return phieu.CT_PHIEUXUAT
                .Sum(x => x.SOLUONG);
        }

        private void CapNhatDanhSach(
            System.Collections.Generic.List<LichSuXuatKhoItem> ds)
        {
            DanhSachLichSuXuatKho =
                new ObservableCollection<LichSuXuatKhoItem>(ds);

            TongSoBanGhi = ds.Count.ToString();
        }

        private void GanDanhSachRong()
        {
            DanhSachLichSuXuatKho =
                new ObservableCollection<LichSuXuatKhoItem>();

            TongSoBanGhi = "0";
        }
    }

    public class LichSuXuatKhoItem
    {
        public int STT { get; set; }

        public string MaPhieu { get; set; }

        public string MaTK { get; set; }

        public string NgayXuat { get; set; }

        public string TrangThai { get; set; }

        public string TongSoLuong { get; set; }

        public string GhiChu { get; set; }
    }
}