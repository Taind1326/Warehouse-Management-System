using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Do_An.ViewModels.Windows;

namespace Do_An.View.Windows
{
    public partial class Login : Window
    {
        private bool _isShowingPassword;

        public Login()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = txtPassword.Password;
            }
        }

        private void txtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = txtPasswordVisible.Text;
            }
        }

        private void btnShowPassword_MouseLeftButtonDown( object sender, MouseButtonEventArgs e)
        {
            _isShowingPassword = !_isShowingPassword;

            if (_isShowingPassword)
            {
                txtPasswordVisible.Text = txtPassword.Password;

                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;

                txtEye.Text = "\uE890";
            }

            else
            {
                txtPassword.Password = txtPasswordVisible.Text;

                txtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;

                txtEye.Text = "\uE722";
            }
        }

        private void LoginInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (DataContext is LoginViewModel vm)
            {
                vm.LoginCommand.Execute(this);
            }
        }
    }
}