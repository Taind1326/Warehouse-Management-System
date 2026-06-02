using Do_An.Helper;
using Do_An.Model;
using Do_An.View.KeToan;
using Do_An.View.Shared;
using Do_An.ViewModels.Shared;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.KeToan
{
    public class KeToanViewModel : BaseViewModel
    {
        // ================= STATE MENU =================
        private bool _isTrangChuSelected = true;
        private bool _isChungTuExpanded;
        private bool _isThongKeExpanded;
        private bool _isTaiKhoanExpanded;
        private bool _isMenuTaiKhoanOpen;

        public bool IsTrangChuSelected
        {
            get => _isTrangChuSelected;
            set { _isTrangChuSelected = value; OnPropertyChanged(); }
        }

        public bool IsChungTuExpanded
        {
            get => _isChungTuExpanded;
            set { _isChungTuExpanded = value; OnPropertyChanged(); }
        }

        public bool IsThongKeExpanded
        {
            get => _isThongKeExpanded;
            set { _isThongKeExpanded = value; OnPropertyChanged(); }
        }

        public bool IsTaiKhoanExpanded
        {
            get => _isTaiKhoanExpanded;
            set { _isTaiKhoanExpanded = value; OnPropertyChanged(); }
        }

        public bool IsMenuTaiKhoanOpen
        {
            get => _isMenuTaiKhoanOpen;
            set { _isMenuTaiKhoanOpen = value; OnPropertyChanged(); }
        }

        private int _tongPhieuNhap;
        public int TongPhieuNhap
        {
            get => _tongPhieuNhap;
            set { _tongPhieuNhap = value; OnPropertyChanged(); }
        }

        private int _tongPhieuXuat;
        public int TongPhieuXuat
        {
            get => _tongPhieuXuat;
            set { _tongPhieuXuat = value; OnPropertyChanged(); }
        }

        private decimal _tongGiaTriNhap;
        public decimal TongGiaTriNhap
        {
            get => _tongGiaTriNhap;
            set
            {
                _tongGiaTriNhap = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TongGiaTriNhapText));
            }
        }

        public string TongGiaTriNhapText =>
            TongGiaTriNhap.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + " đ";

        // ================= CURRENT VIEW =================
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        // ================= COMMAND =================
        public ICommand TrangChuCommand { get; }
        public ICommand PhieuNhapCommand { get; }
        public ICommand PhieuXuatCommand { get; }

        public ICommand ThongKeNhapKhoCommand { get; }
        public ICommand ThongKeXuatKhoCommand { get; }

        public ICommand ThongTinTaiKhoanCommand { get; }
        public ICommand DoiMatKhauCommand { get; }
        public ICommand DangXuatCommand { get; }

        public ICommand ToggleChungTuCommand { get; }
        public ICommand ToggleThongKeCommand { get; }
        public ICommand ToggleTaiKhoanCommand { get; }
        public ICommand ToggleMenuTaiKhoanCommand { get; }

        // ================= CONSTRUCTOR =================
        public KeToanViewModel()
        {
            TrangChuCommand = new RelayCommand(_ => OpenTrangChu());

            PhieuNhapCommand = new RelayCommand(_ => OpenPhieuNhap());
            PhieuXuatCommand = new RelayCommand(_ => OpenPhieuXuat());

            ThongKeNhapKhoCommand = new RelayCommand(_ => OpenThongKeNhapKho());
            ThongKeXuatKhoCommand = new RelayCommand(_ => OpenThongKeXuatKho());

            ThongTinTaiKhoanCommand = new RelayCommand(_ => OpenThongTinTaiKhoan());
            DoiMatKhauCommand = new RelayCommand(_ => OpenDoiMatKhau());
            DangXuatCommand = new RelayCommand(_ => DangXuat());

            ToggleChungTuCommand = new RelayCommand(_ => ToggleChungTu());
            ToggleThongKeCommand = new RelayCommand(_ => ToggleThongKe());
            ToggleTaiKhoanCommand = new RelayCommand(_ => ToggleTaiKhoan());
            ToggleMenuTaiKhoanCommand = new RelayCommand(_ => ToggleMenuTaiKhoan());
            LoadTongQuan();

            OpenTrangChu();
        }

        // ================= NAVIGATION =================
        private void OpenTrangChu()
        {
            ResetMenu();
            IsTrangChuSelected = true;

            var uc = new UcTrangChuKeToan();
            uc.DataContext = this;

            CurrentView = uc;
        }

        private void OpenPhieuNhap()
        {
            ResetMenu();
            IsChungTuExpanded = true;

            var uc = new UcNhapKho();
            uc.DataContext = new UcNhapKhoViewModel(
                moForm: null,
                quayLaiDanhSach: null,
                veTrangChu: OpenTrangChu);

            CurrentView = uc;
        }

        private void OpenPhieuXuat()
        {
            ResetMenu();
            IsChungTuExpanded = true;

            var uc = new UcXuatKho();
            uc.DataContext = new UcXuatKhoViewModel(
                moForm: null,
                quayLaiDanhSach: null,
                veTrangChu: OpenTrangChu);

            CurrentView = uc;
        }

        private void OpenThongKeNhapKho()
        {
            ResetMenu();
            IsThongKeExpanded = true;

            var uc = new UcThongKeNhapKho();
            uc.DataContext = new UcThongKeNhapKhoViewModel(OpenTrangChu);

            CurrentView = uc;
        }

        private void OpenThongKeXuatKho()
        {
            ResetMenu();
            IsThongKeExpanded = true;

            var uc = new UcThongKeXuatKho();
            uc.DataContext = new UcThongKeXuatKhoViewModel(OpenTrangChu);

            CurrentView = uc;
        }

        private void OpenThongTinTaiKhoan()
        {
            ResetMenu();
            IsTaiKhoanExpanded = true;

            var uc = new UcThongTin();
            uc.DataContext = new UcThongTinViewModel(OpenTrangChu);

            CurrentView = uc;
        }

        private void OpenDoiMatKhau()
        {
            ResetMenu();
            IsTaiKhoanExpanded = true;

            var uc = new UcDoiMatKhau();
            uc.DataContext = new UcDoiMatKhauViewModel(OpenTrangChu);

            CurrentView = uc;
        }

        // ================= TOGGLE =================
        private void ToggleChungTu()
        {
            bool dangMo = IsChungTuExpanded;
            ResetMenu();
            IsChungTuExpanded = !dangMo;
        }

        private void ToggleThongKe()
        {
            bool dangMo = IsThongKeExpanded;
            ResetMenu();
            IsThongKeExpanded = !dangMo;
        }

        private void ToggleTaiKhoan()
        {
            bool dangMo = IsTaiKhoanExpanded;
            ResetMenu();
            IsTaiKhoanExpanded = !dangMo;
        }

        private void ToggleMenuTaiKhoan()
        {
            IsMenuTaiKhoanOpen = !IsMenuTaiKhoanOpen;
        }


        private void LoadTongQuan()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var dsKhoDuocPhanCong = db.PHANCONG_KHO
                    .Where(pc => pc.MATK == CurrentUser.MaTK && pc.TRANGTHAI == true)
                    .Select(pc => pc.MAKHO)
                    .ToList();

                var dsPhieuNhap = db.PHIEUNHAPs
                    .Where(pn => dsKhoDuocPhanCong.Contains(pn.MAKHO)
                              && pn.TRANGTHAI != "Đã hủy")
                    .ToList();

                var dsPhieuXuat = db.PHIEUXUATs
                    .Where(px => dsKhoDuocPhanCong.Contains(px.MAKHO)
                              && px.TRANGTHAI != "Đã hủy")
                    .ToList();

                var dsMaPN = dsPhieuNhap.Select(pn => pn.MAPN).ToList();

                TongPhieuNhap = dsPhieuNhap.Count;
                TongPhieuXuat = dsPhieuXuat.Count;

                TongGiaTriNhap = db.CT_PHIEUNHAP
                    .Where(ct => dsMaPN.Contains(ct.MAPN))
                    .Sum(ct => (decimal?)(ct.SOLUONG * ct.DONGIA)) ?? 0;
            }
        }
        // ================= ACCOUNT =================
        private void DangXuat()
        {
            var hoi = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất không?",
                "Xác nhận đăng xuất",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (hoi != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    db.NHATKies.Add(new NHATKY
                    {
                        MATK = CurrentUser.MaTK,
                        HANHDONG = "Đăng xuất",
                        DOITUONG = CurrentUser.TenTK,
                        THOIGIAN = DateTime.Now,
                        TRANGTHAI = "Thành công",
                        GHICHU = "Tài khoản đăng xuất khỏi hệ thống"
                    });

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ghi nhật ký đăng xuất thất bại!\n" + ex.Message);
            }

            MessageBox.Show("Đăng xuất thành công!");

            new Do_An.View.Windows.Login().Show();

            foreach (Window w in Application.Current.Windows)
            {
                if (w is Do_An.View.KeToan.KeToan)
                {
                    w.Close();
                    break;
                }
            }
        }

        // ================= HELPER =================
        private void ResetMenu()
        {
            IsTrangChuSelected = false;
            IsChungTuExpanded = false;
            IsThongKeExpanded = false;
            IsTaiKhoanExpanded = false;
            IsMenuTaiKhoanOpen = false;
        }
    }
}