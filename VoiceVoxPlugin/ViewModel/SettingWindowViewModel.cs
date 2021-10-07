using System.Collections.Generic;
using System.Collections.ObjectModel;
using VoiceVoxPlugin.Core;
using VoiceVoxPlugin.Data;

namespace VoiceVoxPlugin.ViewModel
{
    public class SettingWindowViewModel : Element
    {
        private static SettingWindowViewModel _instance;
        public static SettingWindowViewModel Instance => _instance ?? (_instance = new SettingWindowViewModel());
        public static void Clear()
        {
            _instance = null;
        }

        public ObservableCollection<Speaker> Speakers { get; } = new ObservableCollection<Speaker>();
        public int SpeakerId { get; set; }

        private decimal _speedScale = 1.0m;

        public decimal SpeedScale
        {
            get => _speedScale;
            set
            {
                if (value < 0.5m)
                {
                    value = 0.5m;
                }

                if (2.0m < value)
                {
                    value = 2.0m;
                }

                _speedScale = value;
            }
        }

        private decimal _pitchScale = 0.0m;
        public decimal PitchScale
        {
            get => _pitchScale;
            set
            {
                if (value < -0.15m)
                {
                    value = -0.15m;
                }

                if (0.15m < value)
                {
                    value = 0.15m;
                }

                _pitchScale = value;
            }
        }

        public decimal IntonationScale { get; set; } = 1.0m;

        public decimal VolumeScale { get; set; } = 1.0m;

        public int SpeedScaleForSlider
        {
            get => (int)(SpeedScale * 100m);
            set => SpeedScale = value / 100m;
        }

        public int PitchScaleForSlider
        {
            get => (int)(PitchScale * 100m);
            set => PitchScale = value / 100m;
        }

        public int IntonationScaleForSlider
        {
            get => (int)(IntonationScale * 100m);
            set => IntonationScale = value / 100m;
        }

        public int VolumeScaleForSlider
        {
            get => (int)(VolumeScale * 100m);
            set => VolumeScale = value / 100m;
        }
    }
}