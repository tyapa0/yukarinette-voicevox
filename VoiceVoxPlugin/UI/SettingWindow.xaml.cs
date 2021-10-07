using VoiceVoxPlugin.ViewModel;

namespace VoiceVoxPlugin.UI
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow 
    {
        public SettingWindow()
        {
            InitializeComponent();
            DataContext = SettingWindowViewModel.Instance;
        }
    }
}
