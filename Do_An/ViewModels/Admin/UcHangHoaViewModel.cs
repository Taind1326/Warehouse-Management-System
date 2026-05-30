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
    public class UcHangHoaViewModel : BaseViewModel
    {
        private readonly Action _moForm;
        private readonly Action _quayLaiDanhSach;
        private readonly Action _veTrangChu;

        private ObservableCollection<HangHoaItem> _danhSachHangHoa;
        public ObservableCollection<HangHoaItem> DanhSachHangHoa
        {
            get => _danhSachHangHoa;
            set { _danhSachHangHoa = value; OnPropertyChanged(); }
        }

        private HangHoaItem _hangHoaDangChon;
        public HangHoaItem HangHoaDangChon
        {
            get => _hangHoaDangChon;
            set { _hangHoaDangChon = value; OnPropertyChanged(); }
        }

        private string _tongHangHoa;
        public string TongHangHoa
        {
            get => _tongHangHoa;
            set { _tongHangHoa = value; OnPropertyChanged(); }
        }

        private bool _isEdit;
        public bool IsEdit
        {
            get => _isEdit;
            set
            {
                _isEdit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieuDe));
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                TimKiemHangHoa();
            }
        }

        public string TieuDe => IsEdit ? "SỬA HÀNG HÓA" : "THÊM HÀNG HÓA";

        public string MaHang { get; set; }
        public string TenHang { get; set; }
        public string TenLoaiHang { get; set; }
        public string TenNSX { get; set; }
        public string DonViTinh { get; set; }
        public string GiaNhap { get; set; }
        public string SoLuongTon { get; set; }
        public string SoLuongToiThieu { get; set; }
        public DateTime NgayNhap { get; set; }
        public string GhiChu { get; set; }

        public ObservableCollection<string> DanhSachLoaiHang { get; set; }
        public ObservableCollection<string> DanhSachNSX { get; set; }
        public ObservableCollection<string> DanhSachDonViTinh { get; set; }

        public ICommand ThemCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }
        public ICommand ThoatCommand { get; }

        public UcHangHoaViewModel(Action moForm, Action quayLaiDanhSach, Action veTrangChu)
        {
            _moForm = moForm;
            _quayLaiDanhSach = quayLaiDanhSach;
            _veTrangChu = veTrangChu;

            ThemCommand = new RelayCommand(p => MoThem());
            SuaCommand = new RelayCommand(p => MoSua());
            XoaCommand = new RelayCommand(p => XoaHangHoa());
            LuuCommand = new RelayCommand(p => Luu());
            HuyCommand = new RelayCommand(p => QuayLaiDanhSach());
            ThoatCommand = new RelayCommand(p => VeTrangChu());

            LoadComboBox();
            LoadHangHoa();
        }

        public void LoadHangHoa()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.SANPHAMs
                    .ToList()
                    .Select((sp, index) => TaoHangHoaItem(sp, index))
                    .ToList();

                DanhSachHangHoa = new ObservableCollection<HangHoaItem>(ds);
                TongHangHoa = ds.Count.ToString();
            }
        }

        private void LoadComboBox()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                DanhSachLoaiHang = new ObservableCollection<string>(
                    db.LOAIHANGs.Select(x => x.TENLOAI).ToList());

                DanhSachNSX = new ObservableCollection<string>(
                    db.NHASANXUATs.Select(x => x.TENNSX).ToList());

                DanhSachDonViTinh = new ObservableCollection<string>(
                    db.DONVITINHs.Select(x => x.TENDVT).ToList());
            }

            OnPropertyChanged(nameof(DanhSachLoaiHang));
            OnPropertyChanged(nameof(DanhSachNSX));
            OnPropertyChanged(nameof(DanhSachDonViTinh));
        }

        private HangHoaItem TaoHangHoaItem(SANPHAM sp, int index)
        {
            return new HangHoaItem
            {
                STT = index + 1,
                MaHang = sp.MASP,
                TenHang = sp.TENSP,
                TenLoaiHang = sp.LOAIHANG?.TENLOAI ?? "",
                TenNSX = sp.NHASANXUAT?.TENNSX ?? "",
                DonViTinh = sp.DONVITINH?.TENDVT ?? "",
                GiaNhap = DinhDangTien(sp.DONGIA),
                SoLuongTon = TinhSoLuongTon(sp).ToString()
            };
        }

        private int TinhSoLuongTon(SANPHAM sp)
        {
            if (sp.TONKHOes == null)
                return 0;

            return sp.TONKHOes.Sum(x => x.SOLUONGTON);
        }

        private string DinhDangTien(decimal soTien)
        {
            return soTien.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
        }

        private decimal ChuyenTienSangSo(string value)
        {
            value = value?.Replace(".", "")
                          .Replace(",", "")
                          .Replace("đ", "")
                          .Trim();

            decimal result;
            decimal.TryParse(value, out result);
            return result;
        }

        private void MoThem()
        {
            IsEdit = false;
            XoaForm();
            LoadComboBox();
            _moForm?.Invoke();
        }

        private void MoSua()
        {
            if (HangHoaDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn hàng hóa cần sửa!");
                return;
            }

            IsEdit = true;
            LoadComboBox();
            DoDuLieuLenForm(HangHoaDangChon);
            _moForm?.Invoke();
        }

        private void Luu()
        {
            if (!KiemTraDuLieu())
                return;

            try
            {
                bool thanhCong = IsEdit ? SuaHangHoa() : ThemHangHoa();

                if (!thanhCong)
                    return;

                MessageBox.Show("Lưu thành công!");
                QuayLaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lưu thất bại!\n" + ex.Message);
            }
        }

        private bool ThemHangHoa()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                MaHang = MaHang?.Trim();
                TenHang = TenHang?.Trim();

                if (db.SANPHAMs.Any(x => x.MASP.Trim().ToLower() == MaHang.ToLower()))
                {
                    MessageBox.Show("Mã hàng đã tồn tại!");
                    return false;
                }

                if (db.SANPHAMs.Any(x => x.TENSP.Trim().ToLower() == TenHang.ToLower()))
                {
                    MessageBox.Show("Tên hàng đã tồn tại!");
                    return false;
                }

                SANPHAM sp = TaoSanPhamMoi(db);
                db.SANPHAMs.Add(sp);


                GhiLog(db, "Thêm sản phẩm", MaHang, "Thêm hàng hóa " + TenHang);

                db.SaveChanges();
                return true;
            }
        }

        private bool SuaHangHoa()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                SANPHAM sp = db.SANPHAMs.FirstOrDefault(x => x.MASP == MaHang);

                if (sp == null)
                {
                    MessageBox.Show("Không tìm thấy hàng hóa cần sửa!");
                    return false;
                }

                if (db.SANPHAMs.Any(x =>
                    x.MASP != MaHang &&
                    x.TENSP.Trim().ToLower() == TenHang.ToLower()))
                {
                    MessageBox.Show("Tên hàng đã tồn tại!");
                    return false;
                }

                CapNhatSanPham(db, sp);

                GhiLog(db, "Sửa sản phẩm", MaHang, "Sửa hàng hóa " + TenHang);

                db.SaveChanges();
                return true;
            }
        }

        private SANPHAM TaoSanPhamMoi(QUANLI_KHOHANGEntities db)
        {
            return new SANPHAM
            {
                MASP = MaHang,
                TENSP = TenHang,
                MALOAI = LayMaLoaiHang(db),
                MADVT = LayMaDonViTinh(db),
                MANSX = LayMaNSX(db),
                DONGIA = ChuyenTienSangSo(GiaNhap),
                MUCTONTOITHIEU = LaySoLuongToiThieu()
            };
        }

        private void CapNhatSanPham(QUANLI_KHOHANGEntities db, SANPHAM sp)
        {
            sp.TENSP = TenHang;
            sp.MALOAI = LayMaLoaiHang(db);
            sp.MADVT = LayMaDonViTinh(db);
            sp.MANSX = LayMaNSX(db);
            sp.DONGIA = ChuyenTienSangSo(GiaNhap);
            sp.MUCTONTOITHIEU = LaySoLuongToiThieu();
        }

        private string LayMaLoaiHang(QUANLI_KHOHANGEntities db)
        {
            var loai = db.LOAIHANGs.FirstOrDefault(x => x.TENLOAI == TenLoaiHang);
            return loai?.MALOAI;
        }

        private string LayMaDonViTinh(QUANLI_KHOHANGEntities db)
        {
            var dvt = db.DONVITINHs.FirstOrDefault(x => x.TENDVT == DonViTinh);
            return dvt?.MADVT;
        }

        private string LayMaNSX(QUANLI_KHOHANGEntities db)
        {
            var nsx = db.NHASANXUATs.FirstOrDefault(x => x.TENNSX == TenNSX);
            return nsx?.MANSX;
        }

        private int LaySoLuongToiThieu()
        {
            int result;
            int.TryParse(SoLuongToiThieu, out result);
            return result;
        }

        private void XoaHangHoa()
        {
            if (HangHoaDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn hàng hóa cần xóa!");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa hàng hóa này không?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var sp = db.SANPHAMs.FirstOrDefault(x => x.MASP == HangHoaDangChon.MaHang);

                    if (sp == null)
                    {
                        MessageBox.Show("Không tìm thấy hàng hóa!");
                        return;
                    }

                    if (KiemTraSanPhamDaPhatSinh(sp))
                    {
                        MessageBox.Show("Không thể xóa vì sản phẩm đã phát sinh dữ liệu!");
                        return;
                    }

                    XoaTonKhoMacDinhCuaSanPham(db, sp.MASP);

                    db.SANPHAMs.Remove(sp);

                    GhiLog(db, "Xóa sản phẩm", sp.MASP, "Xóa hàng hóa " + sp.TENSP);

                    db.SaveChanges();
                }

                MessageBox.Show("Xóa thành công!");
                LoadHangHoa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xóa thất bại!\n" + ex.Message);
            }
        }

        private void XoaTonKhoMacDinhCuaSanPham(QUANLI_KHOHANGEntities db, string maSP)
        {
            var dsTonKho = db.TONKHOes
                .Where(x => x.MASP == maSP && x.SOLUONGTON == 0)
                .ToList();

            foreach (var tonKho in dsTonKho)
            {
                db.TONKHOes.Remove(tonKho);
            }
        }

        private bool KiemTraSanPhamDaPhatSinh(SANPHAM sp)
        {
            if (sp.CT_PHIEUNHAP.Any()) return true;
            if (sp.CT_PHIEUXUAT.Any()) return true;
            if (sp.CT_KIEMKE.Any()) return true;
            if (sp.TONKHOes.Any(x => x.SOLUONGTON > 0)) return true;

            return false;
        }

        private void TimKiemHangHoa()
        {
            string tuKhoa = SearchText?.Trim().ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.SANPHAMs
                    .ToList()
                    .Where(x =>
                        string.IsNullOrWhiteSpace(tuKhoa) ||
                        x.MASP.ToLower().Contains(tuKhoa) ||
                        x.TENSP.ToLower().Contains(tuKhoa) ||
                        (x.LOAIHANG != null &&
                         x.LOAIHANG.TENLOAI.ToLower().Contains(tuKhoa)) ||
                        (x.NHASANXUAT != null &&
                         x.NHASANXUAT.TENNSX.ToLower().Contains(tuKhoa)))
                    .Select((sp, index) => TaoHangHoaItem(sp, index))
                    .ToList();

                DanhSachHangHoa = new ObservableCollection<HangHoaItem>(ds);
                TongHangHoa = ds.Count.ToString();
            }
        }

        private void DoDuLieuLenForm(HangHoaItem hh)
        {
            MaHang = hh.MaHang;
            TenHang = hh.TenHang;
            TenLoaiHang = hh.TenLoaiHang;
            TenNSX = hh.TenNSX;
            DonViTinh = hh.DonViTinh;
            GiaNhap = hh.GiaNhap;
            SoLuongTon = hh.SoLuongTon;

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var sp = db.SANPHAMs.FirstOrDefault(x => x.MASP == hh.MaHang);
                SoLuongToiThieu = sp?.MUCTONTOITHIEU.ToString() ?? "0";
            }

            BaoThayDoiForm();
        }

        private void XoaForm()
        {
            MaHang = "";
            TenHang = "";
            TenLoaiHang = DanhSachLoaiHang?.FirstOrDefault();
            TenNSX = DanhSachNSX?.FirstOrDefault();
            DonViTinh = DanhSachDonViTinh?.FirstOrDefault();
            GiaNhap = "0";
            SoLuongTon = "0";
            SoLuongToiThieu = "5";
            NgayNhap = DateTime.Now;
            GhiChu = "";

            BaoThayDoiForm();
        }

        private bool KiemTraDuLieu()
        {
            MaHang = MaHang?.Trim();
            TenHang = TenHang?.Trim();

            if (string.IsNullOrWhiteSpace(MaHang))
            {
                MessageBox.Show("Vui lòng nhập mã hàng!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TenHang))
            {
                MessageBox.Show("Vui lòng nhập tên hàng!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TenLoaiHang))
            {
                MessageBox.Show("Vui lòng chọn loại hàng!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TenNSX))
            {
                MessageBox.Show("Vui lòng chọn nhà sản xuất!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(DonViTinh))
            {
                MessageBox.Show("Vui lòng chọn đơn vị tính!");
                return false;
            }

            decimal giaNhap = ChuyenTienSangSo(GiaNhap);
            if (giaNhap < 0)
            {
                MessageBox.Show("Giá nhập không được âm!");
                return false;
            }

            int mucTon;
            if (!int.TryParse(SoLuongToiThieu, out mucTon) || mucTon < 0)
            {
                MessageBox.Show("Số lượng tối thiểu phải là số nguyên không âm!");
                return false;
            }

            return true;
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
            LoadHangHoa();
            _quayLaiDanhSach?.Invoke();
        }

        private void VeTrangChu()
        {
            _veTrangChu?.Invoke();
        }

        private void BaoThayDoiForm()
        {
            OnPropertyChanged(nameof(IsEdit));
            OnPropertyChanged(nameof(TieuDe));
            OnPropertyChanged(nameof(MaHang));
            OnPropertyChanged(nameof(TenHang));
            OnPropertyChanged(nameof(TenLoaiHang));
            OnPropertyChanged(nameof(TenNSX));
            OnPropertyChanged(nameof(DonViTinh));
            OnPropertyChanged(nameof(GiaNhap));
            OnPropertyChanged(nameof(SoLuongTon));
            OnPropertyChanged(nameof(SoLuongToiThieu));
            OnPropertyChanged(nameof(NgayNhap));
            OnPropertyChanged(nameof(GhiChu));
        }
    }

    public class HangHoaItem
    {
        public int STT { get; set; }
        public string MaHang { get; set; }
        public string TenHang { get; set; }
        public string TenLoaiHang { get; set; }
        public string TenNSX { get; set; }
        public string DonViTinh { get; set; }
        public string GiaNhap { get; set; }
        public string SoLuongTon { get; set; }
    }
}

