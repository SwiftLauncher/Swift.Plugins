using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Swift.WebUtilities.LinkHelper
{
    public class Session
    {
        public int Id { get; }
        public string Name { get; }
        public IEnumerable<Site> Sites { get; set; } = new List<Site>();

        public Session(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class Site : INotifyPropertyChanged
    {
        public string Url { get; }

        public string FavIconUrl { get; }

        public string Title { get; private set; }

        public Site(string url)
        {
            Url = url.StartsWith("http") ? url : "http://" + url;
            FavIconUrl = $"http://www.google.com/s2/favicons?domain_url={Uri.EscapeDataString(Url)}";
            Title = Url;
            ReadTitle();
        }

        private async Task ReadTitle()
        {
            var t = await Task.Factory.StartNew(() =>
            {
                try
                {
                    WebClient x = new WebClient();
                    string source = x.DownloadString(Url);
                    x.Dispose();
                    return
                        Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                            RegexOptions.IgnoreCase)
                            .Groups["Title"].Value;
                }
                catch (WebException)
                {
                    return Url;
                }
            });
            Title = WebUtility.HtmlDecode(t).Replace("\r", "").Replace("\n", "");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
