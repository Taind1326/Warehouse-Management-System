using Do_An.Helper;
using Do_An.Model;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Do_An.ViewModels.Shared
{
    public class UcDoiMatKhauViewModel : BaseViewModel
    {
        private readonly Action _thoat;

        public string MatKhauCu { get; set; }
        public string MatKhauMoi { get; set; }
        public string XacNhanMatKhau { get; set; }

        private string _thongBao;
        public string ThongBao
        {
            get => _thongBao;
            set
            {
                _thongBao = value;
                OnPropertyChanged();
            }
        }

        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }

        public UcDoiMatKhauViewModel(Action thoat = null)
        {
            _thoat = thoat;

            LuuCommand = new RelayCommand(_ => LuuMatKhau());
            HuyCommand = new RelayCommand(_ => Huy());
        }

        private void LuuMatKhau()
        {
            if (!KiemTraDuLieu())
                return;

            try
            {
                using (var db = new QUANLI_KHOHANGEntities())
                {
                    string maTK = CurrentUser.MaTK?.Trim();

                    var taiKhoan = db.TAIKHOANs
                        .FirstOrDefault(x => x.MATK == maTK);

                    if (taiKhoan == null)
                    {
                        ThongBao = "Không tìm thấy tài khoản hiện tại.";
                        return;
                    }

                    if (taiKhoan.MATKHAU != MatKhauCu)
                    {
                        ThongBao = "Mật khẩu hiện tại không đúng.";
                        return;
                    }

                    taiKhoan.MATKHAU = MatKhauMoi;

                    GhiLogDoiMatKhau(db, maTK);

                    db.SaveChanges();
                }

                MessageBox.Show(
                    "Đổi mật khẩu thành công!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _thoat?.Invoke();
            }
            catch (DbUpdateException)
            {
                ThongBao = "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu mới.";
            }
            catch (Exception)
            {
                ThongBao = "Có lỗi xảy ra khi đổi mật khẩu.";
            }
        }

        private bool KiemTraDuLieu()
        {
            if (string.IsNullOrWhiteSpace(MatKhauCu))
            {
                ThongBao = "Vui lòng nhập mật khẩu hiện tại.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(MatKhauMoi))
            {
                ThongBao = "Vui lòng nhập mật khẩu mới.";
                return false;
            }

            if (MatKhauMoi.Length < 6)
            {
                ThongBao = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return false;
            }

            if (!MatKhauMoi.Any(char.IsUpper))
            {
                ThongBao = "Mật khẩu mới phải có ít nhất 1 chữ hoa.";
                return false;
            }

            if (!MatKhauMoi.Any(char.IsLower))
            {
                ThongBao = "Mật khẩu mới phải có ít nhất 1 chữ thường.";
                return false;
            }

            if (!MatKhauMoi.Any(char.IsDigit))
            {
                ThongBao = "Mật khẩu mới phải có ít nhất 1 chữ số.";
                return false;
            }

            if (!MatKhauMoi.Any(LaKyTuDacBiet))
            {
                ThongBao = "Mật khẩu mới phải có ít nhất 1 ký tự đặc biệt.";
                return false;
            }

            if (MatKhauMoi == MatKhauCu)
            {
                ThongBao = "Mật khẩu mới không được trùng mật khẩu hiện tại.";
                return false;
            }

            if (MatKhauMoi != XacNhanMatKhau)
            {
                ThongBao = "Xác nhận mật khẩu mới không khớp.";
                return false;
            }

            ThongBao = "";
            return true;
        }

        private bool LaKyTuDacBiet(char kyTu)
        {
            return !char.IsLetterOrDigit(kyTu);
        }

        private void GhiLogDoiMatKhau(
            QUANLI_KHOHANGEntities db,
            string maTK)
        {
            db.NHATKies.Add(new NHATKY
            {
                MATK = maTK,
                HANHDONG = "Đổi mật khẩu",
                DOITUONG = maTK,
                THOIGIAN = DateTime.Now,
                TRANGTHAI = "Thành công",
                GHICHU = "Người dùng tự đổi mật khẩu"
            });
        }

        private void Huy()
        {
            _thoat?.Invoke();
        }
    }
}