using VoiceVoxPlugin.Core;

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
    }
}