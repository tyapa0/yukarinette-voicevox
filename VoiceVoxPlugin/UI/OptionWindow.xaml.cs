using System.Windows;
using Microsoft.Win32;

namespace VoiceVoxPlugin.UI
{
    /// <summary>
    /// OptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionWindow 
    {
        public OptionWindow(bool exitWhenFinished, string exePath)
        {
            InitializeComponent();
            CheckBox.IsChecked = exitWhenFinished;
            TextBox.Text = exePath;
        }

        public bool ExitWhenFinished => CheckBox.IsChecked ?? false;
        public string ExePath => TextBox.Text ?? "";

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "VOICE VOX の実行ファイルの場所を指定してください",
                Filter = "実行ファイル|*.exe",
            };

            if (!(dialog.ShowDialog() ?? false))
            {
                return;
            }

            TextBox.Text = dialog.FileName;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
