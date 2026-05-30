using Do_An.Helper;
using Do_An.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Do_An.ViewModels.NhanVienKho
{
    public class UcKhoViewModel : BaseViewModel
    {
        private readonly Action _veTrangChu;

        private ObservableCollection<KhoItem> _danhSachKho;
        public ObservableCollection<KhoItem> DanhSachKho
        {
            get => _danhSachKho;
            set
            {
                _danhSachKho = value;
                OnPropertyChanged();
            }
        }

        private KhoItem _selectedItem;
        public KhoItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
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
                TimKiemKho();
            }
        }

        public ICommand ThoatCommand { get; }

        public UcKhoViewModel(Action veTrangChu)
        {
            _veTrangChu = veTrangChu;

            ThoatCommand = new RelayCommand(_ => VeTrangChu());

            LoadKho();
        }

        public void LoadKho()
        {
            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.KHOes
                    .ToList()
                    .Select((kho, index) => TaoKhoItem(kho, index))
                    .ToList();

                DanhSachKho = new ObservableCollection<KhoItem>(ds);
            }
        }

        private KhoItem TaoKhoItem(KHO kho, int index)
        {
            return new KhoItem
            {
                STT = index + 1,
                MaKho = kho.MAKHO,
                TenKho = kho.TENKHO,
                DiaChi = kho.DIADIEM,
                Hotline = kho.SDT
            };
        }

        private void TimKiemKho()
        {
            string tuKhoa = SearchText?
                .Trim()
                .ToLower();

            using (var db = new QUANLI_KHOHANGEntities())
            {
                var ds = db.KHOes
                    .ToList()
                    .Where(kho => KiemTraDungTuKhoa(kho, tuKhoa))
                    .Select((kho, index) => TaoKhoItem(kho, index))
                    .ToList();

                DanhSachKho = new ObservableCollection<KhoItem>(ds);
            }
        }

        private bool KiemTraDungTuKhoa(KHO kho, string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
                return true;

            return
                kho.MAKHO.ToLower().Contains(tuKhoa) ||
                kho.TENKHO.ToLower().Contains(tuKhoa) ||
                (!string.IsNullOrWhiteSpace(kho.DIADIEM) &&
                 kho.DIADIEM.ToLower().Contains(tuKhoa)) ||
                (!string.IsNullOrWhiteSpace(kho.SDT) &&
                 kho.SDT.ToLower().Contains(tuKhoa));
        }

        private void VeTrangChu()
        {
            _veTrangChu();
        }
    }

    public class KhoItem
    {
        public int STT { get; set; }
        public string MaKho { get; set; }
        public string TenKho { get; set; }
        public string DiaChi { get; set; }
        public string Hotline { get; set; }
    }
}