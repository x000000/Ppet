using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace Ppet
{
    public class InputClient : IDisposable
    {
        internal const int FlushInterval = 20;

        private static readonly TimeSpan Threshold = TimeSpan.FromMilliseconds(60);
        internal static readonly Dictionary<MouseButton, byte> ButtonToType = new Dictionary<MouseButton, byte> {
            [MouseButton.LeftDown]    = InputMessage.MouseLDown,
            [MouseButton.LeftUp]      = InputMessage.MouseLUp,
            [MouseButton.RightDown]   = InputMessage.MouseRDown,
            [MouseButton.RightUp]     = InputMessage.MouseRUp,
            [MouseButton.MiddleDown]  = InputMessage.MouseMDown,
            [MouseButton.MiddleUp]    = InputMessage.MouseMUp,
        };

        public bool IsConnected => stream != null && client != null;

        public event EventHandler? Connected;
        public event EventHandler? Disconnected;

        private readonly MouseHook msHook;
        private readonly KeyboardHook kbHook;
        private readonly Dictionary<uint, DateTime> strokes = new Dictionary<uint, DateTime>();
        private Timer? flushTimer;

        private TcpClient? client;
        private Stream? stream;
        private WinApi.PointStruct pos;

        public InputClient(MouseHook msHook, KeyboardHook kbHook)
        {
            this.msHook = msHook;
            this.kbHook = kbHook;
        }

        public void Dispose()
        {
            Disconnect();
            stream?.Dispose();
            client?.Dispose();
        }

        protected internal virtual (TcpClient?, Stream) Connect(string address, int port)
        {
            var tcpClient = new TcpClient(address, port);
            return (tcpClient, tcpClient.GetStream());
        }

        public void Connect(string address)
        {
            try {
                var tokens = address.Replace(" ", "").Split(":", 2);
                var (ip, port) = tokens.Length > 1
                    ? (tokens[0], int.Parse(tokens[1]))
                    : (address, InputListener.DefaultPort);

                (client, stream) = Connect(ip, port);
                pos = default;
                flushTimer = new Timer(Flush, true, FlushInterval, FlushInterval);

                kbHook.KeyDown       += OnKeyDown;
                kbHook.KeyUp         += OnKeyUp;
                msHook.MouseActivity += OnMouseActivity;
                Connected?.Invoke(this, EventArgs.Empty);
            } catch (Exception e) {
                Debug.WriteLine("Failed to connect to '{0}': {1}", address, e);
                Disconnect();
            }
        }

        public void Disconnect()
        {
            flushTimer?.Dispose();
            kbHook.KeyDown       -= OnKeyDown;
            kbHook.KeyUp         -= OnKeyUp;
            msHook.MouseActivity -= OnMouseActivity;

            try {
                stream?.Close();
                stream = null;
            } catch (Exception e) {
                Debug.WriteLine("Failed to close client stream: {0}", e);
            }
            try {
                client?.Close();
                client = null;
            } catch (Exception e) {
                Debug.WriteLine("Failed to close client: {0}", e);
            }

            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private void Flush(object? _)
        {
            lock (this) {
                if (pos.X == 0 && pos.Y == 0) {
                    return;
                }

                Send(new MouseMessage {
                    Type   = InputMessage.MouseWheel,
                    Button = MouseButton.None,
                    X = pos.X,
                    Y = pos.Y,
                });
                pos.X = pos.Y = 0;
            }
        }

        private bool OnKeyDown(object sender, KeyEventArgs ev)
        {
            var key = (ev.ScanCode << 1) | ev.Flags;
            if (!strokes.TryGetValue(key, out var time) || DateTime.Now - time > Threshold) {
                strokes[key] = DateTime.Now;

                Send(new KeyboardMessage {
                    Type     = InputMessage.KeyDown,
                    Key      = ev.Key,
                    Flags    = ev.Flags,
                    ScanCode = ev.ScanCode,
                });
            }
            return false;
        }

        private bool OnKeyUp(object sender, KeyEventArgs ev)
        {
            var key = (ev.ScanCode << 1) | ev.Flags;
            if (!strokes.TryGetValue(key, out var time) || DateTime.Now - time > Threshold) {
                strokes[key] = DateTime.Now;

                Send(new KeyboardMessage {
                    Type     = InputMessage.KeyUp,
                    Key      = ev.Key,
                    Flags    = ev.Flags,
                    ScanCode = ev.ScanCode,
                });
            }
            return false;
        }

        private bool OnMouseActivity(object sender, MouseEventArgs ev)
        {
            var type = ButtonToType.TryGetValue(ev.Button, out var b) ? b : InputMessage.MouseWheel;
            if (type == InputMessage.MouseWheel && ev.WheelDelta == 0) {
                lock (this) {
                    pos.X += ev.X;
                    pos.Y += ev.Y;
                }
                return false;
            }
            Flush(null);

            Send(new MouseMessage {
                Type   = type,
                Button = ev.Button,
                X = ev.X,
                Y = ev.Y,
                WheelDelta = ev.WheelDelta
            });
            return false;
        }

        private void Send<T>(T message) where T : struct
        {
            try {
                Send(InputMessage.GetBytes(message));
            } catch (Exception e) {
                Debug.WriteLine("Failed to serialize message: {0}", e);
                ((App) Application.Current)?.ShowNotification("Connection lost: " + e.Message);
                Disconnect();
            }
        }

        private void Send(byte[] data)
        {
            try {
                Debug.WriteLine("<<< {0}", string.Join(' ', data.Select(b => b.ToString("X2"))));
                stream!.Write(data, 0, data.Length);
            } catch (Exception e) {
                Debug.WriteLine("Failed to send data: {0}", e);
                ((App) Application.Current)?.ShowNotification("Connection lost: " + e.Message);
                Disconnect();
            }
        }
    }
}
