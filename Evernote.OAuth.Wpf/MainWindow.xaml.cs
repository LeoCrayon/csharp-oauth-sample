using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Codeplex.OAuth;

namespace Evernote.OAuth.Wpf {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (sender, args) => {
                wbLogin.LoadCompleted += OnWbLoginOnLoadCompleted;
                wbLogin.Navigating += WbLoginOnNavigating;
            };
            DataContext = this;
        }

        private const string ConsumerKey = "your consumer key";
        private const string ConsumerSecret = "your consumer secret";
        private const string BaseUrl = "https://sandbox.evernote.com";
        RequestToken requestToken;


        private void OnWbLoginOnLoadCompleted(object sender, NavigationEventArgs args)
        {
            if (!args.Uri.ToString().Equals("https://sandbox.evernote.com/")) {
                ((Storyboard)Resources["FirstLoadAnimation"]).Begin();
                LoginText = "Login";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginText = "Please wait...";
            var authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);
            authorizer.GetRequestToken(BaseUrl + "/oauth", new Codeplex.OAuth.Parameter("oauth_callback", "http://localhost/myapp"))
            .Select(res => res.Token)
                //.ObserveOnDispatcher()
            .Subscribe(token => {
                requestToken = token;
                var url = authorizer.BuildAuthorizeUrl(BaseUrl + "/OAuth.action", token);
                Dispatcher.BeginInvoke(new Action(() => wbLogin.Navigate(new Uri(url))));
            });
        }

        private void WbLoginOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("http://localhost/myapp")) {
                var splitted = e.Uri.ToString().Split('&').Select(s => s.Split('=')).ToDictionary(s => s.First(), s => s.Last());
                if (!splitted.ContainsKey("oauth_verifier")) {
                    ((Storyboard)Resources["HidePopupAnimation"]).Begin();
                    return;
                }
                string verifier = splitted["oauth_verifier"];
                var authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);
                authorizer.GetAccessToken(BaseUrl + "/oauth", requestToken, verifier)
                    //.ObserveOnDispatcher()
                    .Subscribe(res => {
                        try {
                            AuthenticationResult result = new AuthenticationResult();
                            result.AuthenticationToken = Uri.UnescapeDataString(res.Token.Key);
                            result.NoteStoreUrl = Uri.UnescapeDataString(res.ExtraData["edam_noteStoreUrl"].First());
                            result.Expiration = long.Parse(res.ExtraData["edam_expires"].First());
                            result.WebApiUrlPrefix = Uri.UnescapeDataString(res.ExtraData["edam_webApiUrlPrefix"].First());
                            result.User = new User();
                            result.User.Id = int.Parse(res.ExtraData["edam_userId"].First());
                            MessageBox.Show("Login Success. Token is " + result.AuthenticationToken);
                        }
                        catch (KeyNotFoundException) {

                        }
                        finally
                        {
                            Dispatcher.BeginInvoke(new Action(() => ((Storyboard)Resources["HidePopupAnimation"]).Begin()));
                        }
                    });

                e.Cancel = true;
                wbLogin.Visibility = Visibility.Collapsed;
            }
        }


        private string _loginText = "Login";
        public string LoginText
        {
            get { return _loginText; }
            set
            {
                _loginText = value;
                RaisePropertyChanged("LoginText");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
