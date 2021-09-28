using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;
using VoiceVoxPlugin.Annotations;

namespace VoiceVoxPlugin.Core
{
    [AddINotifyPropertyChangedInterface]
    public class Element : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}