using Newtonsoft.Json;

namespace VoiceVoxPlugin.Data
{
    public class Speaker
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("speaker_id")]
        public int SpeakerId { get; set; }
    }
}