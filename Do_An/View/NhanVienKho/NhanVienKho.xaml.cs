using System.Windows;
using Do_An.ViewModels.NhanVienKho;

namespace Do_An.View.NhanVienKho
{
    public partial class NhanVienKho : Window
    {
        public NhanVienKho()
        {
            InitializeComponent();
            DataContext = new NhanVienKhoViewModel();
        }
    }
}