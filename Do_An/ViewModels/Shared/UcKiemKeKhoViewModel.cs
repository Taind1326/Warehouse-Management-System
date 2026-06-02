using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Shared
{
    public class UcKiemKeKhoViewModel : BaseViewModel
    {
        private readonly Action _moForm;
        private readonly Action _quayLaiDanhSach;
        private readonly Action _veTrangChu;

        public ObservableCollection<KiemKeItem> DanhSachKiemKe { get; set; }
        public ObservableCollection<ChiTietKiemItem> DanhSachChiTietKiem { get; set; }

        public ObservableCollection<string> DanhSachKho { get; set; }
        public ObservableCollection<string> DanhSachNguoiKiem { get; set; }
        public ObservableCollection<string> DanhSachTrangThai { get; set; }

        private KiemKeItem _selectedItem;
        public KiemKeItem SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        private ChiTietKiemItem _selectedChiTietKiem;
        public ChiTietKiemItem SelectedChiTietKiem
        {
            get => _selectedChiTietKiem;
            set { _selectedChiTietKiem = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                TimKiemKiemKe();
            }
        }

        private string _khoDuocChon;
        public string KhoDuocChon
        {
            get => _khoDuocChon;
            set
            {
                _khoDuocChon = value;
                OnPropertyChanged();

                if (!IsEdit && !string.IsNullOrWhiteSpace(value))
                    LoadChiTietMacDinh();
            }
        }

        public bool IsEdit { get; set; }
        public string TieuDe => IsEdit ? "SỬA PHIẾU KIỂM KHO" : "THÊM PHIẾU KIỂM KHO";

        public string MaKiemKe { get; set; }
        public DateTime NgayKiemKe { get; set; }
        public string NguoiKiemDuocChon { get; set; }
        public string TrangThaiDuocChon { get; set; }
        public string GhiChu { get; set; }

        public decimal TongLech
        {
            get
            {
                if (DanhSachChiTietKiem == null)
                    return 0;

                return DanhSachChiTietKiem.Sum(x => Math.Abs(x.ChenhLech));
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ThoatCommand { get; }

        public ICommand HuyCommand { get; }
        public ICommand LuuTamCommand { get; }
        public ICommand LuuCommand { get; }

        public UcKiemKeKhoViewModel(Action moForm, Action quayLaiDanhSach, Action veTrangChu)
        {
            _moForm = moForm;
            _quayLaiDanhSach = quayLaiDanhSach;
            _veTrangChu = veTrangChu;

            DanhSachKiemKe = new ObservableCollection<KiemKeItem>();
            DanhSachChiTietKiem = new ObservableCollection<ChiTietKiemItem>();

            AddCommand = new RelayCommand(_ => MoThem());
            EditCommand = new RelayCommand(_ => MoSua());
            DeleteCommand = new RelayCommand(_ => XoaPhieuKiem());
            ThoatCommand = new RelayCommand(_ => VeTrangChu());

            HuyCommand = new RelayCommand(_ => QuayLaiDanhSach());
            LuuTamCommand = new RelayCommand(_ => LuuPhieuKiem("Lưu tạm"));
            LuuCommand = new RelayCommand(_ => LuuPhieuKiem("Đã kiểm"));

            LoadComboBox();
            LoadKiemKe();
        }

        public void LoadKiemKe()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = LocKiemKeTheoTaiKhoan(db)
                    .ToList()
                    .Select((kk, index) => TaoKiemKeItem(kk, index))
                    .ToList();

                DanhSachKiemKe = new ObservableCollection<KiemKeItem>(ds);
                OnPropertyChanged(nameof(DanhSachKiemKe));
            }
        }

        private void LoadComboBox()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (LaAdmin(db))
                {
                    DanhSachKho = new ObservableCollection<string>(
                        db.KHOes.Select(x => x.TENKHO).ToList());
                }
                else
                {
                    DanhSachKho = new ObservableCollection<string>(
                        db.PHANCONG_KHO
                            .Where(pc => pc.MATK == CurrentUser.MaTK && pc.TRANGTHAI == true)
                            .Select(pc => pc.KHO.TENKHO)
                            .ToList());
                }

                DanhSachNguoiKiem = new ObservableCollection<string>
                {
                    CurrentUser.TenTK
                };

                NguoiKiemDuocChon = CurrentUser.TenTK;
            }

            DanhSachTrangThai = new ObservableCollection<string>
            {
                "Lưu tạm",
                "Đã kiểm",
                "Đã hủy"
            };

            OnPropertyChanged(nameof(DanhSachKho));
            OnPropertyChanged(nameof(DanhSachNguoiKiem));
            OnPropertyChanged(nameof(DanhSachTrangThai));
        }

        private KiemKeItem TaoKiemKeItem(KIEMKEKHO kk, int index)
        {
            return new KiemKeItem
            {
                STT = index + 1,
                MaKiemKe = kk.MAKIEMKE,
                NgayKiem = kk.NGAYKIEMKE.ToString("dd/MM/yyyy"),
                MaKho = kk.MAKHO,
                NguoiKiem = kk.TAIKHOAN?.TENTK ?? "",
                TongLech = TinhTongLech(kk.MAKIEMKE).ToString("N0", CultureInfo.GetCultureInfo("vi-VN")),
                GhiChu = kk.GHICHU,
                TrangThai = kk.TRANGTHAI
            };
        }

        private decimal TinhTongLech(string maKiemKe)
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                return db.CT_KIEMKE
                    .Where(x => x.MAKIEMKE == maKiemKe)
                    .ToList()
                    .Sum(x => Math.Abs(x.CHENHLECH ?? 0));
            }
        }

        private void MoThem()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (!CoQuyenThemKiemKe(db))
                {
                    MessageBox.Show("Chỉ Admin và Nhân viên kho được thêm phiếu kiểm!");
                    return;
                }
            }

            IsEdit = false;
            XoaForm();
            LoadChiTietMacDinh();
            BaoThayDoiForm();
            _moForm?.Invoke();
        }

        private void MoSua()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu kiểm cần sửa!");
                return;
            }

            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (!CoQuyenThemKiemKe(db))
                {
                    MessageBox.Show("Kế toán chỉ được xem phiếu kiểm, không được sửa!");
                    return;
                }
            }

            if (SelectedItem.TrangThai != "Lưu tạm")
            {
                MessageBox.Show("Chỉ được sửa phiếu kiểm có trạng thái Lưu tạm!");
                return;
            }

            IsEdit = true;
            DoDuLieuLenForm(SelectedItem);
            LoadChiTietKiemKe(SelectedItem.MaKiemKe);
            BaoThayDoiForm();
            _moForm?.Invoke();
        }

        private void LoadChiTietKiemKe(string maKiemKe)
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.CT_KIEMKE
                    .Where(x => x.MAKIEMKE == maKiemKe)
                    .ToList()
                    .Select((ct, index) => new ChiTietKiemItem(CapNhatTongLech)
                    {
                        STT = index + 1,
                        MaSanPham = ct.MASP,
                        TenSanPham = ct.SANPHAM?.TENSP ?? "",
                        SoLuongHeThong = ct.SOLUONGHETHONG,
                        SoLuongThucTe = ct.SOLUONGTHUCTE
                    })
                    .ToList();

                DanhSachChiTietKiem = new ObservableCollection<ChiTietKiemItem>(ds);
            }

            OnPropertyChanged(nameof(DanhSachChiTietKiem));
            OnPropertyChanged(nameof(TongLech));
        }

        private void LoadChiTietMacDinh()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                string maKho = LayMaKho(db);

                var ds = db.TONKHOes
                    .Where(tk => tk.MAKHO == maKho)
                    .ToList()
                    .Select((tk, index) => new ChiTietKiemItem(CapNhatTongLech)
                    {
                        STT = index + 1,
                        MaSanPham = tk.MASP,
                        TenSanPham = tk.SANPHAM?.TENSP ?? "",
                        SoLuongHeThong = tk.SOLUONGTON,
                        SoLuongThucTe = tk.SOLUONGTON
                    })
                    .ToList();

                DanhSachChiTietKiem = new ObservableCollection<ChiTietKiemItem>(ds);
            }

            OnPropertyChanged(nameof(DanhSachChiTietKiem));
            OnPropertyChanged(nameof(TongLech));
        }

        private void LuuPhieuKiem(string trangThai)
        {
            NguoiKiemDuocChon = CurrentUser.TenTK;
            OnPropertyChanged(nameof(NguoiKiemDuocChon));

            if (!KiemTraDuLieu())
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    if (!CoQuyenThemKiemKe(db))
                    {
                        MessageBox.Show("Kế toán chỉ được xem phiếu kiểm, không được thêm/sửa!");
                        return;
                    }

                    if (IsEdit)
                        SuaPhieuKiem(db, trangThai);
                    else
                        ThemPhieuKiem(db, trangThai);

                    db.SaveChanges();
                }

                MessageBox.Show(trangThai == "Đã kiểm"
                    ? "Lưu phiếu kiểm thành công!"
                    : "Đã lưu tạm phiếu kiểm!");

                QuayLaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lưu phiếu kiểm thất bại!\n" + LayLoiChiTiet(ex));
            }
        }

        private void ThemPhieuKiem(QUANLI_KHOHANGEntities db, string trangThai)
        {
            if (db.KIEMKEKHOes.Any(x => x.MAKIEMKE == MaKiemKe))
                throw new Exception("Mã phiếu kiểm đã tồn tại!");

            string maKho = LayMaKho(db);

            if (!CoQuyenThaoTacKho(db, maKho))
                throw new Exception("Bạn không có quyền kiểm kê kho này!");

            var kk = new KIEMKEKHO
            {
                MAKIEMKE = MaKiemKe,
                MAKHO = maKho,
                MATK = CurrentUser.MaTK,
                NGAYKIEMKE = NgayKiemKe,
                TRANGTHAI = trangThai,
                GHICHU = GhiChu
            };

            db.KIEMKEKHOes.Add(kk);

            foreach (var item in DanhSachChiTietKiem)
            {
                db.CT_KIEMKE.Add(new CT_KIEMKE
                {
                    MAKIEMKE = MaKiemKe,
                    MASP = item.MaSanPham,
                    SOLUONGHETHONG = item.SoLuongHeThong,
                    SOLUONGTHUCTE = item.SoLuongThucTe
                });
            }

            if (trangThai == "Đã kiểm")
                CapNhatTonKhoSauKiemKe(db, maKho);

            GhiLog(db, "Tạo phiếu kiểm kê", MaKiemKe, "Tạo phiếu kiểm kê " + MaKiemKe);
        }

        private void SuaPhieuKiem(QUANLI_KHOHANGEntities db, string trangThai)
        {
            var kk = LocKiemKeTheoTaiKhoan(db)
                .FirstOrDefault(x => x.MAKIEMKE == MaKiemKe);

            if (kk == null)
                throw new Exception("Không tìm thấy phiếu kiểm hoặc bạn không có quyền sửa phiếu này!");

            if (kk.TRANGTHAI != "Lưu tạm")
                throw new Exception("Chỉ được sửa phiếu kiểm có trạng thái Lưu tạm!");

            string maKho = LayMaKho(db);

            if (!CoQuyenThaoTacKho(db, maKho))
                throw new Exception("Bạn không có quyền kiểm kê kho này!");

            kk.MAKHO = maKho;
            kk.MATK = CurrentUser.MaTK;
            kk.NGAYKIEMKE = NgayKiemKe;
            kk.TRANGTHAI = trangThai;
            kk.GHICHU = GhiChu;

            var dsCu = db.CT_KIEMKE
                .Where(x => x.MAKIEMKE == MaKiemKe)
                .ToList();

            foreach (var ct in dsCu)
                db.CT_KIEMKE.Remove(ct);

            foreach (var item in DanhSachChiTietKiem)
            {
                db.CT_KIEMKE.Add(new CT_KIEMKE
                {
                    MAKIEMKE = MaKiemKe,
                    MASP = item.MaSanPham,
                    SOLUONGHETHONG = item.SoLuongHeThong,
                    SOLUONGTHUCTE = item.SoLuongThucTe
                });
            }

            if (trangThai == "Đã kiểm")
                CapNhatTonKhoSauKiemKe(db, maKho);

            GhiLog(db, "Cập nhật kiểm kê", MaKiemKe, "Cập nhật kiểm kê " + MaKiemKe);
        }

        private void CapNhatTonKhoSauKiemKe(QUANLI_KHOHANGEntities db, string maKho)
        {
            foreach (var item in DanhSachChiTietKiem)
            {
                if (item.SoLuongThucTe < 0)
                    throw new Exception("Số lượng thực tế của sản phẩm " + item.TenSanPham + " không được âm!");

                var tonKho = db.TONKHOes.FirstOrDefault(x =>
                    x.MAKHO == maKho &&
                    x.MASP == item.MaSanPham);

                if (tonKho == null)
                {
                    tonKho = new TONKHO
                    {
                        MAKHO = maKho,
                        MASP = item.MaSanPham,
                        SOLUONGTON = 0
                    };

                    db.TONKHOes.Add(tonKho);
                }

                tonKho.SOLUONGTON = item.SoLuongThucTe;
            }
        }

        private void XoaPhieuKiem()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu kiểm cần xóa!");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa phiếu kiểm này không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var kk = LocKiemKeTheoTaiKhoan(db)
                        .FirstOrDefault(x => x.MAKIEMKE == SelectedItem.MaKiemKe);

                    if (kk == null)
                    {
                        MessageBox.Show("Không tìm thấy phiếu kiểm hoặc bạn không có quyền thao tác phiếu này!");
                        return;
                    }

                    bool laAdmin = LaAdmin(db);
                    bool laNhanVienKho = LaNhanVienKho(db);

                    if (!laAdmin && !laNhanVienKho)
                    {
                        MessageBox.Show("Kế toán chỉ được xem phiếu kiểm, không được xóa/hủy!");
                        return;
                    }

                    if (kk.TRANGTHAI == "Đã hủy")
                    {
                        MessageBox.Show("Phiếu này đã bị hủy trước đó!");
                        return;
                    }

                    if (kk.TRANGTHAI == "Đã kiểm")
                    {
                        if (!laAdmin)
                        {
                            MessageBox.Show("Chỉ Admin mới được hủy phiếu đã kiểm!");
                            return;
                        }

                        kk.TRANGTHAI = "Đã hủy";

                        GhiLog(db, "Hủy phiếu kiểm kê", kk.MAKIEMKE,
                            "Admin chuyển phiếu kiểm kê sang trạng thái Đã hủy");

                        db.SaveChanges();

                        MessageBox.Show("Đã chuyển phiếu kiểm sang trạng thái Đã hủy!");
                        LoadKiemKe();
                        return;
                    }

                    if (kk.TRANGTHAI == "Lưu tạm")
                    {
                        var chiTiet = db.CT_KIEMKE
                            .Where(x => x.MAKIEMKE == kk.MAKIEMKE)
                            .ToList();

                        foreach (var item in chiTiet)
                            db.CT_KIEMKE.Remove(item);

                        db.KIEMKEKHOes.Remove(kk);

                        GhiLog(db, "Hủy phiếu kiểm kê", kk.MAKIEMKE,
                            "Xóa phiếu kiểm kê lưu tạm");

                        db.SaveChanges();

                        MessageBox.Show("Xóa phiếu kiểm lưu tạm thành công!");
                        LoadKiemKe();
                        return;
                    }

                    MessageBox.Show("Trạng thái phiếu không hợp lệ!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xóa phiếu kiểm thất bại!\n" + LayLoiChiTiet(ex));
            }
        }

        private bool KiemTraDuLieu()
        {
            MaKiemKe = MaKiemKe?.Trim();

            if (string.IsNullOrWhiteSpace(MaKiemKe))
            {
                MessageBox.Show("Mã phiếu kiểm không được để trống!");
                return false;
            }

            if (!Regex.IsMatch(MaKiemKe, @"^KK\d{4}$"))
            {
                MessageBox.Show("Mã phiếu kiểm phải có dạng KK0001!");
                return false;
            }

            if (NgayKiemKe.Date > DateTime.Now.Date)
            {
                MessageBox.Show("Ngày kiểm kê không được lớn hơn ngày hiện tại!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(KhoDuocChon))
            {
                MessageBox.Show("Vui lòng chọn kho!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NguoiKiemDuocChon))
            {
                MessageBox.Show("Vui lòng chọn người kiểm!");
                return false;
            }

            if (DanhSachChiTietKiem == null || DanhSachChiTietKiem.Count == 0)
            {
                MessageBox.Show("Danh sách chi tiết kiểm kho đang trống!");
                return false;
            }

            foreach (var item in DanhSachChiTietKiem)
            {
                if (item.SoLuongThucTe < 0)
                {
                    MessageBox.Show("Số lượng thực tế của sản phẩm " + item.TenSanPham + " không được âm!");
                    return false;
                }
            }

            return true;
        }

        private void TimKiemKiemKe()
        {
            string tuKhoa = SearchText?.Trim().ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = LocKiemKeTheoTaiKhoan(db)
                    .ToList()
                    .Where(kk =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        kk.MAKIEMKE.ToLower().Contains(tuKhoa) ||
                        kk.MAKHO.ToLower().Contains(tuKhoa) ||
                        kk.MATK.ToLower().Contains(tuKhoa) ||
                        kk.TRANGTHAI.ToLower().Contains(tuKhoa) ||
                        (kk.TAIKHOAN != null &&
                         kk.TAIKHOAN.TENTK.ToLower().Contains(tuKhoa)))
                    .Select((kk, index) => TaoKiemKeItem(kk, index))
                    .ToList();

                DanhSachKiemKe = new ObservableCollection<KiemKeItem>(ds);
                OnPropertyChanged(nameof(DanhSachKiemKe));
            }
        }

        private IQueryable<KIEMKEKHO> LocKiemKeTheoTaiKhoan(QUANLI_KHOHANGEntities db)
        {
            if (LaAdmin(db))
                return db.KIEMKEKHOes;

            return db.KIEMKEKHOes.Where(kk =>
                db.PHANCONG_KHO.Any(pc =>
                    pc.MATK == CurrentUser.MaTK &&
                    pc.MAKHO == kk.MAKHO &&
                    pc.TRANGTHAI == true));
        }

        private bool CoQuyenThaoTacKho(QUANLI_KHOHANGEntities db, string maKho)
        {
            if (LaAdmin(db))
                return true;

            return db.PHANCONG_KHO.Any(pc =>
                pc.MATK == CurrentUser.MaTK &&
                pc.MAKHO == maKho &&
                pc.TRANGTHAI == true);
        }

        private bool CoQuyenThemKiemKe(QUANLI_KHOHANGEntities db)
        {
            return LaAdmin(db) || LaNhanVienKho(db);
        }

        private bool LaAdmin(QUANLI_KHOHANGEntities db)
        {
            var taiKhoan = db.TAIKHOANs.FirstOrDefault(x => x.MATK == CurrentUser.MaTK);

            return taiKhoan != null &&
                   taiKhoan.VAITROes.Any(vt => vt.TENVT == "Admin");
        }

        private bool LaNhanVienKho(QUANLI_KHOHANGEntities db)
        {
            var taiKhoan = db.TAIKHOANs.FirstOrDefault(x => x.MATK == CurrentUser.MaTK);

            return taiKhoan != null &&
                   taiKhoan.VAITROes.Any(vt => vt.TENVT == "NhanVienKho");
        }

        private void VeTrangChu()
        {
            _veTrangChu?.Invoke();
        }

        private string LayMaKho(QUANLI_KHOHANGEntities db)
        {
            return db.KHOes.FirstOrDefault(x => x.TENKHO == KhoDuocChon || x.MAKHO == KhoDuocChon)?.MAKHO;
        }

        private string LayTenKho(QUANLI_KHOHANGEntities db, string maKho)
        {
            return db.KHOes.FirstOrDefault(x => x.MAKHO == maKho)?.TENKHO;
        }

        private string LayTenTaiKhoan(QUANLI_KHOHANGEntities db, string maTK)
        {
            return db.TAIKHOANs.FirstOrDefault(x => x.MATK == maTK)?.TENTK;
        }

        private void DoDuLieuLenForm(KiemKeItem item)
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var kk = LocKiemKeTheoTaiKhoan(db)
                    .FirstOrDefault(x => x.MAKIEMKE == item.MaKiemKe);

                if (kk == null)
                    return;

                MaKiemKe = kk.MAKIEMKE;
                NgayKiemKe = kk.NGAYKIEMKE;
                KhoDuocChon = LayTenKho(db, kk.MAKHO);
                NguoiKiemDuocChon = kk.TAIKHOAN?.TENTK ?? CurrentUser.TenTK;
                TrangThaiDuocChon = kk.TRANGTHAI;
                GhiChu = kk.GHICHU;
            }
        }

        private void XoaForm()
        {
            LoadComboBox();

            MaKiemKe = "";
            NgayKiemKe = DateTime.Now;
            KhoDuocChon = DanhSachKho?.FirstOrDefault();
            NguoiKiemDuocChon = CurrentUser.TenTK;
            TrangThaiDuocChon = "Lưu tạm";
            GhiChu = "";

            DanhSachChiTietKiem = new ObservableCollection<ChiTietKiemItem>();

            BaoThayDoiForm();
        }

        private void GhiLog(QUANLI_KHOHANGEntities db, string hanhDong, string doiTuong, string ghiChu)
        {
            db.NHATKies.Add(new NHATKY
            {
                MATK = CurrentUser.MaTK,
                HANHDONG = hanhDong,
                DOITUONG = doiTuong,
                THOIGIAN = DateTime.Now,
                TRANGTHAI = "Thành công",
                GHICHU = ghiChu
            });
        }

        private void QuayLaiDanhSach()
        {
            LoadKiemKe();
            _quayLaiDanhSach?.Invoke();
        }

        private string LayLoiChiTiet(Exception ex)
        {
            string loi = ex.Message;

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                loi += "\n" + ex.Message;
            }

            return loi;
        }

        private void CapNhatTongLech()
        {
            OnPropertyChanged(nameof(TongLech));
        }

        private void BaoThayDoiForm()
        {
            OnPropertyChanged(nameof(IsEdit));
            OnPropertyChanged(nameof(TieuDe));
            OnPropertyChanged(nameof(MaKiemKe));
            OnPropertyChanged(nameof(NgayKiemKe));
            OnPropertyChanged(nameof(KhoDuocChon));
            OnPropertyChanged(nameof(NguoiKiemDuocChon));
            OnPropertyChanged(nameof(TrangThaiDuocChon));
            OnPropertyChanged(nameof(GhiChu));
            OnPropertyChanged(nameof(DanhSachChiTietKiem));
            OnPropertyChanged(nameof(TongLech));
        }
    }

    public class KiemKeItem
    {
        public int STT { get; set; }
        public string MaKiemKe { get; set; }
        public string NgayKiem { get; set; }
        public string MaKho { get; set; }
        public string NguoiKiem { get; set; }
        public string TongLech { get; set; }
        public string GhiChu { get; set; }
        public string TrangThai { get; set; }
    }

    public class ChiTietKiemItem : BaseViewModel
    {
        private readonly Action _capNhatTongLech;

        public ChiTietKiemItem(Action capNhatTongLech)
        {
            _capNhatTongLech = capNhatTongLech;
        }

        public int STT { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuongHeThong { get; set; }

        private int _soLuongThucTe;
        public int SoLuongThucTe
        {
            get => _soLuongThucTe;
            set
            {
                _soLuongThucTe = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChenhLech));
                _capNhatTongLech?.Invoke();
            }
        }

        public int ChenhLech => SoLuongThucTe - SoLuongHeThong;
    }
}