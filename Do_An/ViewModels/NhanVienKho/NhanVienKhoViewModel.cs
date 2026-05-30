using Do_An.Helper;
using Do_An.Model;
using Do_An.View.NhanVienKho;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Do_An.View.Shared;
using Do_An.ViewModels.Shared;

namespace Do_An.ViewModels.NhanVienKho
{
    public class NhanVienKhoViewModel : BaseViewModel
    {
        // ================= STATE MENU =================
        private bool _isTrangChuSelected = true;
        private bool _isHangHoaSelected;
        private bool _isKhoExpanded;
        private bool _isLichSuExpanded;
        private bool _isTaiKhoanExpanded;

        public bool IsTrangChuSelected
        {
            get => _isTrangChuSelected;
            set { _isTrangChuSelected = value; OnPropertyChanged(); }
        }

        public bool IsHangHoaSelected
        {
            get => _isHangHoaSelected;
            set { _isHangHoaSelected = value; OnPropertyChanged(); }
        }

        public bool IsKhoExpanded
        {
            get => _isKhoExpanded;
            set { _isKhoExpanded = value; OnPropertyChanged(); }
        }

        public bool IsLichSuExpanded
        {
            get => _isLichSuExpanded;
            set { _isLichSuExpanded = value; OnPropertyChanged(); }
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


        // ================= DASHBOARD DATA =================
        public string TongKho { get; set; }
        public string TongHangHoa { get; set; }
        public string TongSoLuongTon { get; set; }


        // ================= COMMAND =================
        public ICommand OpenTrangChuCommand { get; }
        public ICommand OpenKhoCommand { get; }
        public ICommand OpenHangHoaCommand { get; }
        public ICommand OpenTonKhoCommand { get; }
        public ICommand OpenLichSuNhapKhoCommand { get; }
        public ICommand OpenLichSuXuatKhoCommand { get; }
        public ICommand OpenLichSuKiemKeKhoCommand { get; }
        public ICommand OpenDoiMatKhauCommand { get; }


        public ICommand ToggleKhoCommand { get; }
        public ICommand ToggleLichSuCommand { get; }
        public ICommand ToggleTaiKhoanCommand { get; }
        public ICommand ToggleMenuTaiKhoanCommand { get; }
        public ICommand ThongTinCommand { get; }
        public ICommand DangXuatCommand { get; }


        // ================= CONSTRUCTOR =================
        public NhanVienKhoViewModel()
        {
            // NAVIGATION
            OpenTrangChuCommand = new RelayCommand(p => OpenTrangChu());
            OpenKhoCommand = new RelayCommand(p => OpenKho());
            OpenHangHoaCommand = new RelayCommand(p => OpenHangHoa());
            OpenTonKhoCommand = new RelayCommand(p => OpenTonKho());
            OpenLichSuNhapKhoCommand = new RelayCommand(p => OpenLichSuNhapKho());
            OpenLichSuXuatKhoCommand = new RelayCommand(p => OpenLichSuXuatKho());
            OpenLichSuKiemKeKhoCommand =new RelayCommand(p => OpenLichSuKiemKeKho());
            OpenDoiMatKhauCommand = new RelayCommand(p => OpenDoiMatKhau());

            // EXPAND
            ToggleKhoCommand = new RelayCommand(p => ToggleKho());
            ToggleLichSuCommand = new RelayCommand(p => ToggleLichSu());
            ToggleTaiKhoanCommand = new RelayCommand(p => ToggleTaiKhoan());

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
                string maTK = CurrentUser.MaTK?.Trim();

                if (string.IsNullOrWhiteSpace(maTK))
                {
                    GanDashboardMacDinh();
                    return;
                }

                string maKho = LayMaKhoDangPhanCong(db, maTK);

                if (string.IsNullOrWhiteSpace(maKho))
                {
                    GanDashboardMacDinh();
                    return;
                }

                TongKho = db.KHOes
                    .Count()
                    .ToString("00");

                TongHangHoa = db.TONKHOes
                    .Count(x => x.MAKHO == maKho)
                    .ToString("00");

                int tongTon = db.TONKHOes
                    .Where(x => x.MAKHO == maKho)
                    .Select(x => x.SOLUONGTON)
                    .DefaultIfEmpty(0)
                    .Sum();

                TongSoLuongTon = tongTon.ToString("00");

                OnPropertyChanged(nameof(TongKho));
                OnPropertyChanged(nameof(TongHangHoa));
                OnPropertyChanged(nameof(TongSoLuongTon));
            }
        }

