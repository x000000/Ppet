using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Ppet
{
    public partial class AddressWindow : INotifyPropertyChanged
    {
        private string? address;
        public string? Address
        {
            get => address;
            set {
                address = value;
                NotifyPropertyChanged();
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        #endregion

        public AddressWindow() => InitializeComponent();

        protected override void OnActivated(EventArgs ev)
        {
            base.OnActivated(ev);
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        private void Ok_OnClick(object sender, RoutedEventArgs ev) => DialogResult = true;

        private void Cancel_OnClick(object sender, RoutedEventArgs ev) => DialogResult = false;
    }
}