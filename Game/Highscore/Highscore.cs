using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Conesoft.Game.Highscore
{
    interface IHighscores
    {
        void Get(Action<Highscore[]> callback);
        void Add(Highscore highscore);
    }

    public class Highscore
    {
        public string Name { get; set; }
        public string Seconds { get; set; }
    }

    public class Highscores : IHighscores
    {
        private string ApiKey = "1c8aabf0-bdb0-4554-83de-6463f59d7d4e";

        public void Add(Highscore entry)
        {
            var message = "Name=" + entry.Name + "&Seconds=" + entry.Seconds + "&ApiKey=" + ApiKey;
            var webClient = new WebClient();
            webClient.Headers["Content-type"] = "application/x-www-form-urlencoded";
            webClient.UploadStringAsync(new Uri("https://faroff.azurewebsites.net/v0.9"), message);
        }

        public void Get(Action<Highscore[]> callback)
        {
            var webClient = new WebClient();
            webClient.DownloadStringCompleted += webClient_LoadHighscoresCompleted;
            webClient.DownloadStringAsync(new Uri("https://faroff.azurewebsites.net/v0.9"), callback);
        }

        void webClient_LoadHighscoresCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled == false && e.Error == null)
            {
                var result = (string)e.Result;
                var callback = (Action<Highscore[]>)e.UserState;
                callback(JsonConvert.DeserializeObject<Highscore[]>(result));
            }
        }
    }
}
