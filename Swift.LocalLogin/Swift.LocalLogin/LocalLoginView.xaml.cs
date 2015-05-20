using Swift.Extensibility.Services;
using Swift.Extensibility.Services.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Swift.Extensibility;

namespace Swift.LocalLogin
{
    /// <summary>
    /// Interaction logic for LocalLoginView.xaml
    /// </summary>
    public partial class LocalLoginView : UserControl, IProfileProvider, IPluginServiceUser
    {
        #region Properties

        public string Username { get; set; }

        public string Password { get; set; }

        #endregion

        #region Handlers

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Username = UnBox.Text;
            BtLogin.IsEnabled = CanLogin();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = PwBox.Password;
            BtLogin.IsEnabled = CanLogin();
        }

        private void BtLogin_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private bool CanLogin() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

        private void Login()
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

        private void PwBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CanLogin()) Login();
            if ((bool)CBRememberMe.IsChecked)
            {
                _pluginServices.GetSettingsStore<LocalLoginView>().Store(RememberedAccountSettingsKey, Username);
            }
            else if (_pluginServices.GetSettingsStore<LocalLoginView>().ContainsKey(RememberedAccountSettingsKey))
            {
                _pluginServices.GetSettingsStore<LocalLoginView>().DeleteEntry(RememberedAccountSettingsKey);
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
            if (_pluginServices.GetSettingsStore<LocalLoginView>().ContainsKey(RememberedAccountSettingsKey))
            {
                var user = _pluginServices.GetSettingsStore<LocalLoginView>().Retrieve<string>(RememberedAccountSettingsKey);
                if (!string.IsNullOrWhiteSpace(user))
                {
                    Username = user;
                    UnBox.Text = user;
                }
            }
            Dispatcher.Invoke(() => UnBox.Focus());
        }

        public void SetPluginServices(IPluginServices pluginServices)
        {
            _pluginServices = pluginServices;
        }

        #endregion

        private IPluginServices _pluginServices;
        private List<Tuple<string, string>> _knownUsers = new List<Tuple<string, string>>();
        private const string RememberedAccountSettingsKey = "RememberedAccount";

        public LocalLoginView()
        {
            LoginView = this;
            InitializeComponent();
            // TODO load users from storage
            _knownUsers.Add(Tuple.Create("tim", "asd"));
        }

    }
}
