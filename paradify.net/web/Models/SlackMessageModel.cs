
using Newtonsoft.Json;

namespace web.Models
{
    internal class SlackMessageModel
    {
        public SlackMessageModel()
        {
        }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}