using Newtonsoft.Json;

namespace VoiceVoxPlugin.Data
{
    public class Speaker
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("speaker_uuid")]
        public string Speaker_uuId { get; set; }

        [JsonProperty("styles")]
        public VoiceVoxSpeakerStyle[] Styles { get; set; }
    }
    public class VoiceVoxSpeakerStyle
    {
        [JsonProperty("name")]
        public string StyleName { get; set; }

        [JsonProperty("id")]
        public int SpeakerId { get; set; }
    }
}