using Newtonsoft.Json;
using System;
using System.Net;
using web.Models;

namespace web.Services
{
    public class SlackService : ISlackService
    {
        public SlackService(string url)
        {
            Url = url;
        }

        private string Url { get; }

        public void PostMessage(string text, string username = null, string channel = null)
        {
            SlackMessageModel message = new SlackMessageModel()
            {
                Channel = channel,
                Username = username,
                Text = text
            };

            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                var response = client.UploadString(new Uri(Url), "POST" , JsonConvert.SerializeObject(message));
            }
        }
    }
}