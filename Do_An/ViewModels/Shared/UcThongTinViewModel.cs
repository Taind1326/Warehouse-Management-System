using Do_An.Helper;
using Do_An.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace Do_An.ViewModels.Shared
{
    public class UcThongTinViewModel : BaseViewModel
    {
        private readonly Action _thoat;

        public string MaTaiKhoan { get; set; }
        public string TenTaiKhoan { get; set; }
        public string MaNhanVien { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string VaiTro { get; set; }
        public string Kho { get; set; }

        public ICommand CloseCommand { get; }

        public UcThongTinViewModel(Action thoat = null)
        {
            _thoat = thoat;

            CloseCommand = new RelayCommand(_ => Thoat());

            LoadThongTin();
        }

        private void LoadThongTin()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var tk = db.TAIKHOANs
                    .FirstOrDefault(x => x.MATK == CurrentUser.MaTK);

                if (tk == null)
                {
                    GanThongTinMacDinh();
                    return;
                }

                MaTaiKhoan = tk.MATK?.Trim();
                TenTaiKhoan = tk.TENTK;
                MaNhanVien = tk.MANV?.Trim();
                HoTen = tk.NHANVIEN?.TENNV;
                Email = tk.NHANVIEN?.EMAIL;
                SoDienThoai = tk.NHANVIEN?.SDT;

                VaiTro = tk.VAITROes
                    .Select(x => x.TENVT)
                    .FirstOrDefault() ?? "Chưa phân quyền";

                Kho = tk.PHANCONG_KHO
                    .Where(x => x.TRANGTHAI == true)
                    .Select(x => x.KHO.TENKHO)
                    .FirstOrDefault() ?? "Chưa phân công";
            }

            BaoThayDoiThongTin();
        }

        private void GanThongTinMacDinh()
        {
            MaTaiKhoan = "Không xác định";
            TenTaiKhoan = "Không xác định";
            MaNhanVien = "Không xác định";
            HoTen = "Không xác định";
            Email = "Không xác định";
            SoDienThoai = "Không xác định";
            VaiTro = "Không xác định";
            Kho = "Không xác định";

            BaoThayDoiThongTin();
        }

        private void BaoThayDoiThongTin()
        {
            OnPropertyChanged(nameof(MaTaiKhoan));
            OnPropertyChanged(nameof(TenTaiKhoan));
            OnPropertyChanged(nameof(MaNhanVien));
            OnPropertyChanged(nameof(HoTen));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(SoDienThoai));
            OnPropertyChanged(nameof(VaiTro));
            OnPropertyChanged(nameof(Kho));
        }

        private void Thoat()
        {
            _thoat?.Invoke();
        }
    }
}