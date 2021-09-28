using Newtonsoft.Json;

namespace VoiceVoxPlugin.Data
{
    public class Mora
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("consonant")]
        public string Consonant { get; set; }

        [JsonProperty("consonant_length")]
        public double? ConsonantLength { get; set; }

        [JsonProperty("vowel")]
        public string Vowel { get; set; }

        [JsonProperty("vowel_length")]
        public double VowelLength { get; set; }

        [JsonProperty("pitch")]
        public double Pitch { get; set; }
    }
}