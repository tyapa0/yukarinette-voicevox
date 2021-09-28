using System.Collections.Generic;
using Newtonsoft.Json;

namespace VoiceVoxPlugin.Data
{
    public class AccentPhrase
    {
        [JsonProperty("moras")]
        public IEnumerable<Mora> Moras { get; set; }

        [JsonProperty("accent")]
        public int Accent { get; set; }

        [JsonProperty("pause_mora")]
        public object PauseMora { get; set; }
    }
}