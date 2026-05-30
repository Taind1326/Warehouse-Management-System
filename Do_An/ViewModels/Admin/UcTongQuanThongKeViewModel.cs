using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace Do_An.ViewModels.Admin
{
    public class UcTongQuanThongKeViewModel : BaseViewModel
    {
        private readonly Action _veTrangChu;

        public string TongNhap { get; set; }
        public string TongXuat { get; set; }
        public string GiaTriTon { get; set; }
        public string TonThap { get; set; }
        public string TongSoLuongTon { get; set; }

        public ObservableCollection<ThongKeBieuDoItem> DuLieuBieuDo { get; set; }
        public ObservableCollection<ThongKeBangItem> BangSoLieu { get; set; }
        public ObservableCollection<CanhBaoTonThapItem> CanhBaoTonThap { get; set; }

        public ICommand ThoatCommand { get; }

        public UcTongQuanThongKeViewModel(Action veTrangChu)
        {
            _veTrangChu = veTrangChu;

            DuLieuBieuDo = new ObservableCollection<ThongKeBieuDoItem>();
            BangSoLieu = new ObservableCollection<ThongKeBangItem>();
            CanhBaoTonThap = new ObservableCollection<CanhBaoTonThapItem>();

            ThoatCommand = new RelayCommand(_ => VeTrangChu());

            LoadThongKe();
        }

        public void LoadThongKe()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                decimal tongNhap = db.PHIEUNHAPs
                    .Where(x => x.TRANGTHAI == "Đã nhập")
                    .Select(x => x.TONGTIEN)
                    .DefaultIfEmpty(0)
                    .Sum();

                decimal tongXuat = db.PHIEUXUATs
                    .Where(x => x.TRANGTHAI == "Đã xuất")
                    .Select(x => x.TONGTIEN)
                    .DefaultIfEmpty(0)
                    .Sum();

                var tonKho = db.TONKHOes.ToList();

                decimal giaTriTon = tonKho.Sum(x =>
                    x.SOLUONGTON * (x.SANPHAM?.DONGIA ?? 0));

                int tongSoLuongTon = tonKho.Sum(x => x.SOLUONGTON);

                int tonThap = tonKho.Count(x =>
                    x.SOLUONGTON <= (x.SANPHAM?.MUCTONTOITHIEU ?? 0));

                int soPhieuNhap = db.PHIEUNHAPs.Count();
                int soPhieuXuat = db.PHIEUXUATs.Count();
                int soKho = db.KHOes.Count();
                int soSanPham = db.SANPHAMs.Count();

                TongNhap = DinhDangSoNgan(tongNhap);
                TongXuat = DinhDangSoNgan(tongXuat);
                GiaTriTon = DinhDangSoNgan(giaTriTon);
                TonThap = tonThap.ToString();
                TongSoLuongTon = tongSoLuongTon.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));

                TaoDuLieuBieuDo(tongNhap, tongXuat, giaTriTon);
                TaoBangSoLieu(soPhieuNhap, soPhieuXuat, soKho, soSanPham, tongSoLuongTon, tonThap, tongNhap, tongXuat, giaTriTon);
                TaoCanhBaoTonThap(tonKho);
            }

            BaoThayDoi();
        }

        private void TaoDuLieuBieuDo(decimal tongNhap, decimal tongXuat, decimal giaTriTon)
        {
            decimal max = new[] { tongNhap, tongXuat, giaTriTon }.Max();

            if (max <= 0)
                max = 1;

            DuLieuBieuDo = new ObservableCollection<ThongKeBieuDoItem>
            {
                TaoCot("Nhập", tongNhap, max),
                TaoCot("Xuất", tongXuat, max),
                TaoCot("Tồn", giaTriTon, max)
            };
        }

        private ThongKeBieuDoItem TaoCot(string noiDung, decimal giaTri, decimal max)
        {
            double chieuCao = 240 * (double)(giaTri / max);

            if (giaTri > 0 && chieuCao < 18)
                chieuCao = 18;

            return new ThongKeBieuDoItem
            {
                NoiDung = noiDung,
                GiaTri = DinhDangSoNgan(giaTri),
                ChieuCaoCot = chieuCao
            };
        }

        private void TaoBangSoLieu(
            int soPhieuNhap,
            int soPhieuXuat,
            int soKho,
            int soSanPham,
            int tongSoLuongTon,
            int tonThap,
            decimal tongNhap,
            decimal tongXuat,
            decimal giaTriTon)
        {
            BangSoLieu = new ObservableCollection<ThongKeBangItem>
            {
                new ThongKeBangItem
                {
                    NoiDung = "Phiếu nhập",
                    SoLuong = soPhieuNhap.ToString(),
                    GiaTri = DinhDangTien(tongNhap),
                    GhiChu = "Tổng giá trị nhập kho"
                },
                new ThongKeBangItem
                {
                    NoiDung = "Phiếu xuất",
                    SoLuong = soPhieuXuat.ToString(),
                    GiaTri = DinhDangTien(tongXuat),
                    GhiChu = "Tổng giá trị xuất kho"
                },
                new ThongKeBangItem
                {
                    NoiDung = "Kho hàng",
                    SoLuong = soKho.ToString(),
                    GiaTri = "-",
                    GhiChu = "Số kho đang quản lý"
                },
                new ThongKeBangItem
                {
                    NoiDung = "Sản phẩm",
                    SoLuong = soSanPham.ToString(),
                    GiaTri = "-",
                    GhiChu = "Mặt hàng đang quản lý"
                },
                new ThongKeBangItem
                {
                    NoiDung = "Tồn kho",
                    SoLuong = tongSoLuongTon.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")),
                    GiaTri = DinhDangTien(giaTriTon),
                    GhiChu = "Tổng số lượng tồn"
                },
                new ThongKeBangItem
                {
                    NoiDung = "Tồn thấp",
                    SoLuong = tonThap.ToString(),
                    GiaTri = "-",
                    GhiChu = "Dưới mức tối thiểu"
                }
            };
        }

        private void TaoCanhBaoTonThap(System.Collections.Generic.List<TONKHO> tonKho)
        {
            var ds = tonKho
                .Where(x => x.SOLUONGTON <= (x.SANPHAM?.MUCTONTOITHIEU ?? 0))
                .OrderBy(x => x.SOLUONGTON)
                .Take(5)
                .Select(x => new CanhBaoTonThapItem
                {
                    TenSanPham = x.SANPHAM?.TENSP ?? x.MASP,
                    TonHienTai = x.SOLUONGTON + " / " + (x.SANPHAM?.MUCTONTOITHIEU ?? 0)
                })
                .ToList();

            CanhBaoTonThap = new ObservableCollection<CanhBaoTonThapItem>(ds);

            if (CanhBaoTonThap.Count == 0)
            {
                CanhBaoTonThap.Add(new CanhBaoTonThapItem
                {
                    TenSanPham = "Không có sản phẩm tồn thấp",
                    TonHienTai = "Ổn định"
                });
            }
        }

        private string DinhDangTien(decimal value)
        {
            return value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
        }

        private string DinhDangSoNgan(decimal value)
        {
            if (value >= 1000000000)
                return (value / 1000000000).ToString("0.#") + " tỷ";

            if (value >= 1000000)
                return (value / 1000000).ToString("0.#") + " tr";

            if (value >= 1000)
                return (value / 1000).ToString("0.#") + "k";

            return value.ToString("0");
        }

        private void VeTrangChu()
        {
            _veTrangChu?.Invoke();
        }

        private void BaoThayDoi()
        {
            OnPropertyChanged(nameof(TongNhap));
            OnPropertyChanged(nameof(TongXuat));
            OnPropertyChanged(nameof(GiaTriTon));
            OnPropertyChanged(nameof(TonThap));
            OnPropertyChanged(nameof(TongSoLuongTon));
            OnPropertyChanged(nameof(DuLieuBieuDo));
            OnPropertyChanged(nameof(BangSoLieu));
            OnPropertyChanged(nameof(CanhBaoTonThap));
        }
    }

    public class ThongKeBieuDoItem
    {
        public string NoiDung { get; set; }
        public string GiaTri { get; set; }
        public double ChieuCaoCot { get; set; }
    }

    public class ThongKeBangItem
    {
        public string NoiDung { get; set; }
        public string SoLuong { get; set; }
        public string GiaTri { get; set; }
        public string GhiChu { get; set; }
    }

    public class CanhBaoTonThapItem
    {
        public string TenSanPham { get; set; }
        public string TonHienTai { get; set; }
    }
}