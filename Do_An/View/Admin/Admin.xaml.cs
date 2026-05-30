using Do_An.ViewModels.Admin;
using System.Windows;

namespace Do_An.View.Admin
{
    public partial class Admin : Window
    {
        public Admin()
        {
            InitializeComponent();
            DataContext = new AdminViewModel();
        }
    }
}