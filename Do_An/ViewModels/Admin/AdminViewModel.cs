using Do_An.Helper;
using Do_An.Model;
using Do_An.View.Admin;
using Do_An.View.Shared;
using Do_An.ViewModel;
using Do_An.ViewModels.Shared;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Admin
{
    public class AdminViewModel : BaseViewModel
    {
        // ================= STATE MENU =================
        private bool _isTrangChuSelected = true;
        private bool _isNhanVienExpanded;
        private bool _isKhoExpanded;
        private bool _isHangHoaExpanded;
        private bool _isThongKeExpanded;
        private bool _isNhatKyExpanded;
        private bool _isMenuTaiKhoanOpen;
        private UcNhanVienViewModel _nhanVienViewModel;
        private UcTaiKhoanViewModel _taiKhoanViewModel;
        private UcNhaSanXuatViewModel _nhaSanXuatViewModel;
        private UcHangHoaViewModel _hangHoaViewModel;
        private UcLoaiHangViewModel _loaiHangViewModel;
        private UcKhoViewModel _khoViewModel;
        private UcNhapKhoViewModel _nhapKhoViewModel;
        private UcXuatKhoViewModel _xuatKhoViewModel;
        private UcKiemKeKhoViewModel _kiemKeKhoViewModel;
        private UcTonKhoViewModel _tonKhoViewModel;

        public bool IsTrangChuSelected
        {
            get => _isTrangChuSelected;
            set { _isTrangChuSelected = value; OnPropertyChanged(); }
        }

        public bool IsNhanVienExpanded
        {
            get => _isNhanVienExpanded;
            set { _isNhanVienExpanded = value; OnPropertyChanged(); }
        }

        public bool IsKhoExpanded
        {
            get => _isKhoExpanded;
            set { _isKhoExpanded = value; OnPropertyChanged(); }
        }

        public bool IsHangHoaExpanded
        {
            get => _isHangHoaExpanded;
            set { _isHangHoaExpanded = value; OnPropertyChanged(); }
        }

        public bool IsThongKeExpanded
        {
            get => _isThongKeExpanded;
            set { _isThongKeExpanded = value; OnPropertyChanged(); }
        }

        public bool IsNhatKyExpanded
        {
            get => _isNhatKyExpanded;
            set { _isNhatKyExpanded = value; OnPropertyChanged(); }
        }

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

        // ================= DASHBOARD DATA =================
        public string TongNhanVien { get; set; }
        public string TongKho { get; set; }
        public string TongHangHoa { get; set; }
        public string TongBaoCaoThang { get; set; }

        // ================= COMMAND =================
        public ICommand OpenTrangChuCommand { get; }
        public ICommand OpenNhanVienCommand { get; }
        public ICommand OpenTaiKhoanCommand { get; }
        public ICommand OpenNhaCungCapCommand { get; }
        public ICommand OpenNhatKyLoginCommand { get; }
        public ICommand OpenNhatKyThaoTacCommand { get; }

        public ICommand OpenKhoCommand { get; }
        public ICommand OpenHangHoaCommand { get; }
        public ICommand OpenLoaiHangCommand { get; }
        public ICommand OpenThongKeCommand { get; }
        public ICommand OpenNhapKhoCommand { get; }
        public ICommand OpenXuatKhoCommand { get; }
        public ICommand OpenKiemKeKhoCommand { get; }
        public ICommand OpenTonKhoCommand { get; }

        public ICommand ToggleNhanVienCommand { get; }
        public ICommand ToggleKhoCommand { get; }
        public ICommand ToggleHangHoaCommand { get; }
        public ICommand ToggleThongKeCommand { get; }
        public ICommand ToggleNhatKyCommand { get; }

        public ICommand ToggleMenuTaiKhoanCommand { get; }
        public ICommand ThongTinCommand { get; }
        public ICommand DangXuatCommand { get; }

        // ================= CONSTRUCTOR =================
        public AdminViewModel()
        {
            // NAVIGATION
            OpenTrangChuCommand = new RelayCommand(p => OpenTrangChu());
            OpenNhanVienCommand = new RelayCommand(p => OpenNhanVien());
            OpenTaiKhoanCommand = new RelayCommand(p => OpenTaiKhoan());
            OpenNhaCungCapCommand = new RelayCommand(p => OpenNhaCungCap());
            OpenNhatKyLoginCommand = new RelayCommand(p => OpenNhatKyLogin());
            OpenNhatKyThaoTacCommand = new RelayCommand(p => OpenNhatKyThaoTac());

            OpenKhoCommand = new RelayCommand(p => OpenKho());
            OpenHangHoaCommand = new RelayCommand(p => OpenHangHoa());
            OpenLoaiHangCommand = new RelayCommand(p => OpenLoaiHang());
            OpenThongKeCommand = new RelayCommand(p => OpenThongKe());
            OpenNhapKhoCommand = new RelayCommand(p => OpenNhapKho());
            OpenXuatKhoCommand = new RelayCommand(p => OpenXuatKho());
            OpenKiemKeKhoCommand = new RelayCommand(p => OpenKiemKeKho());
            OpenTonKhoCommand = new RelayCommand(p => OpenTonKho());

            // EXPAND
            ToggleNhanVienCommand = new RelayCommand(p => ToggleNhanVien());
            ToggleKhoCommand = new RelayCommand(p => ToggleKho());
            ToggleHangHoaCommand = new RelayCommand(p => ToggleHangHoa());
            ToggleThongKeCommand = new RelayCommand(p => ToggleThongKe());
            ToggleNhatKyCommand = new RelayCommand(p => ToggleNhatKy());

            // ACCOUNT
            ToggleMenuTaiKhoanCommand = new RelayCommand(p => ToggleMenuTaiKhoan());
            ThongTinCommand = new RelayCommand(p => ThongTin());
            DangXuatCommand = new RelayCommand(p => DangXuat());

            // INIT
            LoadDashboardData();
            OpenTrangChu();
        }

        // ================= LOAD DATA =================
        private void LoadDashboardData()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                TongNhanVien = db.NHANVIENs.Count().ToString("00");
                TongKho = db.KHOes.Count().ToString("00");
                TongHangHoa = db.SANPHAMs.Count().ToString("00");

                int thang = DateTime.Now.Month;
                int nam = DateTime.Now.Year;

                int pn = db.PHIEUNHAPs.Count(x => x.NGAYNHAP.Month == thang && x.NGAYNHAP.Year == nam);
                int px = db.PHIEUXUATs.Count(x => x.NGAYXUAT.Month == thang && x.NGAYXUAT.Year == nam);

                TongBaoCaoThang = (pn + px).ToString("00");

                OnPropertyChanged(nameof(TongNhanVien));
                OnPropertyChanged(nameof(TongKho));
                OnPropertyChanged(nameof(TongHangHoa));
                OnPropertyChanged(nameof(TongBaoCaoThang));
            }
        }

        // ================= NAVIGATION =================
        private void OpenTrangChu()
        {
            ResetMenu();
            IsTrangChuSelected = true;

            var uc = new UcTrangChuAdmin();
            uc.DataContext = this;

            CurrentView = uc;
        }

        private void OpenNhanVien()
        {
            ResetMenu();
            IsNhanVienExpanded = true;

            if (_nhanVienViewModel == null)
            {
                _nhanVienViewModel = new UcNhanVienViewModel(
                    MoFormNhanVien,
                    OpenNhanVien,
                    OpenTrangChu
                );
            }

            _nhanVienViewModel.LoadNhanVien();

            var uc = new UcNhanVien();
            uc.DataContext = _nhanVienViewModel;

            CurrentView = uc;
        }

        private void MoFormNhanVien()
        {
            var uc = new UcThemSuaNhanVien();
            uc.DataContext = _nhanVienViewModel;

            CurrentView = uc;
        }

        private void OpenTaiKhoan()
        {
            ResetMenu();
            IsNhanVienExpanded = true;

            if (_taiKhoanViewModel == null)
            {
                _taiKhoanViewModel = new UcTaiKhoanViewModel(
                    MoFormTaiKhoan,
                    OpenTaiKhoan,
                    OpenTrangChu
                );
            }

            _taiKhoanViewModel.LoadTaiKhoan();

            var uc = new UcTaiKhoan();
            uc.DataContext = _taiKhoanViewModel;

            CurrentView = uc;
        }
        private void MoFormTaiKhoan()
        {
            var uc = new UcThemSuaTaiKhoan();
            uc.DataContext = _taiKhoanViewModel;

            CurrentView = uc;
        }

        private void OpenNhaCungCap()
        {
            ResetMenu();
            IsHangHoaExpanded = true;

            if (_nhaSanXuatViewModel == null)
            {
                _nhaSanXuatViewModel = new UcNhaSanXuatViewModel(
                    MoFormNhaSanXuat,
                    OpenNhaCungCap,
                    OpenTrangChu
                );
            }

            _nhaSanXuatViewModel.LoadNhaSanXuat();

            var uc = new UcNhaSanXuat();
            uc.DataContext = _nhaSanXuatViewModel;

            CurrentView = uc;
        }

        private void MoFormNhaSanXuat()
        {
            var uc = new UcThemSuaNhaSanXuat();
            uc.DataContext = _nhaSanXuatViewModel;

            CurrentView = uc;
        }

        private void OpenNhatKyLogin()
        {
            ResetMenu();
            IsNhatKyExpanded = true;

            var uc = new UcNhatKyLogin();
            uc.DataContext = new UcNhatKyLoginViewModel(OpenTrangChu);

            CurrentView = uc;
        }

        private void OpenNhatKyThaoTac()
        {
            ResetMenu();
            IsNhatKyExpanded = true;

            var uc = new UcNhatKyThaoTac();
            uc.DataContext = new UcNhatKyThaoTacViewModel(OpenTrangChu);

            CurrentView = uc;
        }

        private void OpenKho()
        {
            ResetMenu();
            IsKhoExpanded = true;

            if (_khoViewModel == null)
            {
                _khoViewModel = new UcKhoViewModel(
                    MoFormKho,
                    OpenKho,
                    OpenTrangChu
                );
            }

            _khoViewModel.LoadKho();

            var uc = new UcKho();
            uc.DataContext = _khoViewModel;

            CurrentView = uc;
        }


        private void MoFormKho()
        {
            var uc = new UcThemSuaKho();
            uc.DataContext = _khoViewModel;

            CurrentView = uc;
        }


        private void OpenHangHoa()
        {
            ResetMenu();
            IsHangHoaExpanded = true;

            if (_hangHoaViewModel == null)
            {
                _hangHoaViewModel = new UcHangHoaViewModel(
                    MoFormHangHoa,
                    OpenHangHoa,
                    OpenTrangChu
                );
            }

            _hangHoaViewModel.LoadHangHoa();

            var uc = new UcHangHoa();
            uc.DataContext = _hangHoaViewModel;

            CurrentView = uc;
        }


        private void MoFormHangHoa()
        {
            var uc = new UcThemSuaHangHoa();
            uc.DataContext = _hangHoaViewModel;

            CurrentView = uc;
        }


        private void OpenLoaiHang()
        {
            ResetMenu();
            IsHangHoaExpanded = true;

            if (_loaiHangViewModel == null)
            {
                _loaiHangViewModel = new UcLoaiHangViewModel(
                    MoFormLoaiHang,
                    OpenLoaiHang,
                    OpenTrangChu
                );
            }

            _loaiHangViewModel.LoadLoaiHang();

            var uc = new UcLoaiHang();
            uc.DataContext = _loaiHangViewModel;

            CurrentView = uc;
        }

        private void MoFormLoaiHang()
        {
            var uc = new UcThemSuaLoaiHang();
            uc.DataContext = _loaiHangViewModel;

            CurrentView = uc;
        }


        private void OpenNhapKho()
        {
            ResetMenu();
            IsKhoExpanded = true;

            if (_nhapKhoViewModel == null)
            {
                _nhapKhoViewModel = new UcNhapKhoViewModel(
                     MoFormNhapKho,
                     OpenNhapKho,
                     OpenTrangChu
                 );
               
            }

            _nhapKhoViewModel.LoadPhieuNhap();

            var uc = new UcNhapKho();
            uc.DataContext = _nhapKhoViewModel;

            CurrentView = uc;
        }


        private void MoFormNhapKho()
        {
            var uc = new UcPhieuNhap();
            uc.DataContext = _nhapKhoViewModel;

            CurrentView = uc;
        }


        private void OpenXuatKho()
        {
            ResetMenu();
            IsKhoExpanded = true;

            if (_xuatKhoViewModel == null)
            {
                _xuatKhoViewModel = new UcXuatKhoViewModel(
                    MoFormXuatKho,
                    OpenXuatKho,
                    OpenTrangChu
                );
            }

            _xuatKhoViewModel.LoadPhieuXuat();

            var uc = new UcXuatKho();
            uc.DataContext = _xuatKhoViewModel;

            CurrentView = uc;
        }

        private void MoFormXuatKho()
        {
            var uc = new UcPhieuXuat();
            uc.DataContext = _xuatKhoViewModel;

            CurrentView = uc;
        }


        private void OpenKiemKeKho()
        {
            ResetMenu();
            IsKhoExpanded = true;

            if (_kiemKeKhoViewModel == null)
            {
                _kiemKeKhoViewModel = new UcKiemKeKhoViewModel(
                    MoFormKiemKeKho,
                    OpenKiemKeKho,
                    OpenTrangChu
                );
            }

            _kiemKeKhoViewModel.LoadKiemKe();

            var uc = new UcKiemKeKho();
            uc.DataContext = _kiemKeKhoViewModel;

            CurrentView = uc;
        }

        private void MoFormKiemKeKho()
        {
            var uc = new UcPhieuKiemKe();
            uc.DataContext = _kiemKeKhoViewModel;

            CurrentView = uc;
        }


        private void OpenTonKho()
        {
            ResetMenu();
            IsKhoExpanded = true;

            if (_tonKhoViewModel == null)
            {
                _tonKhoViewModel = new UcTonKhoViewModel(OpenTrangChu);
            }

            _tonKhoViewModel.LoadTonKho();

            var uc = new UcTonKho();
            uc.DataContext = _tonKhoViewModel;

            CurrentView = uc;
        }


        private void OpenThongKe()
        {
            ResetMenu();
            IsThongKeExpanded = true;

            var uc = new UcThongKeTongQuan();
            uc.DataContext = new UcThongKeTongQuanViewModel();

            CurrentView = uc;
        }

        // ================= TOGGLE =================
        private void ToggleNhanVien()
        {
            bool dangMo = IsNhanVienExpanded;
            ResetMenu();
            IsNhanVienExpanded = !dangMo;
        }

        private void ToggleKho()
        {
            bool dangMo = IsKhoExpanded;
            ResetMenu();
            IsKhoExpanded = !dangMo;
        }

        private void ToggleHangHoa()
        {
            bool dangMo = IsHangHoaExpanded;
            ResetMenu();
            IsHangHoaExpanded = !dangMo;
        }

        private void ToggleThongKe()
        {
            bool dangMo = IsThongKeExpanded;
            ResetMenu();
            IsThongKeExpanded = !dangMo;
        }

        private void ToggleNhatKy()
        {
            bool dangMo = IsNhatKyExpanded;
            ResetMenu();
            IsNhatKyExpanded = !dangMo;
        }

        // ================= ACCOUNT =================
        private void ToggleMenuTaiKhoan()
        {
            IsMenuTaiKhoanOpen = !IsMenuTaiKhoanOpen;
        }

        private void ThongTin()
        {
            IsMenuTaiKhoanOpen = false;

            var uc = new UcThongTin();
            uc.DataContext = new UcThongTinViewModel(OpenTrangChu);

            CurrentView = uc;
        }

        private void DangXuat()
        {
            IsMenuTaiKhoanOpen = false;

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
                        THOIGIAN = DateTime.Now, // 🔥 FIX CHÍNH Ở ĐÂY
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
                if (w is Do_An.View.Admin.Admin)
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
            IsNhanVienExpanded = false;
            IsKhoExpanded = false;
            IsHangHoaExpanded = false;
            IsThongKeExpanded = false;
            IsNhatKyExpanded = false;
            IsMenuTaiKhoanOpen = false;
        }
    }
}