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
    public class UcNhapKhoViewModel : BaseViewModel
    {
        private readonly Action _moForm;
        private readonly Action _quayLaiDanhSach;
        private readonly Action _veTrangChu;

        private ObservableCollection<PhieuNhapItem> _danhSachPhieuNhap;
        public ObservableCollection<PhieuNhapItem> DanhSachPhieuNhap
        {
            get => _danhSachPhieuNhap;
            set { _danhSachPhieuNhap = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ChiTietNhapItem> _danhSachChiTietNhap;
        public ObservableCollection<ChiTietNhapItem> DanhSachChiTietNhap
        {
            get => _danhSachChiTietNhap;
            set
            {
                _danhSachChiTietNhap = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TongTien));
            }
        }

        private ObservableCollection<SanPhamNhapItem> _tatCaSanPham;
        public ObservableCollection<SanPhamNhapItem> TatCaSanPham
        {
            get => _tatCaSanPham;
            set { _tatCaSanPham = value; OnPropertyChanged(); }
        }

        private ObservableCollection<SanPhamNhapItem> _danhSachSanPham;
        public ObservableCollection<SanPhamNhapItem> DanhSachSanPham
        {
            get => _danhSachSanPham;
            set { _danhSachSanPham = value; OnPropertyChanged(); }
        }

        private PhieuNhapItem _selectedItem;
        public PhieuNhapItem SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        private ChiTietNhapItem _selectedChiTietNhap;
        public ChiTietNhapItem SelectedChiTietNhap
        {
            get => _selectedChiTietNhap;
            set { _selectedChiTietNhap = value; OnPropertyChanged(); }
        }

        private SanPhamNhapItem _sanPhamDuocChon;
        public SanPhamNhapItem SanPhamDuocChon
        {
            get => _sanPhamDuocChon;
            set { _sanPhamDuocChon = value; OnPropertyChanged(); }
        }

        private string _soLuongNhap = "1";
        public string SoLuongNhap
        {
            get => _soLuongNhap;
            set { _soLuongNhap = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                TimKiemPhieuNhap();
            }
        }

        private string _nhaSanXuatDuocChon;
        public string NhaSanXuatDuocChon
        {
            get => _nhaSanXuatDuocChon;
            set
            {
                _nhaSanXuatDuocChon = value;
                OnPropertyChanged();
                LocSanPhamTheoNSX();
            }
        }

        public bool IsEdit { get; set; }
        public string TieuDe => IsEdit ? "SỬA PHIẾU NHẬP" : "THÊM PHIẾU NHẬP";

        public string MaPhieuNhap { get; set; }
        public DateTime NgayNhap { get; set; }
        public string KhoDuocChon { get; set; }
        public string NguoiLapDuocChon { get; set; }
        public string TrangThaiDuocChon { get; set; }

        public ObservableCollection<string> DanhSachKho { get; set; }
        public ObservableCollection<string> DanhSachNhaSanXuat { get; set; }
        public ObservableCollection<string> DanhSachNguoiLap { get; set; }
        public ObservableCollection<string> DanhSachTrangThai { get; set; }

        public decimal TongTien
        {
            get
            {
                if (DanhSachChiTietNhap == null)
                    return 0;

                return DanhSachChiTietNhap.Sum(x => x.ThanhTien);
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ThoatCommand { get; }

        public ICommand ThemSanPhamCommand { get; }
        public ICommand XoaSanPhamCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand LuuTamCommand { get; }
        public ICommand HuyCommand { get; }

        public UcNhapKhoViewModel(Action moForm, Action quayLaiDanhSach, Action veTrangChu)
        {
            _moForm = moForm;
            _quayLaiDanhSach = quayLaiDanhSach;
            _veTrangChu = veTrangChu;

            DanhSachPhieuNhap = new ObservableCollection<PhieuNhapItem>();
            DanhSachChiTietNhap = new ObservableCollection<ChiTietNhapItem>();

            AddCommand = new RelayCommand(_ => MoThem());
            EditCommand = new RelayCommand(_ => MoSua());
            DeleteCommand = new RelayCommand(_ => XoaPhieuNhap());
            ThoatCommand = new RelayCommand(_ => VeTrangChu());

            ThemSanPhamCommand = new RelayCommand(_ => ThemSanPham());
            XoaSanPhamCommand = new RelayCommand(_ => XoaSanPham());
            LuuCommand = new RelayCommand(_ => LuuPhieuNhap("Đã nhập"));
            LuuTamCommand = new RelayCommand(_ => LuuPhieuNhap("Lưu tạm"));
            HuyCommand = new RelayCommand(_ => QuayLaiDanhSach());

            LoadComboBox();
            LoadPhieuNhap();
        }

        public void LoadPhieuNhap()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = LocPhieuNhapTheoTaiKhoan(db)
                    .ToList()
                    .Select((pn, index) => TaoPhieuNhapItem(pn, index))
                    .ToList();

                DanhSachPhieuNhap = new ObservableCollection<PhieuNhapItem>(ds);
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

                DanhSachNhaSanXuat = new ObservableCollection<string>(
                    db.NHASANXUATs.Select(x => x.TENNSX).ToList());

                DanhSachNguoiLap = new ObservableCollection<string>
        {
            CurrentUser.TenTK
        };

                NguoiLapDuocChon = CurrentUser.TenTK;

                TatCaSanPham = new ObservableCollection<SanPhamNhapItem>(
                    db.SANPHAMs
                        .ToList()
                        .Select(sp => new SanPhamNhapItem
                        {
                            MaSP = sp.MASP,
                            TenSP = sp.TENSP,
                            TenLoai = sp.LOAIHANG?.TENLOAI ?? "",
                            DonViTinh = sp.DONVITINH?.TENDVT ?? "",
                            DonGia = sp.DONGIA,
                            MaNSX = sp.MANSX
                        })
                        .ToList());

                DanhSachSanPham = new ObservableCollection<SanPhamNhapItem>(TatCaSanPham);
            }

            DanhSachTrangThai = new ObservableCollection<string>
    {
        "Lưu tạm",
        "Đã nhập",
        "Đã hủy"
    };

            OnPropertyChanged(nameof(DanhSachKho));
            OnPropertyChanged(nameof(DanhSachNhaSanXuat));
            OnPropertyChanged(nameof(DanhSachNguoiLap));
            OnPropertyChanged(nameof(DanhSachTrangThai));
            OnPropertyChanged(nameof(TatCaSanPham));
            OnPropertyChanged(nameof(DanhSachSanPham));
        }

        private PhieuNhapItem TaoPhieuNhapItem(PHIEUNHAP pn, int index)
        {
            return new PhieuNhapItem
            {
                STT = index + 1,
                MaPhieuNhap = pn.MAPN,
                NgayNhap = pn.NGAYNHAP.ToString("dd/MM/yyyy"),
                MaKho = pn.MAKHO,
                NhaSanXuat = pn.NHASANXUAT?.TENNSX ?? "",
                NguoiLap = pn.TAIKHOAN?.TENTK ?? "",
                TongTien = DinhDangTien(pn.TONGTIEN),
                TrangThai = pn.TRANGTHAI
            };
        }

        private void MoThem()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (!CoQuyenThemPhieuNhap(db))
                {
                    MessageBox.Show("Chỉ Admin và Nhân viên kho được thêm phiếu nhập!");
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
                MessageBox.Show("Vui lòng chọn phiếu nhập cần sửa!");
                return;
            }

            using (var db = new QUANLI_KHOHANGEntities())
            {
                if (!CoQuyenThemPhieuNhap(db))
                {
                    MessageBox.Show("Kế toán chỉ được xem phiếu nhập, không được sửa!");
                    return;
                }
            }

            if (SelectedItem.TrangThai != "Lưu tạm")
            {
                MessageBox.Show("Chỉ được sửa phiếu nhập có trạng thái Lưu tạm!");
                return;
            }

            IsEdit = true;
            DoDuLieuLenForm(SelectedItem);
            LoadChiTietPhieuNhap(SelectedItem.MaPhieuNhap);
            BaoThayDoiForm();

            _moForm?.Invoke();
        }

        private void XoaPhieuNhap()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu nhập cần xóa!");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa phiếu nhập này không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var pn = LocPhieuNhapTheoTaiKhoan(db)
                        .FirstOrDefault(x => x.MAPN == SelectedItem.MaPhieuNhap);

                    if (pn == null)
                    {
                        MessageBox.Show("Không tìm thấy phiếu nhập hoặc bạn không có quyền thao tác phiếu này!");
                        return;
                    }

                    bool laAdmin = LaAdmin(db);
                    bool laNhanVienKho = LaNhanVienKho(db);

                    if (!laAdmin && !laNhanVienKho)
                    {
                        MessageBox.Show("Kế toán chỉ được xem phiếu nhập, không được xóa/hủy!");
                        return;
                    }

                    if (pn.TRANGTHAI == "Đã hủy")
                    {
                        MessageBox.Show("Phiếu này đã bị hủy trước đó!");
                        return;
                    }

                    if (pn.TRANGTHAI == "Đã nhập")
                    {
                        if (!laAdmin)
                        {
                            MessageBox.Show("Chỉ Admin mới được hủy phiếu đã nhập!");
                            return;
                        }

                        TruTonKhoTheoChiTietCu(db, pn.MAKHO, pn.MAPN);
                        pn.TRANGTHAI = "Đã hủy";

                        GhiLog(db, "Hủy phiếu nhập", pn.MAPN,
                            "Admin chuyển phiếu nhập sang trạng thái Đã hủy");

                        db.SaveChanges();

                        MessageBox.Show("Đã chuyển phiếu nhập sang trạng thái Đã hủy!");
                        LoadPhieuNhap();
                        return;
                    }

                    if (pn.TRANGTHAI == "Lưu tạm")
                    {
                        var chiTiet = db.CT_PHIEUNHAP
                            .Where(x => x.MAPN == pn.MAPN)
                            .ToList();

                        foreach (var item in chiTiet)
                            db.CT_PHIEUNHAP.Remove(item);

                        db.PHIEUNHAPs.Remove(pn);

                        GhiLog(db, "Hủy phiếu nhập", pn.MAPN,
                            "Xóa phiếu nhập lưu tạm");

                        db.SaveChanges();

                        MessageBox.Show("Xóa phiếu nhập lưu tạm thành công!");
                        LoadPhieuNhap();
                        return;
                    }

                    MessageBox.Show("Phiếu này không thể xóa!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xóa phiếu nhập thất bại!\n" + LayLoiChiTiet(ex));
            }
        }

        private void LoadChiTietPhieuNhap(string maPN)
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.CT_PHIEUNHAP
                    .Where(x => x.MAPN == maPN)
                    .ToList()
                    .Select((ct, index) => new ChiTietNhapItem
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

                DanhSachChiTietNhap = new ObservableCollection<ChiTietNhapItem>(ds);
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
            if (!int.TryParse(SoLuongNhap, out soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên lớn hơn 0!");
                return;
            }

            var tonTai = DanhSachChiTietNhap
                .FirstOrDefault(x => x.MaHang == SanPhamDuocChon.MaSP);

            if (tonTai != null)
            {
                tonTai.SoLuong += soLuong;
                DanhSachChiTietNhap = new ObservableCollection<ChiTietNhapItem>(DanhSachChiTietNhap);
            }
            else
            {
                DanhSachChiTietNhap.Add(new ChiTietNhapItem
                {
                    STT = DanhSachChiTietNhap.Count + 1,
                    MaHang = SanPhamDuocChon.MaSP,
                    TenHang = SanPhamDuocChon.TenSP,
                    NhomHang = SanPhamDuocChon.TenLoai,
                    SoLuong = soLuong,
                    DonViTinh = SanPhamDuocChon.DonViTinh,
                    DonGia = SanPhamDuocChon.DonGia
                });
            }

            CapNhatSTTChiTiet();
            SoLuongNhap = "1";
            OnPropertyChanged(nameof(TongTien));
        }

        private void XoaSanPham()
        {
            if (SelectedChiTietNhap == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa khỏi phiếu!");
                return;
            }

            DanhSachChiTietNhap.Remove(SelectedChiTietNhap);
            CapNhatSTTChiTiet();

            OnPropertyChanged(nameof(DanhSachChiTietNhap));
            OnPropertyChanged(nameof(TongTien));
        }

        private void LuuPhieuNhap(string trangThai)
        {
            NguoiLapDuocChon = CurrentUser.TenTK;
            OnPropertyChanged(nameof(NguoiLapDuocChon));

            if (!KiemTraDuLieu())
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    if (!CoQuyenThemPhieuNhap(db))
                    {
                        MessageBox.Show("Kế toán chỉ được xem phiếu nhập, không được thêm/sửa!");
                        return;
                    }

                    if (IsEdit)
                        SuaPhieuNhap(db, trangThai);
                    else
                        ThemPhieuNhap(db, trangThai);

                    db.SaveChanges();
                }

                MessageBox.Show(trangThai == "Đã nhập"
                    ? "Lưu phiếu nhập thành công!"
                    : "Đã lưu tạm phiếu nhập!");

                QuayLaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lưu phiếu nhập thất bại!\n" + LayLoiChiTiet(ex));
            }
        }

        private void ThemPhieuNhap(QUANLI_KHOHANGEntities db, string trangThai)
        {
            if (db.PHIEUNHAPs.Any(x => x.MAPN == MaPhieuNhap))
                throw new Exception("Mã phiếu nhập đã tồn tại!");

            string maKho = LayMaKho(db);

            if (!CoQuyenThaoTacKho(db, maKho))
                throw new Exception("Bạn không có quyền nhập hàng vào kho này!");

            var pn = new PHIEUNHAP
            {
                MAPN = MaPhieuNhap,
                MAKHO = maKho,
                MANSX = LayMaNSX(db),
                MATK = CurrentUser.MaTK,
                NGAYNHAP = NgayNhap,
                TONGTIEN = TongTien,
                TRANGTHAI = trangThai
            };

            db.PHIEUNHAPs.Add(pn);

            foreach (var item in DanhSachChiTietNhap)
            {
                db.CT_PHIEUNHAP.Add(new CT_PHIEUNHAP
                {
                    MAPN = MaPhieuNhap,
                    MASP = item.MaHang,
                    SOLUONG = item.SoLuong,
                    DONGIA = item.DonGia
                });
            }

            if (trangThai == "Đã nhập")
                CongTonKhoTheoDanhSachMoi(db, maKho);

            GhiLog(db, "Tạo phiếu nhập", MaPhieuNhap,
                "Tạo phiếu nhập " + MaPhieuNhap);
        }

        private void SuaPhieuNhap(QUANLI_KHOHANGEntities db, string trangThai)
        {
            var pn = LocPhieuNhapTheoTaiKhoan(db)
                .FirstOrDefault(x => x.MAPN == MaPhieuNhap);

            if (pn == null)
                throw new Exception("Không tìm thấy phiếu nhập hoặc bạn không có quyền sửa phiếu này!");

            if (pn.TRANGTHAI != "Lưu tạm")
                throw new Exception("Chỉ được sửa phiếu nhập có trạng thái Lưu tạm!");

            string maKhoCu = pn.MAKHO;
            string maKhoMoi = LayMaKho(db);

            if (!CoQuyenThaoTacKho(db, maKhoMoi))
                throw new Exception("Bạn không có quyền nhập hàng vào kho này!");

            pn.MAKHO = maKhoMoi;
            pn.MANSX = LayMaNSX(db);
            pn.NGAYNHAP = NgayNhap;
            pn.TONGTIEN = TongTien;
            pn.TRANGTHAI = trangThai;

            var dsCu = db.CT_PHIEUNHAP
                .Where(x => x.MAPN == MaPhieuNhap)
                .ToList();

            foreach (var ct in dsCu)
                db.CT_PHIEUNHAP.Remove(ct);

            foreach (var item in DanhSachChiTietNhap)
            {
                db.CT_PHIEUNHAP.Add(new CT_PHIEUNHAP
                {
                    MAPN = MaPhieuNhap,
                    MASP = item.MaHang,
                    SOLUONG = item.SoLuong,
                    DONGIA = item.DonGia
                });
            }

            if (trangThai == "Đã nhập")
                CongTonKhoTheoDanhSachMoi(db, maKhoMoi);

            GhiLog(db, "Sửa phiếu nhập", MaPhieuNhap,
                "Sửa phiếu nhập " + MaPhieuNhap);
        }

        private void CongTonKhoTheoDanhSachMoi(QUANLI_KHOHANGEntities db, string maKho)
        {
            foreach (var item in DanhSachChiTietNhap)
            {
                var tonKho = db.TONKHOes.FirstOrDefault(x =>
                    x.MAKHO == maKho &&
                    x.MASP == item.MaHang);

                if (tonKho == null)
                {
                    tonKho = new TONKHO
                    {
                        MAKHO = maKho,
                        MASP = item.MaHang,
                        SOLUONGTON = 0
                    };

                    db.TONKHOes.Add(tonKho);
                }

                tonKho.SOLUONGTON += item.SoLuong;
            }
        }

        private void TruTonKhoTheoChiTietCu(QUANLI_KHOHANGEntities db, string maKho, string maPN)
        {
            var dsCu = db.CT_PHIEUNHAP
                .Where(x => x.MAPN == maPN)
                .ToList();

            foreach (var item in dsCu)
            {
                var tonKho = db.TONKHOes.FirstOrDefault(x =>
                    x.MAKHO == maKho &&
                    x.MASP == item.MASP);

                if (tonKho == null)
                    throw new Exception("Không tìm thấy tồn kho của sản phẩm " + item.MASP);

                if (tonKho.SOLUONGTON < item.SOLUONG)
                    throw new Exception("Tồn kho không đủ để hủy/sửa phiếu nhập " + maPN);

                tonKho.SOLUONGTON -= item.SOLUONG;
            }
        }

        private bool KiemTraDuLieu()
        {
            if (string.IsNullOrWhiteSpace(MaPhieuNhap))
            {
                MessageBox.Show("Mã phiếu nhập không được để trống!");
                return false;
            }

            if (!Regex.IsMatch(MaPhieuNhap, @"^PN\d{4}$"))
            {
                MessageBox.Show("Mã phiếu nhập phải có dạng PN0001!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(KhoDuocChon))
            {
                MessageBox.Show("Vui lòng chọn kho!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NhaSanXuatDuocChon))
            {
                MessageBox.Show("Vui lòng chọn nhà sản xuất!");
                return false;
            }

            if (DanhSachChiTietNhap == null || DanhSachChiTietNhap.Count == 0)
            {
                MessageBox.Show("Phiếu nhập phải có ít nhất 1 sản phẩm!");
                return false;
            }

            if (NgayNhap < DateTime.Today)
            {
                MessageBox.Show("Ngày nhập không hợp lệ!");
                return false;
            }

            return true;
        }
        private bool KiemTraMaPhieuNhap(string maPhieuNhap)
        {
            if (string.IsNullOrWhiteSpace(maPhieuNhap))
                return false;

            maPhieuNhap = maPhieuNhap.Trim();

            if (!maPhieuNhap.StartsWith("PN", StringComparison.OrdinalIgnoreCase))
                return false;

            string phanSo = maPhieuNhap.Substring(2);

            if (string.IsNullOrWhiteSpace(phanSo))
                return false;

            return phanSo.All(char.IsDigit);
        }

        private void TimKiemPhieuNhap()
        {
            string tuKhoa = SearchText?.Trim().ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = LocPhieuNhapTheoTaiKhoan(db)
                    .ToList()
                    .Where(pn =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        pn.MAPN.ToLower().Contains(tuKhoa) ||
                        pn.MAKHO.ToLower().Contains(tuKhoa) ||
                        pn.MANSX.ToLower().Contains(tuKhoa) ||
                        pn.MATK.ToLower().Contains(tuKhoa) ||
                        pn.TRANGTHAI.ToLower().Contains(tuKhoa) ||
                        (pn.NHASANXUAT != null &&
                         pn.NHASANXUAT.TENNSX.ToLower().Contains(tuKhoa)) ||
                        (pn.TAIKHOAN != null &&
                         pn.TAIKHOAN.TENTK.ToLower().Contains(tuKhoa)))
                    .Select((pn, index) => TaoPhieuNhapItem(pn, index))
                    .ToList();

                DanhSachPhieuNhap = new ObservableCollection<PhieuNhapItem>(ds);
            }
        }

        private void LocSanPhamTheoNSX()
        {
            if (TatCaSanPham == null)
                return;

            using (var db = new QUANLI_KHOHANGEntities())
            {
                string maNSX = LayMaNSX(db);

                DanhSachSanPham = new ObservableCollection<SanPhamNhapItem>(
                    TatCaSanPham.Where(x =>
                        string.IsNullOrWhiteSpace(maNSX) ||
                        x.MaNSX == maNSX));
            }

            SanPhamDuocChon = null;
        }

        private string LayMaKho(QUANLI_KHOHANGEntities db)
        {
            return db.KHOes.FirstOrDefault(x => x.TENKHO == KhoDuocChon)?.MAKHO;
        }

        private string LayTenKho(QUANLI_KHOHANGEntities db, string maKho)
        {
            return db.KHOes.FirstOrDefault(x => x.MAKHO == maKho)?.TENKHO;
        }

        private string LayMaNSX(QUANLI_KHOHANGEntities db)
        {
            return db.NHASANXUATs.FirstOrDefault(x => x.TENNSX == NhaSanXuatDuocChon)?.MANSX;
        }

        private string LayTenNSX(QUANLI_KHOHANGEntities db, string maNSX)
        {
            return db.NHASANXUATs.FirstOrDefault(x => x.MANSX == maNSX)?.TENNSX;
        }

        private void DoDuLieuLenForm(PhieuNhapItem item)
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var pn = LocPhieuNhapTheoTaiKhoan(db)
                    .FirstOrDefault(x => x.MAPN == item.MaPhieuNhap);

                if (pn == null)
                    return;

                MaPhieuNhap = pn.MAPN;
                NgayNhap = pn.NGAYNHAP;
                KhoDuocChon = LayTenKho(db, pn.MAKHO);
                NhaSanXuatDuocChon = LayTenNSX(db, pn.MANSX);
                NguoiLapDuocChon = pn.TAIKHOAN?.TENTK ?? CurrentUser.TenTK;
                TrangThaiDuocChon = pn.TRANGTHAI;
            }
        }

        private void XoaForm()
        {
            LoadComboBox();

            MaPhieuNhap = "";
            NgayNhap = DateTime.Now;
            KhoDuocChon = DanhSachKho?.FirstOrDefault();
            NhaSanXuatDuocChon = DanhSachNhaSanXuat?.FirstOrDefault();
            NguoiLapDuocChon = CurrentUser.TenTK;
            TrangThaiDuocChon = "Lưu tạm";
            SoLuongNhap = "1";

            DanhSachChiTietNhap = new ObservableCollection<ChiTietNhapItem>();

            BaoThayDoiForm();
        }

        private void CapNhatSTTChiTiet()
        {
            for (int i = 0; i < DanhSachChiTietNhap.Count; i++)
                DanhSachChiTietNhap[i].STT = i + 1;

            DanhSachChiTietNhap = new ObservableCollection<ChiTietNhapItem>(DanhSachChiTietNhap);
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

        private bool CoQuyenThemPhieuNhap(QUANLI_KHOHANGEntities db)
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

        private IQueryable<PHIEUNHAP> LocPhieuNhapTheoTaiKhoan(QUANLI_KHOHANGEntities db)
        {
            if (LaAdmin(db))
                return db.PHIEUNHAPs;

            return db.PHIEUNHAPs.Where(pn =>
                db.PHANCONG_KHO.Any(pc =>
                    pc.MATK == CurrentUser.MaTK &&
                    pc.MAKHO == pn.MAKHO &&
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
            LoadPhieuNhap();
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

        private void BaoThayDoiForm()
        {
            OnPropertyChanged(nameof(IsEdit));
            OnPropertyChanged(nameof(TieuDe));
            OnPropertyChanged(nameof(MaPhieuNhap));
            OnPropertyChanged(nameof(NgayNhap));
            OnPropertyChanged(nameof(KhoDuocChon));
            OnPropertyChanged(nameof(NhaSanXuatDuocChon));
            OnPropertyChanged(nameof(NguoiLapDuocChon));
            OnPropertyChanged(nameof(TrangThaiDuocChon));
            OnPropertyChanged(nameof(DanhSachChiTietNhap));
            OnPropertyChanged(nameof(DanhSachSanPham));
            OnPropertyChanged(nameof(SanPhamDuocChon));
            OnPropertyChanged(nameof(SoLuongNhap));
            OnPropertyChanged(nameof(TongTien));
        }
    }

    public class PhieuNhapItem
    {
        public int STT { get; set; }
        public string MaPhieuNhap { get; set; }
        public string NgayNhap { get; set; }
        public string MaKho { get; set; }
        public string NhaSanXuat { get; set; }
        public string NguoiLap { get; set; }
        public string TongTien { get; set; }
        public string TrangThai { get; set; }
    }

    public class ChiTietNhapItem
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

    public class SanPhamNhapItem
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public string TenLoai { get; set; }
        public string DonViTinh { get; set; }
        public decimal DonGia { get; set; }
        public string MaNSX { get; set; }
    }
}