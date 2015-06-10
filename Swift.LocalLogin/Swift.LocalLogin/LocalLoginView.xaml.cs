using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Swift.Extensibility.Plugins;
using Swift.Extensibility.Services;
using Swift.Extensibility.Services.Profile;

namespace Swift.LocalLogin
{
    /// <summary>
    /// Interaction logic for LocalLoginView.xaml
    /// </summary>
    public partial class LocalLoginView : IProfileProvider, IPlugin
    {
        public string Username { get; set; }

        public string Password { get; set; }

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
                UserProfile = new UserProfile(Username, new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(Password)));
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

        public string Description { get; } = "Provides a way to locally log into Swift.";

        public Uri Icon { get; } = new Uri("pack://application:,,,/Swift.LocalLogin;component/Logo_lightning_bg.png");

        public bool IsLoggedIn { get; private set; }

        public object LoginView { get; }

        public string ServiceName { get; } = "Swift-Local Login";

        public UserProfile UserProfile { get; private set; }

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
                    Dispatcher.Invoke(() => PwBox.Focus());
                    return;
                }
            }
            Dispatcher.Invoke(() => UnBox.Focus());
        }

        [Import]
        private IPluginServices _pluginServices;
        private readonly List<Tuple<string, string>> _knownUsers = new List<Tuple<string, string>>();
        private const string RememberedAccountSettingsKey = "RememberedAccount";

        public LocalLoginView()
        {
            LoginView = this;
            InitializeComponent();
        }

        public int InitializationPriority { get; } = 0;
        public void OnInitialization(InitializationEventArgs args)
        {
            // TODO load users from storage
            _knownUsers.Add(Tuple.Create("tim", "asd"));
        }

        public int ShutdownPriority { get; } = 0;
        public void OnShutdown(ShutdownEventArgs args) { }
    }
}
