using Do_An.ViewModels.Shared;
using System.Windows.Controls;

namespace Do_An.View.Shared
{
    public partial class UcDoiMatKhau : UserControl
    {
        public UcDoiMatKhau()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!(DataContext is UcDoiMatKhauViewModel vm))
                return;

            var passwordBox = sender as PasswordBox;

            if (passwordBox == null)
                return;

            string tenTruong = passwordBox.Tag?.ToString();

            if (tenTruong == "MatKhauCu")
                vm.MatKhauCu = passwordBox.Password;
            else if (tenTruong == "MatKhauMoi")
                vm.MatKhauMoi = passwordBox.Password;
            else if (tenTruong == "XacNhanMatKhau")
                vm.XacNhanMatKhau = passwordBox.Password;
        }
    }
}