using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Swift.Extensibility;
using Swift.Extensibility.Input;
using Swift.Extensibility.Services;
using Swift.Extensibility.Services.Settings;
using Swift.Extensibility.UI;

namespace Swift.WebUtilities.LinkHelper
{
    public class SessionDataItemProvider : IDataItemSource, ISettingsSource, IInitializationAware, ISwiftExtender
    {
        public static SessionDataItemProvider Instance { get; private set; }

        private static int SiteLaunchDelay { get; set; } = 4;

        public IList<Session> Sessions { get; } = new List<Session>();

        public SessionDataItemProvider()
        {
            Instance = this;
            Settings = new List<ISetting>
            {
                new Header("General") {ToolTip="General Settings",Description="General settings for WebUtilities LinkHelper" },
                new IntegerSetting("Delay between site launches", 200, _ => { SiteLaunchDelay = _; }) { MinValue = 0, MaxValue = 2000, UseSliderView = true, Description = "Values in milliseconds (default = 200)" },
                new IntegerSetting("Delay between site launches", 200, _ => { SiteLaunchDelay = _; }) { MinValue = 0, MaxValue = 2000, UseSliderView = false },
                new ButtonSetting("Test for ButtonSetting",()=>Process.Start("www.xkcd.com")) {ButtonContent="Test Click",Description="Goes to XKCD",ToolTip="Go to XKCD" },
                new LinkButtonSetting("Test for LinkButton without ButtonContent",new Uri("swift-function://dataitems?Daily"))
            };
            Sessions.Add(new Session(0, "Daily Websites") { Sites = new[] { new Site("www.chip.de"), new Site("www.gizmodo.de"), new Site("www.heise.de"), new Site("www.mydealz.de"), new Site("www.computerbase.de"), new Site("www.golem.de"), new Site("www.winfuture.de"), new Site("www.wparea.de"), new Site("www.windowscentral.com"), new Site("www.theverge.com"), new Site("www.wmpoweruser.com"), new Site("www.neowin.net"), new Site("www.winbeta.org") } });
        }

        public IEnumerable<DataItem> GetMatchingItems(string input) => Sessions.Where(_ => _.Name.StartsWith(input) || _.Name.Contains(input))
            .Select(
                _ =>
                    DataItem.Create(this, _.Name, "WebUtilities Session", 0,
                        _, OnSessionItemExecution));

        public async void OnSessionItemExecution(DataItem item)
        {
            foreach (var site in Sessions.First(_ => _.Id == item.Id).Sites)
            {
                Process.Start(site.Url);
                await Task.Delay(SiteLaunchDelay);
            }
        }

        public string DisplayName => "Sessions";
        public ISettingsSource Parent => null;
        public IEnumerable<ISetting> Settings { get; }

        [Import]
        private IPluginServices _pluginServices;

        public void OnInitialization(InitializationEventArgs args)
        {
            _pluginServices.GetService<IUiService>().AddUiResource(new Uri("pack://application:,,,/Swift.WebUtilities;component/LinkHelper/SessionTemplates.xaml", UriKind.Absolute));
        }

        public int InitializationPriority => 0;
    }
}
