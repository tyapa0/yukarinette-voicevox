using System.Collections;
using System.Collections.Generic;
using VoiceVoxPlugin.Core;
using VoiceVoxPlugin.Data;

namespace VoiceVoxPlugin.ViewModel
{
    public class OptionWindowViewModel : Element
    {
        public string ExePath { get; set; }
        public bool ExitWhenFinished { get; set; }

        private int _voiceVoxTimeout = 120;

        public int VoiceVoxTimeout
        {
            get => _voiceVoxTimeout;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

                _voiceVoxTimeout = value;
            }
        }

        public IEnumerable<SoundDevice> SoundDevices { get; set; }
        public string SoundDeviceId { get; set; }
    }
}