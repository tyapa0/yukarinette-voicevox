using System.Collections.Generic;
using Newtonsoft.Json;

namespace VoiceVoxPlugin.Data
{
    public class QueryResult
    {
        [JsonProperty("accent_phrases")]
        public IEnumerable<AccentPhrase> AccentPhrases { get; set; }

        [JsonProperty("speedScale")]
        public decimal SpeedScale { get; set; }

        [JsonProperty("pitchScale")]
        public decimal PitchScale { get; set; }

        [JsonProperty("intonationScale")]
        public decimal IntonationScale { get; set; }

        [JsonProperty("volumeScale")]
        public decimal VolumeScale { get; set; }

        [JsonProperty("prePhonemeLength")]
        public decimal PrePhonemeLength { get; set; }

        [JsonProperty("postPhonemeLength")]
        public decimal PostPhonemeLength { get; set; }

        [JsonProperty("outputSamplingRate")]
        public int OutputSamplingRate { get; set; }

        [JsonProperty("outputStereo")]
        public bool OutputStereo { get; set; }
    }
}