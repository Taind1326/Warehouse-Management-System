using Do_An.Helper;
using Do_An.Model;
using Do_An.View.KeToan;
using Do_An.View.Shared;
using Do_An.ViewModels.Shared;
using System;
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

        private bool _isMenuTaiKhoanOpen;
        public bool IsMenuTaiKhoanOpen
        {
            get => _isMenuTaiKhoanOpen;
            set { _isMenuTaiKhoanOpen = value; OnPropertyChanged(); }
        }


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
        public ICommand OpenThongKeXuatKhoCommand { get; }
        public ICommand ThongKePhieuXuatCommand { get; }
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
            // NAVIGATION
            TrangChuCommand = new RelayCommand(_ => OpenTrangChu());
            PhieuNhapCommand = new RelayCommand(_ => OpenPhieuNhap());
            PhieuXuatCommand = new RelayCommand(_ => OpenPhieuXuat());
            //ThongKeNhapKhoCommand = new RelayCommand(_ => OpenThongKeNhapKho());
            ThongKePhieuXuatCommand = new RelayCommand(_ => OpenThongKePhieuXuat());
            ThongTinTaiKhoanCommand = new RelayCommand(_ => OpenThongTinTaiKhoan());
            DoiMatKhauCommand = new RelayCommand(_ => OpenDoiMatKhau());

            // TOGGLE
            ToggleChungTuCommand = new RelayCommand(_ => ToggleChungTu());
            ToggleThongKeCommand = new RelayCommand(_ => ToggleThongKe());
            ToggleTaiKhoanCommand = new RelayCommand(_ => ToggleTaiKhoan());
            ToggleMenuTaiKhoanCommand = new RelayCommand(_ => ToggleMenuTaiKhoan());

            // ACCOUNT
            DangXuatCommand = new RelayCommand(_ => DangXuat());

            // INIT
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

            CurrentView = new UcTrangChuKeToan();
        }

        private void OpenPhieuXuat()
        {
            ResetMenu();
            IsChungTuExpanded = true;

            CurrentView = new UcTrangChuKeToan();
        }

        private void OpenThongKePhieuNhap()
        {
            ResetMenu();
            IsThongKeExpanded = true;

            CurrentView = new UcTrangChuKeToan();
        }

        private void OpenThongKePhieuXuat()
        {
            ResetMenu();
            IsThongKeExpanded = true;

            CurrentView = new UcTrangChuKeToan();
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

        // ================= ACCOUNT =================
        private void ToggleMenuTaiKhoan()
        {
            IsMenuTaiKhoanOpen = !IsMenuTaiKhoanOpen;
        }

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