        private string LayMaKhoDangPhanCong(
            QUANLI_KHOHANGEntities db,
            string maTK)
        {
            return db.PHANCONG_KHO
                .Where(x =>
                    x.MATK == maTK &&
                    x.TRANGTHAI == true)
                .Select(x => x.MAKHO)
                .FirstOrDefault();
        }

        private void GanDashboardMacDinh()
        {
            TongKho = "00";
            TongHangHoa = "00";
            TongSoLuongTon = "00";

            OnPropertyChanged(nameof(TongKho));
            OnPropertyChanged(nameof(TongHangHoa));
            OnPropertyChanged(nameof(TongSoLuongTon));
        }


        // ================= NAVIGATION =================
        private void OpenTrangChu()
        {
            ResetMenu();
            IsTrangChuSelected = true;

            LoadDashboardData();

            var uc = new UcTrangChuNhanVienKho();
            uc.DataContext = this;

            CurrentView = uc;
        }

        private void OpenKho()
        {
            ResetMenu();
            IsKhoExpanded = true;

            var uc = new UcKho();
            uc.DataContext = new UcKhoViewModel(OpenTrangChu);

            CurrentView = uc;
        }

        private void OpenHangHoa()
        {
            ResetMenu();
            IsHangHoaSelected = true;

            var uc = new UcHangHoa();
            uc.DataContext = new UcHangHoaViewModel(OpenTrangChu);

            CurrentView = uc;
        }


        private void OpenLichSuNhapKho()
        {
            ResetMenu();
            IsLichSuExpanded = true;

            var uc = new UcLichSuNhapKho();
            uc.DataContext = new UcLichSuNhapKhoViewModel(OpenTrangChu);

            CurrentView = uc;
        }


        private void OpenLichSuXuatKho()
        {
            ResetMenu();
            IsLichSuExpanded = true;

            var uc = new UcLichSuXuatKho();
            uc.DataContext = new UcLichSuXuatKhoViewModel(OpenTrangChu);

            CurrentView = uc;
        }


        private void OpenLichSuKiemKeKho()
        {
            ResetMenu();
            IsLichSuExpanded = true;

            var uc = new UcLichSuKiemKeKho();

            uc.DataContext =
                new UcLichSuKiemKeKhoViewModel(OpenTrangChu);

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

        private void OpenTonKho()
        {
            ResetMenu();
            IsKhoExpanded = true;

            // var uc = new UcTonKho();
            // uc.DataContext = new UcTonKhoViewModel();
            // CurrentView = uc;
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
                        THOIGIAN = System.DateTime.Now,
                        TRANGTHAI = "Thành công",
                        GHICHU = "Tài khoản đăng xuất khỏi hệ thống"
                    });

                    db.SaveChanges();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ghi nhật ký đăng xuất thất bại!\n" + ex.Message);
            }

            MessageBox.Show("Đăng xuất thành công!");

            new Do_An.View.Windows.Login().Show();

            foreach (Window w in Application.Current.Windows)
            {
                if (w is Do_An.View.NhanVienKho.NhanVienKho)
                {
                    w.Close();
                    break;
                }
            }
        }


        // ================= TOGGLE =================
        private void ToggleKho()
        {
            bool dangMo = IsKhoExpanded;
            ResetMenu();
            IsKhoExpanded = !dangMo;
        }

        private void ToggleLichSu()
        {
            bool dangMo = IsLichSuExpanded;
            ResetMenu();
            IsLichSuExpanded = !dangMo;
        }

        private void ToggleTaiKhoan()
        {
            bool dangMo = IsTaiKhoanExpanded;
            ResetMenu();
            IsTaiKhoanExpanded = !dangMo;
        }

        // ================= HELPER =================
        private void ResetMenu()
        {
            IsTrangChuSelected = false;
            IsMenuTaiKhoanOpen = false;
            IsHangHoaSelected = false;
            IsKhoExpanded = false;
            IsLichSuExpanded = false;
            IsTaiKhoanExpanded = false;
        }
    }
}