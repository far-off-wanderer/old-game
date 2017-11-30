using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;

namespace Conesoft
{
    public partial class HighscoresPage : PhoneApplicationPage
    {
        public HighscoresPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var highscores = TinyIoC.TinyIoCContainer.Current.Resolve<Conesoft.Game.Highscore.IHighscores>();
            highscores.Get(entries =>
            {
                DataContext = new
                {
                    Highscores = from entry in entries
                                 select new
                                 {
                                     Entry = entry.Name + " " + entry.Seconds + " seconds"
                                 }
                };
            });
        }
    }
}