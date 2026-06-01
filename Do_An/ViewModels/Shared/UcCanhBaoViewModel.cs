using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Do_An.ViewModels.Shared
{
    public class UcCanhBaoViewModel : BaseViewModel
    {
        private readonly ObservableCollection<CanhBaoItem> _danhSachGoc
            = new ObservableCollection<CanhBaoItem>();

        public ObservableCollection<CanhBaoItem> DanhSachHienThi { get; }
            = new ObservableCollection<CanhBaoItem>();

        private string _filterMode = "all";

        public bool FilterTatCa => _filterMode == "all";
        public bool FilterChuaXem => _filterMode == "chua";
        public bool FilterDaXem => _filterMode == "da";

        private int _soLuongChuaXem;
        public int SoLuongChuaXem
        {
            get => _soLuongChuaXem;
            set
            {
                _soLuongChuaXem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BadgeText));
                OnPropertyChanged(nameof(BadgeMau));
                OnPropertyChanged(nameof(BadgeNen));
                OnPropertyChanged(nameof(BadgeVien));
            }
        }

        public string BadgeText => SoLuongChuaXem > 0
            ? $"⚠ {SoLuongChuaXem} chưa xem"
            : "✓ Tất cả đã xem";

        public string BadgeMau => SoLuongChuaXem > 0 ? "#FFE65100" : "#FF2E7D32";
        public string BadgeNen => SoLuongChuaXem > 0 ? "#FFFFF3E0" : "#FFE8F5E9";
        public string BadgeVien => SoLuongChuaXem > 0 ? "#FFFFCC80" : "#FFA5D6A7";

        public ICommand DanhDauCommand { get; }
        public ICommand FilterCommand { get; }

        public UcCanhBaoViewModel()
        {
            DanhDauCommand = new RelayCommand(p => DanhDauDaXem(p));
            FilterCommand = new RelayCommand(p => ApDungFilter(p?.ToString()));

            LamMoi();
        }

        private void LamMoi()
        {
            TaoCanhBao();
            LoadCanhBao();
        }

        private bool LaAdmin(QUANLI_KHOHANGEntities db)
        {
            string maTK = CurrentUser.MaTK?.Trim();

            return db.TAIKHOANs
                .Where(tk => tk.MATK == maTK)
                .SelectMany(tk => tk.VAITROes)
                .Any(vt => vt.TENVT == "Admin");
        }

        private IQueryable<TONKHO> LocTonKhoTheoTaiKhoan(QUANLI_KHOHANGEntities db)
        {
            string maTK = CurrentUser.MaTK?.Trim();

            if (LaAdmin(db))
                return db.TONKHOes;

            return db.TONKHOes.Where(tk =>
                db.PHANCONG_KHO.Any(pc =>
                    pc.MATK == maTK &&
                    pc.MAKHO == tk.MAKHO &&
                    pc.TRANGTHAI == true));
        }

        private IQueryable<CANHBAO> LocCanhBaoTheoTaiKhoan(QUANLI_KHOHANGEntities db)
        {
            string maTK = CurrentUser.MaTK?.Trim();

            if (LaAdmin(db))
                return db.CANHBAOs;

            return db.CANHBAOs.Where(cb =>
                db.PHANCONG_KHO.Any(pc =>
                    pc.MATK == maTK &&
                    pc.MAKHO == cb.MAKHO &&
                    pc.TRANGTHAI == true));
        }

        private void TaoCanhBao()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var dsTonThap = LocTonKhoTheoTaiKhoan(db)
                    .Where(tk => tk.SOLUONGTON <= tk.SANPHAM.MUCTONTOITHIEU)
                    .ToList();

                foreach (var ton in dsTonThap)
                {
                    bool daCoCanhBaoHomNay = db.CANHBAOs.Any(cb =>
                        cb.MAKHO == ton.MAKHO &&
                        cb.MASP == ton.MASP &&
                        cb.NGAYTAO.Year == DateTime.Now.Year &&
                        cb.NGAYTAO.Month == DateTime.Now.Month &&
                        cb.NGAYTAO.Day == DateTime.Now.Day);

                    if (daCoCanhBaoHomNay)
                        continue;

                    db.CANHBAOs.Add(new CANHBAO
                    {
                        MAKHO = ton.MAKHO,
                        MASP = ton.MASP,
                        NOIDUNG =
                            "Sản phẩm \"" + ton.SANPHAM.TENSP +
                            "\" tại kho \"" + ton.KHO.TENKHO +
                            "\" chỉ còn " + ton.SOLUONGTON +
                            " sản phẩm, dưới mức tối thiểu " +
                            ton.SANPHAM.MUCTONTOITHIEU + ".",
                        NGAYTAO = DateTime.Now,
                        DAXEM = false
                    });
                }

                db.SaveChanges();
            }
        }

        private void LoadCanhBao()
        {
            _danhSachGoc.Clear();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = LocCanhBaoTheoTaiKhoan(db)
                    .OrderByDescending(cb => cb.NGAYTAO)
                    .ToList()
                    .Select(cb => new CanhBaoItem
                    {
                        MACANHBAO = cb.MACANHBAO,
                        NOIDUNG = cb.NOIDUNG,
                        NGAYTAO = cb.NGAYTAO,
                        DAXEM = cb.DAXEM
                    })
                    .ToList();

                foreach (var item in ds)
                    _danhSachGoc.Add(item);
            }

            SoLuongChuaXem = _danhSachGoc.Count(x => !x.DAXEM);
            CapNhatHienThi();
        }

        private void DanhDauDaXem(object thamSo)
        {
            if (thamSo == null)
                return;

            int maCanhBao = Convert.ToInt32(thamSo);

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var cb = LocCanhBaoTheoTaiKhoan(db)
                    .FirstOrDefault(x => x.MACANHBAO == maCanhBao);

                if (cb == null)
                    return;

                cb.DAXEM = true;
                db.SaveChanges();
            }

            var item = _danhSachGoc.FirstOrDefault(x => x.MACANHBAO == maCanhBao);

            if (item != null)
                item.DAXEM = true;

            SoLuongChuaXem = _danhSachGoc.Count(x => !x.DAXEM);
            CapNhatHienThi();
        }

        private void ApDungFilter(string mode)
        {
            _filterMode = string.IsNullOrWhiteSpace(mode) ? "all" : mode;

            OnPropertyChanged(nameof(FilterTatCa));
            OnPropertyChanged(nameof(FilterChuaXem));
            OnPropertyChanged(nameof(FilterDaXem));

            CapNhatHienThi();
        }

        private void CapNhatHienThi()
        {
            DanhSachHienThi.Clear();

            var ds = _danhSachGoc.AsEnumerable();

            if (_filterMode == "chua")
                ds = ds.Where(x => !x.DAXEM);

            if (_filterMode == "da")
                ds = ds.Where(x => x.DAXEM);

            foreach (var item in ds)
                DanhSachHienThi.Add(item);
        }
    }

    public class CanhBaoItem : BaseViewModel
    {
        public int MACANHBAO { get; set; }
        public string NOIDUNG { get; set; }
        public DateTime NGAYTAO { get; set; }

        private bool _daXem;
        public bool DAXEM
        {
            get => _daXem;
            set
            {
                _daXem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NutHienThi));
                OnPropertyChanged(nameof(DaXemHienThi));
                OnPropertyChanged(nameof(ItemOpacity));
                OnPropertyChanged(nameof(IconNen));
                OnPropertyChanged(nameof(IconText));
                OnPropertyChanged(nameof(NoiDungMau));
            }
        }

        public string NgayTaoHT => NGAYTAO.ToString("dd/MM/yyyy HH:mm");

        public string NutHienThi => DAXEM ? "Collapsed" : "Visible";
        public string DaXemHienThi => DAXEM ? "Visible" : "Collapsed";

        public double ItemOpacity => DAXEM ? 0.45 : 1.0;

        public string IconNen => DAXEM ? "#FFF5F1EF" : "#FFFFF3E0";
        public string IconText => DAXEM ? "✓" : "⚠";
        public string NoiDungMau => DAXEM ? "#FF9A8C86" : "#FF4E342E";
    }
}