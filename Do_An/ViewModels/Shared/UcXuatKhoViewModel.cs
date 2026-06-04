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
    public class UcXuatKhoViewModel : BaseViewModel
    {
        private readonly Action _moForm;
        private readonly Action _quayLaiDanhSach;
        private readonly Action _veTrangChu;

        public ObservableCollection<PhieuXuatItem> DanhSachPhieuXuat { get; set; }
        public ObservableCollection<ChiTietXuatItem> DanhSachChiTietXuat { get; set; }
        public ObservableCollection<SanPhamXuatItem> DanhSachSanPham { get; set; }

        public ObservableCollection<string> DanhSachNoiXuat { get; set; }
        public ObservableCollection<string> DanhSachNoiNhan { get; set; }
        public ObservableCollection<string> DanhSachNguoiLap { get; set; }
        public ObservableCollection<string> DanhSachTrangThai { get; set; }

        private PhieuXuatItem _selectedItem;
        public PhieuXuatItem SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        private ChiTietXuatItem _selectedChiTietXuat;
        public ChiTietXuatItem SelectedChiTietXuat
        {
            get => _selectedChiTietXuat;
            set { _selectedChiTietXuat = value; OnPropertyChanged(); }
        }

        private SanPhamXuatItem _sanPhamDuocChon;
        public SanPhamXuatItem SanPhamDuocChon
        {
            get => _sanPhamDuocChon;
            set { _sanPhamDuocChon = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                TimKiemPhieuXuat();
            }
        }

        private string _soLuongXuat = "1";
        public string SoLuongXuat
        {
            get => _soLuongXuat;
            set { _soLuongXuat = value; OnPropertyChanged(); }
        }

        public bool IsEdit { get; set; }
        public string TieuDe => IsEdit ? "SỬA PHIẾU XUẤT" : "THÊM PHIẾU XUẤT";

        public string MaPhieuXuat { get; set; }
        public DateTime NgayXuat { get; set; }
        public string NoiXuatDuocChon { get; set; }
        public string NoiNhanDuocChon { get; set; }
        public string NguoiLapDuocChon { get; set; }
        public string TrangThaiDuocChon { get; set; }

        public decimal TongTien
        {
            get
            {
                if (DanhSachChiTietXuat == null)
                    return 0;

                return DanhSachChiTietXuat.Sum(x => x.ThanhTien);
            }
        }

        public ICommand ThemMoiCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand ThoatCommand { get; }

        public ICommand ThemSanPhamCommand { get; }
        public ICommand XoaSanPhamCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand LuuTamCommand { get; }
        public ICommand QuayLaiCommand { get; }

        public UcXuatKhoViewModel(Action moForm, Action quayLaiDanhSach, Action veTrangChu)
        {
            _moForm = moForm;
            _quayLaiDanhSach = quayLaiDanhSach;
            _veTrangChu = veTrangChu;

            DanhSachPhieuXuat = new ObservableCollection<PhieuXuatItem>();
            DanhSachChiTietXuat = new ObservableCollection<ChiTietXuatItem>();

            ThemMoiCommand = new RelayCommand(_ => MoThem());
            SuaCommand = new RelayCommand(_ => MoSua());
            XoaCommand = new RelayCommand(_ => XoaPhieuXuat());
            ThoatCommand = new RelayCommand(_ => VeTrangChu());

            ThemSanPhamCommand = new RelayCommand(_ => ThemSanPham());
            XoaSanPhamCommand = new RelayCommand(_ => XoaSanPham());
            LuuCommand = new RelayCommand(_ => LuuPhieuXuat("Đã xuất"));
            LuuTamCommand = new RelayCommand(_ => LuuPhieuXuat("Lưu tạm"));
            QuayLaiCommand = new RelayCommand(_ => QuayLaiDanhSach());

            LoadComboBox();
            LoadPhieuXuat();
        }

        public void LoadPhieuXuat()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = LocPhieuXuatTheoTaiKhoan(db)
                    .ToList()
                    .Select((px, index) => TaoPhieuXuatItem(px, index))
                    .ToList();

                DanhSachPhieuXuat = new ObservableCollection<PhieuXuatItem>(ds);
                OnPropertyChanged(nameof(DanhSachPhieuXuat));
            }
        }

        private void LoadComboBox()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (LaAdmin(db))
                {
                    DanhSachNoiXuat = new ObservableCollection<string>(
                        db.KHOes.Select(x => x.TENKHO).ToList());
                }
                else
                {
                    DanhSachNoiXuat = new ObservableCollection<string>(
                        db.PHANCONG_KHO
                            .Where(pc => pc.MATK == CurrentUser.MaTK && pc.TRANGTHAI == true)
                            .Select(pc => pc.KHO.TENKHO)
                            .ToList());
                }

                DanhSachNoiNhan = new ObservableCollection<string>(
                    db.KHOes.Select(x => x.TENKHO).ToList());

                DanhSachNguoiLap = new ObservableCollection<string>
                {
                    CurrentUser.TenTK
                };

                NguoiLapDuocChon = CurrentUser.TenTK;

                DanhSachSanPham = new ObservableCollection<SanPhamXuatItem>(
                    db.SANPHAMs
                        .ToList()
                        .Select(sp => new SanPhamXuatItem
                        {
                            MaSP = sp.MASP,
                            TenSP = sp.TENSP,
                            TenLoai = sp.LOAIHANG?.TENLOAI ?? "",
                            DonViTinh = sp.DONVITINH?.TENDVT ?? "",
                            DonGia = sp.DONGIA
                        })
                        .ToList());
            }

            DanhSachTrangThai = new ObservableCollection<string>
            {
                "Lưu tạm",
                "Đã xuất",
                "Đã hủy"
            };

            BaoThayDoiComboBox();
        }

        private PhieuXuatItem TaoPhieuXuatItem(PHIEUXUAT px, int index)
        {
            return new PhieuXuatItem
            {
                STT = index + 1,
                MaPhieuXuat = px.MAPX,
                NgayXuat = px.NGAYXUAT.ToString("dd/MM/yyyy"),
                MaKho = px.MAKHO,
                NguoiLap = px.TAIKHOAN?.TENTK ?? "",
                NoiNhan = LayTenKhoNhanTheoMa(px.MAKHONHAN),
                TongTien = DinhDangTien(px.TONGTIEN),
                TrangThai = px.TRANGTHAI
            };
        }

        private void MoThem()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (!CoQuyenThemPhieuXuat(db))
                {
                    MessageBox.Show("Chỉ Admin và Nhân viên kho được thêm phiếu xuất!");
                    return;
                }
            }

            IsEdit = false;
            XoaForm();
            BaoThayDoiForm();
            _moForm?.Invoke();
        }

        private void MoSua()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu xuất cần sửa!");
                return;
            }

            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (!CoQuyenThemPhieuXuat(db))
                {
                    MessageBox.Show("Kế toán chỉ được xem phiếu xuất, không được sửa!");
                    return;
                }
            }

            if (SelectedItem.TrangThai != "Lưu tạm")
            {
                MessageBox.Show("Chỉ được sửa phiếu xuất có trạng thái Lưu tạm!");
                return;
            }

            IsEdit = true;
            DoDuLieuLenForm(SelectedItem);
            LoadChiTietPhieuXuat(SelectedItem.MaPhieuXuat);
            BaoThayDoiForm();

            _moForm?.Invoke();
        }

        private void XoaPhieuXuat()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu xuất cần xóa!");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa phiếu xuất này không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var px = LocPhieuXuatTheoTaiKhoan(db)
                        .FirstOrDefault(x => x.MAPX == SelectedItem.MaPhieuXuat);

                    if (px == null)
                    {
                        MessageBox.Show("Không tìm thấy phiếu xuất hoặc bạn không có quyền thao tác phiếu này!");
                        return;
                    }

                    bool laAdmin = LaAdmin(db);
                    bool laNhanVienKho = LaNhanVienKho(db);

                    if (!laAdmin && !laNhanVienKho)
                    {
                        MessageBox.Show("Kế toán chỉ được xem phiếu xuất, không được xóa/hủy!");
                        return;
                    }

                    if (px.TRANGTHAI == "Đã hủy")
                    {
                        MessageBox.Show("Phiếu này đã bị hủy trước đó!");
                        return;
                    }

                    if (px.TRANGTHAI == "Đã xuất")
                    {
                        if (!laAdmin)
                        {
                            MessageBox.Show("Chỉ Admin mới được hủy phiếu đã xuất!");
                            return;
                        }

                        CongTonKhoTheoChiTietCu(db, px.MAKHO, px.MAPX);
                        TruTonKhoNhanTheoChiTietCu(db, px.MAKHONHAN, px.MAPX);

                        px.TRANGTHAI = "Đã hủy";

                        GhiLog(db, "Hủy phiếu xuất", px.MAPX,
                            "Admin chuyển phiếu xuất sang trạng thái Đã hủy");

                        db.SaveChanges();

                        MessageBox.Show("Đã chuyển phiếu xuất sang trạng thái Đã hủy!");
                        LoadPhieuXuat();
                        return;
                    }

                    if (px.TRANGTHAI == "Lưu tạm")
                    {
                        var chiTiet = db.CT_PHIEUXUAT
                            .Where(x => x.MAPX == px.MAPX)
                            .ToList();

                        foreach (var item in chiTiet)
                            db.CT_PHIEUXUAT.Remove(item);

                        db.PHIEUXUATs.Remove(px);

                        GhiLog(db, "Hủy phiếu xuất", px.MAPX,
                            "Xóa phiếu xuất lưu tạm");

                        db.SaveChanges();

                        MessageBox.Show("Xóa phiếu xuất lưu tạm thành công!");
                        LoadPhieuXuat();
                        return;
                    }

                    MessageBox.Show("Phiếu này không thể xóa!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xóa phiếu xuất thất bại!\n" + LayLoiChiTiet(ex));
            }
        }

        private void LoadChiTietPhieuXuat(string maPX)
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.CT_PHIEUXUAT
                    .Where(x => x.MAPX == maPX)
                    .ToList()
                    .Select((ct, index) => new ChiTietXuatItem
                    {
                        STT = index + 1,
                        MaHang = ct.MASP,
                        TenHang = ct.SANPHAM?.TENSP ?? "",
                        NhomHang = ct.SANPHAM?.LOAIHANG?.TENLOAI ?? "",
                        SoLuong = ct.SOLUONG,
                        DonViTinh = ct.SANPHAM?.DONVITINH?.TENDVT ?? "",
                        DonGia = ct.DONGIA
                    })
                    .ToList();

                DanhSachChiTietXuat = new ObservableCollection<ChiTietXuatItem>(ds);
                OnPropertyChanged(nameof(DanhSachChiTietXuat));
                OnPropertyChanged(nameof(TongTien));
            }
        }

        private void ThemSanPham()
        {
            if (SanPhamDuocChon == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm!");
                return;
            }

            int soLuong;
            if (!int.TryParse(SoLuongXuat, out soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên lớn hơn 0!");
                return;
            }

            var tonTai = DanhSachChiTietXuat
                .FirstOrDefault(x => x.MaHang == SanPhamDuocChon.MaSP);

            if (tonTai != null)
            {
                tonTai.SoLuong += soLuong;
                DanhSachChiTietXuat = new ObservableCollection<ChiTietXuatItem>(DanhSachChiTietXuat);
            }
            else
            {
                DanhSachChiTietXuat.Add(new ChiTietXuatItem
                {
                    STT = DanhSachChiTietXuat.Count + 1,
                    MaHang = SanPhamDuocChon.MaSP,
                    TenHang = SanPhamDuocChon.TenSP,
                    NhomHang = SanPhamDuocChon.TenLoai,
                    SoLuong = soLuong,
                    DonViTinh = SanPhamDuocChon.DonViTinh,
                    DonGia = SanPhamDuocChon.DonGia
                });
            }

            CapNhatSTTChiTiet();
            SoLuongXuat = "1";

            OnPropertyChanged(nameof(SoLuongXuat));
            OnPropertyChanged(nameof(TongTien));
        }

        private void XoaSanPham()
        {
            if (SelectedChiTietXuat == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa khỏi phiếu!");
                return;
            }

            DanhSachChiTietXuat.Remove(SelectedChiTietXuat);
            CapNhatSTTChiTiet();

            OnPropertyChanged(nameof(DanhSachChiTietXuat));
            OnPropertyChanged(nameof(TongTien));
        }

        private void LuuPhieuXuat(string trangThai)
        {
            NguoiLapDuocChon = CurrentUser.TenTK;
            OnPropertyChanged(nameof(NguoiLapDuocChon));

            if (!KiemTraDuLieu())
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    if (!CoQuyenThemPhieuXuat(db))
                    {
                        MessageBox.Show("Kế toán chỉ được xem phiếu xuất, không được thêm/sửa!");
                        return;
                    }

                    if (IsEdit)
                        SuaPhieuXuat(db, trangThai);
                    else
                        ThemPhieuXuat(db, trangThai);

                    db.SaveChanges();
                }

                MessageBox.Show(trangThai == "Đã xuất"
                    ? "Lưu phiếu xuất thành công!"
                    : "Đã lưu tạm phiếu xuất!");

                QuayLaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lưu phiếu xuất thất bại!\n" + LayLoiChiTiet(ex));
            }
        }

        private void ThemPhieuXuat(QUANLI_KHOHANGEntities db, string trangThai)
        {
            if (db.PHIEUXUATs.Any(x => x.MAPX == MaPhieuXuat))
                throw new Exception("Mã phiếu xuất đã tồn tại!");

            string maKho = LayMaKho(db);
            string maKhoNhan = LayMaKhoNhan(db);

            if (!CoQuyenThaoTacKho(db, maKho))
                throw new Exception("Bạn không có quyền xuất hàng từ kho này!");

            if (trangThai == "Đã xuất")
                KiemTraTonKhoDuDeXuat(db, maKho);

            var px = new PHIEUXUAT
            {
                MAPX = MaPhieuXuat,
                MAKHO = maKho,
                MAKHONHAN = maKhoNhan,
                MATK = CurrentUser.MaTK,
                NGAYXUAT = NgayXuat,
                TONGTIEN = TongTien,
                TRANGTHAI = trangThai
            };

            db.PHIEUXUATs.Add(px);

            foreach (var item in DanhSachChiTietXuat)
            {
                db.CT_PHIEUXUAT.Add(new CT_PHIEUXUAT
                {
                    MAPX = MaPhieuXuat,
                    MASP = item.MaHang,
                    SOLUONG = item.SoLuong,
                    DONGIA = item.DonGia
                });
            }

            if (trangThai == "Đã xuất")
            {
                TruTonKhoTheoDanhSachMoi(db, maKho);
                CongTonKhoNhanTheoDanhSachMoi(db, maKhoNhan);
            }

            GhiLog(db, "Tạo phiếu xuất", MaPhieuXuat,
                "Tạo phiếu xuất " + MaPhieuXuat);
        }

        private void SuaPhieuXuat(QUANLI_KHOHANGEntities db, string trangThai)
        {
            var px = LocPhieuXuatTheoTaiKhoan(db)
                .FirstOrDefault(x => x.MAPX == MaPhieuXuat);

            if (px == null)
                throw new Exception("Không tìm thấy phiếu xuất hoặc bạn không có quyền sửa phiếu này!");

            if (px.TRANGTHAI != "Lưu tạm")
                throw new Exception("Chỉ được sửa phiếu xuất có trạng thái Lưu tạm!");

            string maKhoMoi = LayMaKho(db);
            string maKhoNhanMoi = LayMaKhoNhan(db);

            if (!CoQuyenThaoTacKho(db, maKhoMoi))
                throw new Exception("Bạn không có quyền xuất hàng từ kho này!");

            if (trangThai == "Đã xuất")
                KiemTraTonKhoDuDeXuat(db, maKhoMoi);

            px.MAKHO = maKhoMoi;
            px.MAKHONHAN = maKhoNhanMoi;
            px.NGAYXUAT = NgayXuat;
            px.TONGTIEN = TongTien;
            px.TRANGTHAI = trangThai;

            var dsCu = db.CT_PHIEUXUAT
                .Where(x => x.MAPX == MaPhieuXuat)
                .ToList();

            foreach (var ct in dsCu)
                db.CT_PHIEUXUAT.Remove(ct);

            foreach (var item in DanhSachChiTietXuat)
            {
                db.CT_PHIEUXUAT.Add(new CT_PHIEUXUAT
                {
                    MAPX = MaPhieuXuat,
                    MASP = item.MaHang,
                    SOLUONG = item.SoLuong,
                    DONGIA = item.DonGia
                });
            }

            if (trangThai == "Đã xuất")
            {
                TruTonKhoTheoDanhSachMoi(db, maKhoMoi);
                CongTonKhoNhanTheoDanhSachMoi(db, maKhoNhanMoi);
            }

            GhiLog(db, "Sửa phiếu xuất", MaPhieuXuat,
                "Sửa phiếu xuất " + MaPhieuXuat);
        }

        private void KiemTraTonKhoDuDeXuat(QUANLI_KHOHANGEntities db, string maKho)
        {
            foreach (var item in DanhSachChiTietXuat)
            {
                var tonKho = db.TONKHOes.FirstOrDefault(x =>
                    x.MAKHO == maKho &&
                    x.MASP == item.MaHang);

                int soLuongTon = tonKho?.SOLUONGTON ?? 0;

                if (soLuongTon < item.SoLuong)
                {
                    throw new Exception(
                        "Sản phẩm " + item.TenHang +
                        " không đủ tồn kho. Tồn hiện tại: " + soLuongTon);
                }
            }
        }

        private void TruTonKhoTheoDanhSachMoi(QUANLI_KHOHANGEntities db, string maKho)
        {
            foreach (var item in DanhSachChiTietXuat)
            {
                var tonKho = db.TONKHOes.FirstOrDefault(x =>
                    x.MAKHO == maKho &&
                    x.MASP == item.MaHang);

                if (tonKho == null)
                    throw new Exception("Không tìm thấy tồn kho của sản phẩm " + item.TenHang);

                if (tonKho.SOLUONGTON < item.SoLuong)
                    throw new Exception("Sản phẩm " + item.TenHang + " không đủ tồn kho!");

                tonKho.SOLUONGTON -= item.SoLuong;
            }
        }

        private void CongTonKhoTheoChiTietCu(QUANLI_KHOHANGEntities db, string maKho, string maPX)
        {
            var dsCu = db.CT_PHIEUXUAT
                .Where(x => x.MAPX == maPX)
                .ToList();

            foreach (var item in dsCu)
            {
                var tonKho = db.TONKHOes.FirstOrDefault(x =>
                    x.MAKHO == maKho &&
                    x.MASP == item.MASP);

                if (tonKho == null)
                {
                    tonKho = new TONKHO
                    {
                        MAKHO = maKho,
                        MASP = item.MASP,
                        SOLUONGTON = 0
                    };

                    db.TONKHOes.Add(tonKho);
                }

                tonKho.SOLUONGTON += item.SOLUONG;
            }
        }

        private void CongTonKhoNhanTheoDanhSachMoi(QUANLI_KHOHANGEntities db, string maKhoNhan)
        {
            foreach (var item in DanhSachChiTietXuat)
            {
                var tonKho = db.TONKHOes.FirstOrDefault(x =>
                    x.MAKHO == maKhoNhan &&
                    x.MASP == item.MaHang);

                if (tonKho == null)
                {
                    tonKho = new TONKHO
                    {
                        MAKHO = maKhoNhan,
                        MASP = item.MaHang,
                        SOLUONGTON = 0
                    };

                    db.TONKHOes.Add(tonKho);
                }

                tonKho.SOLUONGTON += item.SoLuong;
            }
        }

        private void TruTonKhoNhanTheoChiTietCu(QUANLI_KHOHANGEntities db, string maKhoNhan, string maPX)
        {
            var dsCu = db.CT_PHIEUXUAT
                .Where(x => x.MAPX == maPX)
                .ToList();

            foreach (var item in dsCu)
            {
                var tonKho = db.TONKHOes.FirstOrDefault(x =>
                    x.MAKHO == maKhoNhan &&
                    x.MASP == item.MASP);

                if (tonKho == null)
                    throw new Exception("Không tìm thấy tồn kho nhận của sản phẩm " + item.MASP);

                if (tonKho.SOLUONGTON < item.SOLUONG)
                    throw new Exception("Tồn kho nhận không đủ để hủy/sửa phiếu xuất " + maPX);

                tonKho.SOLUONGTON -= item.SOLUONG;
            }
        }

        private bool KiemTraDuLieu()
        {
            MaPhieuXuat = MaPhieuXuat?.Trim();

            if (string.IsNullOrWhiteSpace(MaPhieuXuat))
            {
                MessageBox.Show("Mã phiếu xuất không được để trống!");
                return false;
            }

            if (!Regex.IsMatch(MaPhieuXuat, @"^PX\d{4}$"))
            {
                MessageBox.Show("Mã phiếu xuất phải có dạng PX0001!");
                return false;
            }

            if (NgayXuat.Date < DateTime.Today)
            {
                MessageBox.Show("Ngày xuất không được nhỏ hơn ngày hiện tại!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NoiXuatDuocChon))
            {
                MessageBox.Show("Vui lòng chọn nơi xuất!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NoiNhanDuocChon))
            {
                MessageBox.Show("Vui lòng chọn kho nhận!");
                return false;
            }

            if (NoiXuatDuocChon == NoiNhanDuocChon)
            {
                MessageBox.Show("Không được xuất vào chính kho hiện tại!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentUser.MaTK))
            {
                MessageBox.Show("Không xác định được tài khoản đang đăng nhập!");
                return false;
            }

            if (DanhSachChiTietXuat == null || DanhSachChiTietXuat.Count == 0)
            {
                MessageBox.Show("Phiếu xuất phải có ít nhất 1 sản phẩm!");
                return false;
            }

            return true;
        }

        private void TimKiemPhieuXuat()
        {
            string tuKhoa = SearchText?.Trim().ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = LocPhieuXuatTheoTaiKhoan(db)
                    .ToList()
                    .Where(px =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        px.MAPX.ToLower().Contains(tuKhoa) ||
                        px.MAKHO.ToLower().Contains(tuKhoa) ||
                        px.MAKHONHAN.ToLower().Contains(tuKhoa) ||
                        px.TRANGTHAI.ToLower().Contains(tuKhoa) ||
                        (px.TAIKHOAN != null &&
                         px.TAIKHOAN.TENTK.ToLower().Contains(tuKhoa)))
                    .Select((px, index) => TaoPhieuXuatItem(px, index))
                    .ToList();

                DanhSachPhieuXuat = new ObservableCollection<PhieuXuatItem>(ds);
                OnPropertyChanged(nameof(DanhSachPhieuXuat));
            }
        }

        private string LayTenKhoNhanTheoMa(string maKhoNhan)
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                return db.KHOes.FirstOrDefault(x => x.MAKHO == maKhoNhan)?.TENKHO ?? "";
            }
        }

        private string LayMaKho(QUANLI_KHOHANGEntities db)
        {
            return db.KHOes.FirstOrDefault(x => x.TENKHO == NoiXuatDuocChon)?.MAKHO;
        }

        private string LayTenKho(QUANLI_KHOHANGEntities db, string maKho)
        {
            return db.KHOes.FirstOrDefault(x => x.MAKHO == maKho)?.TENKHO;
        }

        private string LayMaKhoNhan(QUANLI_KHOHANGEntities db)
        {
            return db.KHOes.FirstOrDefault(x => x.TENKHO == NoiNhanDuocChon)?.MAKHO;
        }

        private string LayTenKhoNhan(QUANLI_KHOHANGEntities db, string maKhoNhan)
        {
            return db.KHOes.FirstOrDefault(x => x.MAKHO == maKhoNhan)?.TENKHO;
        }

        private void DoDuLieuLenForm(PhieuXuatItem item)
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var px = LocPhieuXuatTheoTaiKhoan(db)
                    .FirstOrDefault(x => x.MAPX == item.MaPhieuXuat);

                if (px == null)
                    return;

                MaPhieuXuat = px.MAPX;
                NgayXuat = px.NGAYXUAT;
                NoiXuatDuocChon = LayTenKho(db, px.MAKHO);
                NoiNhanDuocChon = LayTenKhoNhan(db, px.MAKHONHAN);
                NguoiLapDuocChon = px.TAIKHOAN?.TENTK ?? CurrentUser.TenTK;
                TrangThaiDuocChon = px.TRANGTHAI;
            }
        }

        private void XoaForm()
        {
            LoadComboBox();

            MaPhieuXuat = "";
            NgayXuat = DateTime.Now;
            NoiXuatDuocChon = DanhSachNoiXuat?.FirstOrDefault();
            NoiNhanDuocChon = DanhSachNoiNhan?.FirstOrDefault();
            NguoiLapDuocChon = CurrentUser.TenTK;
            TrangThaiDuocChon = "Lưu tạm";
            SoLuongXuat = "1";

            DanhSachChiTietXuat = new ObservableCollection<ChiTietXuatItem>();

            BaoThayDoiForm();
        }

        private void CapNhatSTTChiTiet()
        {
            for (int i = 0; i < DanhSachChiTietXuat.Count; i++)
                DanhSachChiTietXuat[i].STT = i + 1;

            DanhSachChiTietXuat = new ObservableCollection<ChiTietXuatItem>(DanhSachChiTietXuat);
            OnPropertyChanged(nameof(DanhSachChiTietXuat));
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

        private bool LaKeToan(QUANLI_KHOHANGEntities db)
        {
            var taiKhoan = db.TAIKHOANs.FirstOrDefault(x => x.MATK == CurrentUser.MaTK);

            return taiKhoan != null &&
                   taiKhoan.VAITROes.Any(vt => vt.TENVT == "KeToan");
        }

        private bool CoQuyenThemPhieuXuat(QUANLI_KHOHANGEntities db)
        {
            return LaAdmin(db) || LaNhanVienKho(db);
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

        private IQueryable<PHIEUXUAT> LocPhieuXuatTheoTaiKhoan(QUANLI_KHOHANGEntities db)
        {
            if (LaAdmin(db))
                return db.PHIEUXUATs;

            return db.PHIEUXUATs.Where(px =>
                db.PHANCONG_KHO.Any(pc =>
                    pc.MATK == CurrentUser.MaTK &&
                    pc.MAKHO == px.MAKHO &&
                    pc.TRANGTHAI == true));
        }

        private void VeTrangChu()
        {
            _veTrangChu?.Invoke();
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
            LoadPhieuXuat();
            _quayLaiDanhSach?.Invoke();
        }

        private string DinhDangTien(decimal soTien)
        {
            return soTien.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
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

        private void BaoThayDoiComboBox()
        {
            OnPropertyChanged(nameof(DanhSachNoiXuat));
            OnPropertyChanged(nameof(DanhSachNoiNhan));
            OnPropertyChanged(nameof(DanhSachNguoiLap));
            OnPropertyChanged(nameof(DanhSachTrangThai));
            OnPropertyChanged(nameof(DanhSachSanPham));
        }

        private void BaoThayDoiForm()
        {
            OnPropertyChanged(nameof(IsEdit));
            OnPropertyChanged(nameof(TieuDe));
            OnPropertyChanged(nameof(MaPhieuXuat));
            OnPropertyChanged(nameof(NgayXuat));
            OnPropertyChanged(nameof(NoiXuatDuocChon));
            OnPropertyChanged(nameof(NoiNhanDuocChon));
            OnPropertyChanged(nameof(NguoiLapDuocChon));
            OnPropertyChanged(nameof(TrangThaiDuocChon));
            OnPropertyChanged(nameof(DanhSachChiTietXuat));
            OnPropertyChanged(nameof(DanhSachSanPham));
            OnPropertyChanged(nameof(SanPhamDuocChon));
            OnPropertyChanged(nameof(SoLuongXuat));
            OnPropertyChanged(nameof(TongTien));
        }
    }

    public class PhieuXuatItem
    {
        public int STT { get; set; }
        public string MaPhieuXuat { get; set; }
        public string NgayXuat { get; set; }
        public string MaKho { get; set; }
        public string NguoiLap { get; set; }
        public string NoiNhan { get; set; }
        public string TongTien { get; set; }
        public string TrangThai { get; set; }
    }

    public class ChiTietXuatItem
    {
        public int STT { get; set; }
        public string MaHang { get; set; }
        public string TenHang { get; set; }
        public string NhomHang { get; set; }
        public int SoLuong { get; set; }
        public string DonViTinh { get; set; }
        public decimal DonGia { get; set; }

        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class SanPhamXuatItem
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public string TenLoai { get; set; }
        public string DonViTinh { get; set; }
        public decimal DonGia { get; set; }
    }
}