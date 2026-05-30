using Do_An.Helper;
using Do_An.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Windows
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _loginButtonText = "Đăng nhập";
        public string LoginButtonText
        {
            get => _loginButtonText;
            set { _loginButtonText = value; OnPropertyChanged(); }
        }

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set { _isLoggingIn = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(p => Login(p), p => !IsLoggingIn);
        }

        private void Login(object parameter)
        {
            Window loginWindow = parameter as Window;

            if (loginWindow == null)
            {
                MessageBox.Show("Không lấy được cửa sổ đăng nhập!");
                return;
            }

            Username = Username?.Trim();
            Password = Password?.Trim();

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!");
                return;
            }

            IsLoggingIn = true;
            LoginButtonText = "Đang đăng nhập...";

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    var taiKhoan = db.TAIKHOANs
                        .Include("VAITROes")
                        .FirstOrDefault(x => x.TENTK == Username);

                    if (taiKhoan == null)
                    {
                        GhiLogDangNhap(db, null, "Đăng nhập", Username, "Thất bại", "Tài khoản không tồn tại");
                        db.SaveChanges();

                        MessageBox.Show("Tên đăng nhập không tồn tại!");
                        return;
                    }

                    if (taiKhoan.MATKHAU != Password)
                    {
                        GhiLogDangNhap(db, taiKhoan.MATK, "Đăng nhập", Username, "Thất bại", "Sai mật khẩu");
                        db.SaveChanges();

                        MessageBox.Show("Mật khẩu không đúng!");
                        return;
                    }

                    if (taiKhoan.TRANGTHAI == false)
                    {
                        GhiLogDangNhap(db, taiKhoan.MATK, "Đăng nhập", Username, "Thất bại", "Tài khoản bị khóa");
                        db.SaveChanges();

                        MessageBox.Show("Tài khoản đã bị khóa, không thể đăng nhập!");
                        return;
                    }

                    var nhanVien = db.NHANVIENs.FirstOrDefault(x => x.MANV == taiKhoan.MANV);

                    if (nhanVien == null || nhanVien.TRANGTHAI == false)
                    {
                        GhiLogDangNhap(db, taiKhoan.MATK, "Đăng nhập", Username, "Thất bại", "Nhân viên đã ngừng hoạt động");
                        db.SaveChanges();

                        MessageBox.Show("Nhân viên đã ngừng hoạt động!");
                        return;
                    }

                    string vaiTro = taiKhoan.VAITROes
                        .Select(x => x.TENVT)
                        .FirstOrDefault();

                    CurrentUser.MaTK = taiKhoan.MATK.Trim();
                    CurrentUser.TenTK = taiKhoan.TENTK;
                    CurrentUser.MaNV = taiKhoan.MANV.Trim();

                    Window windowCanMo = TaoCuaSoTheoVaiTro(vaiTro);

                    if (windowCanMo == null)
                    {
                        GhiLogDangNhap(db, taiKhoan.MATK, "Đăng nhập", Username, "Thất bại", "Chưa phân quyền");
                        db.SaveChanges();

                        MessageBox.Show("Tài khoản chưa được phân quyền!");
                        return;
                    }

                    GhiLogDangNhap(db, taiKhoan.MATK, "Đăng nhập", Username, "Thành công", "Đăng nhập hệ thống");
                    db.SaveChanges();

                    MessageBox.Show("Đăng nhập thành công!");

                    windowCanMo.Show();
                    loginWindow.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đăng nhập thất bại!\n" + ex.Message);
            }
            finally
            {
                ResetLoginState();
            }
        }

        private void GhiLogDangNhap(
            QUANLI_KHOHANGEntities db,
            string maTK,
            string hanhDong,
            string doiTuong,
            string trangThai,
            string ghiChu)
        {
            db.NHATKies.Add(new NHATKY
            {
                MATK = maTK,
                HANHDONG = hanhDong,
                DOITUONG = doiTuong,
                THOIGIAN = DateTime.Now,
                TRANGTHAI = trangThai,
                GHICHU = ghiChu
            });
        }

        private Window TaoCuaSoTheoVaiTro(string vaiTro)
        {
            if (vaiTro == "Admin")
                return new Do_An.View.Admin.Admin();

            if (vaiTro == "NhanVienKho")
                return new Do_An.View.NhanVienKho.NhanVienKho();
           

            if (vaiTro == "KeToan")
                return new Do_An.View.KeToan.KeToan();

            return null;
        }

        private void ResetLoginState()
        {
            IsLoggingIn = false;
            LoginButtonText = "Đăng nhập";
        }
    }
}