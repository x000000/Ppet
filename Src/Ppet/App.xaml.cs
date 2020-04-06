using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using static System.Diagnostics.Debug;
using static Ppet.WinApi;

namespace Ppet
{
    public partial class App
    {
        private ApplicationDataContext dataContext = null!;
        private AddressWindow? addressWindow;
        private TaskbarIcon trayIcon = null!;

        private void App_OnStartup(object sender, StartupEventArgs ev)
        {
            dataContext = (ApplicationDataContext) FindResource("DataContext");
            Assert(dataContext != null, nameof(dataContext) + " != null");

#if DEBUG
            if (ev.Args.FirstOrDefault() == "serve") {
                dataContext.AcceptControl = true;
                return;
            }
#endif
            try {
                foreach (var arg in ev.Args) {
                    if (arg.StartsWith("--port=")) {
                        dataContext.ListenPort = int.Parse(arg.Substring(7));
                        continue;
                    }
                    throw new ArgumentException("Invalid startup argument: " + arg);
                }
            } catch (Exception e) {
                Console.Error.WriteLine(e.Message);
                Shutdown(-1);
                return;
            }

            dataContext.ListenKeyboard();
            trayIcon = (TaskbarIcon) FindResource("TrayIcon");
        }

        private void ShowLogs_OnClick(object sender, RoutedEventArgs ev) => new DebugWindow().Show();

        private void ConnectTo_OnClick(object sender, RoutedEventArgs ev)
        {
            if (addressWindow == null) {
                addressWindow = new AddressWindow { Address = dataContext.RemoteAddress };
                if (addressWindow.ShowDialog() == true) {
                    dataContext.RemoteAddress = addressWindow.Address;
                }
                addressWindow = null;
            } else {
                addressWindow.Activate();
            }
        }

        private void Exit_OnClick(object sender, RoutedEventArgs ev) => Shutdown();

        public void ShowNotification(string message)
            => trayIcon.ShowBalloonTip("Ppet", message, BalloonIcon.None);
    }

    public class ApplicationDataContext : INotifyPropertyChanged
    {
        private bool acceptControl;
        public bool AcceptControl
        {
            get => acceptControl;
            set {
                if (Set(ref acceptControl, value)) {
                    if (value) {
                        inputListener.Start(ListenPort);
                    } else {
                        inputListener.Stop();
                    }
                }
            }
        }

        private string remoteAddress = "192.168.1.63";
        public string RemoteAddress
        {
            get => remoteAddress;
            set {
                Set(ref remoteAddress, value);
                if (!string.IsNullOrEmpty(value)) {
                    if (inputClient.IsConnected) {
                        ToggleClient(false);
                    }
                    ToggleClient(true);
                }
            }
        }

        private bool isConnected;
        public bool IsConnected
        {
            get => isConnected;
            set {
                if (isConnected != value) {
                    ToggleClient(value);
                }
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private bool Set<T>(ref T oldVal, T newVal, [CallerMemberName] string? prop = null)
        {
            if (!Equals(oldVal, newVal)) {
                oldVal = newVal;
                NotifyPropertyChanged(prop);
                return true;
            }
            return false;
        }

        #endregion

        public int ListenPort { get; set; } = InputListener.DefaultPort;

        private readonly MouseHook msHook = new MouseHook();
        private readonly KeyboardHook kbHook = new KeyboardHook();
        private readonly InputListener inputListener = new InputListener();
        private readonly InputClient inputClient;

        public ApplicationDataContext()
        {
            inputClient = new InputClient(msHook, kbHook);
            inputClient.Connected += (o, ev) => {
                msHook.Start();
                Set(ref isConnected, true, nameof(IsConnected));
            };
            inputClient.Disconnected += (o, ev) => {
                msHook.Stop();
                Set(ref isConnected, false, nameof(IsConnected));
            };
        }

        public void ListenKeyboard()
        {
            kbHook.KeyUp += OnKeyUp;
            kbHook.Start();
        }

        private bool OnKeyUp(object sender, KeyEventArgs ev)
        {
            // Ctrl + ScrollLock
            if (ev.Key == VirtualKey.Cancel) {
                ToggleClient();
                return false;
            }
            return true;
        }

        private void ToggleClient() => ToggleClient(!inputClient.IsConnected);

        private void ToggleClient(bool value)
        {
            if (value) {
                inputClient.Connect(RemoteAddress);
            } else {
                inputClient.Disconnect();
            }
        }
    }
}
