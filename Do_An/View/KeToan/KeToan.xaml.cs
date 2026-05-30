using Do_An.ViewModels.KeToan;
using System.Windows;

namespace Do_An.View.KeToan
{
    public partial class KeToan : Window
    {
        public KeToan()
        {
            InitializeComponent();
            DataContext = new KeToanViewModel();
        }
    }
}