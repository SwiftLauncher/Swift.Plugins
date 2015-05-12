using Swift.Extensibility.Services;
using Swift.Extensibility.Services.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Swift.LocalLogin
{
    /// <summary>
    /// Interaction logic for LocalLoginView.xaml
    /// </summary>
    public partial class LocalLoginView : UserControl, IProfileProvider
    {
        #region Properties

        public string Username { get; set; }

        public string Password { get; set; }

        #endregion

        #region Handlers

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Username = UnBox.Text;
            BtLogin.IsEnabled = !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = PwBox.Password;
            BtLogin.IsEnabled = !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private void BtLogin_Click(object sender, RoutedEventArgs e)
        {
            if (_knownUsers.Any(_ => _.Item1 == Username && _.Item2 == Password))
            {
                UserProfile = new UserProfile(Username, Convert.ToBase64String(new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(Password))));
                IsLoggedIn = true;
                LoginCompleted?.Invoke(true);
            }
            else
            {
                TbRetry.Visibility = Visibility.Visible;
                PwBox.Password = "";
            }
        }

        #endregion

        #region IProfileProvider Implementation

        public string Description { get; } = "Provides a way to locally log into Swift.";

        public Uri Icon { get; } = new Uri("pack://application:,,,/Swift.LocalLogin;component/Logo_lightning_bg.png");

        public bool IsLoggedIn { get; private set; } = false;

        public object LoginView { get; } = null;

        public string ServiceName { get; } = "Swift-Local Login";

        public IUserProfile UserProfile { get; private set; } = null;

        public event Action<bool> LoginCompleted;

        public void LoginAsync()
        {

        }

        #endregion

        private List<Tuple<string, string>> _knownUsers = new List<Tuple<string, string>>();

        public LocalLoginView()
        {
            LoginView = this;
            InitializeComponent();
            // TODO load users from storage
            _knownUsers.Add(Tuple.Create("tim", "asd"));
        }
    }
}
