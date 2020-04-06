using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using static Ppet.WinApi;
using static Ppet.WinApi.KeyboardFlag;

namespace Ppet
{
    public class InputReader
    {
        public delegate void ReadHandler(byte[] bytes, int startIndex, out int nextStartIndex);

        private readonly Stream stream;
        private readonly ReadHandler handler;
        private readonly int bufferSize;

        public InputReader(Stream stream, ReadHandler handler, int bufferSize = 2048)
        {
            this.stream     = stream;
            this.handler    = handler;
            this.bufferSize = bufferSize;
        }

        public void ReadToEnd()
        {
            int num;
            var rawBytes = new byte[bufferSize];
            var buf = new List<byte>(bufferSize * 2);

            while ((num = stream.Read(rawBytes, 0, rawBytes.Length)) != 0) {
                Debug.WriteLine(">>> [{0}] {1}", num, string.Join(' ', rawBytes.Take(num)));
                buf.AddRange( rawBytes.Take(num) );

                var arr = buf.ToArray();
                var startIndex = 0;
                while (startIndex < arr.Length) {
                    try {
                        handler(arr, startIndex, out var nextStartIndex);
                        startIndex = nextStartIndex;
                    } catch (Exception e) when (e is ArgumentOutOfRangeException || e is IndexOutOfRangeException) {
                        buf.Clear();
                        buf.AddRange( arr.Skip(startIndex) );
                        break;
                    }
                }

                if (startIndex == arr.Length) {
                    buf.Clear();
                }
            }
        }
    }

    public class InputListener
    {
        public const int DefaultPort = 16152;

        private bool isListening;
        private TcpListener? server;

        public void Start(int port)
        {
            if (!isListening) {
                try {
                    isListening = true;
                    server = new TcpListener(IPAddress.Any, port);
                    server.Start();
                    Task.Run(Listen);
                } catch (Exception e) {
                    isListening = false;
                    Debug.WriteLine("Failed to start TcpListener: {0}", e);
                }
            }
        }

        public void Stop()
        {
            if (isListening) {
                try {
                    server?.Stop();
                    server = null;
                } catch (Exception e) {
                    Debug.WriteLine("Failed to stop TcpListener: {0}", e);
                }
                isListening = false;
            }
        }

        private void Listen()
        {
            while (server != null) {
                TcpClient? client = null;

                try {
                    client = server.AcceptTcpClient();
                    Debug.WriteLine("Client accepted");
                    new InputReader(client.GetStream(), ReadInput).ReadToEnd();
                    client.Close();
                } catch (Exception e) {
                    Debug.WriteLine("Exception: {0}", e);
                    ((App) Application.Current).ShowNotification("Connection lost: " + e.Message);

                    try {
                        client?.Close();
                    } catch {
                        // ignore all
                    }
                }
            }
        }

        private void ReadInput(byte[] bytes, int messageStart, out int nextMessageStart)
        {
            InputStruct input;
            if (bytes[messageStart] == InputMessage.KeyDown || bytes[messageStart] == InputMessage.KeyUp) {
                var msg = InputMessage.FromBytes<KeyboardMessage>(bytes, messageStart, out var size);
                nextMessageStart = messageStart + size;

                var flags = ScanCode;
                if (msg.Type == InputMessage.KeyUp) {
                    flags |= KeyUp;
                }
                if (IsExtendedKey(msg.Key)) {
                    flags |= ExtendedKey;
                }

                input = new InputStruct {
                    Type = (uint) InputType.Keyboard,
                    Data = new InputData {
                        Keyboard = new KeyboardInputStruct {
                            ScanCode = (ushort) msg.ScanCode,
                            Flags    = (uint) flags,
                        }
                    }
                };
            } else {
                var msg = InputMessage.FromBytes<MouseMessage>(bytes, messageStart, out var size);
                nextMessageStart = messageStart + size;

                var data = new MouseInputStruct();

                if (msg.WheelDelta != 0) {
                    data.Flags |= (uint) MouseFlag.Wheel;
                    data.MouseData = msg.WheelDelta;
                }
                var btnFlag = msg.Button switch {
                    MouseButton.LeftDown   => (uint) MouseFlag.LeftDown,
                    MouseButton.LeftUp     => (uint) MouseFlag.LeftUp,
                    MouseButton.RightDown  => (uint) MouseFlag.RightDown,
                    MouseButton.RightUp    => (uint) MouseFlag.RightUp,
                    MouseButton.MiddleDown => (uint) MouseFlag.MiddleDown,
                    MouseButton.MiddleUp   => (uint) MouseFlag.MiddleUp,
                    _ => 0u
                };
                if (btnFlag != 0u) {
                    data.Flags |= btnFlag;
                } else if (msg.X != 0 || msg.Y != 0) {
                    data.Flags |= (uint) MouseFlag.Move;
                    data.X = msg.X;
                    data.Y = msg.Y;
                }

                input = new InputStruct {
                    Type = (uint) InputType.Mouse,
                    Data = new InputData { Mouse = data }
                };
            }

            var result = SendInput(1, new [] { input }, Marshal.SizeOf<InputStruct>());
            if (result == 0) {
                Debug.WriteLine("Send fail: {0}", Marshal.GetLastWin32Error());
            } else {
                Debug.WriteLine("Send result: {0}", result);
            }
        }

        private byte[] UnpackState(byte[] packed)
        {
            var state = new byte[256];
            const byte mask = 0b1000_0001;
            for (var i = 0; i < state.Length; i += 4) {
                var packedByte = packed[1 + i / 4];

                state[i + 0] = (byte) (mask & (packedByte      | packedByte >> 6));
                state[i + 1] = (byte) (mask & (packedByte << 2 | packedByte >> 4));
                state[i + 2] = (byte) (mask & (packedByte << 4 | packedByte >> 2));
                state[i + 3] = (byte) (mask & (packedByte << 6 | packedByte     ));
            }
            return state;
        }

        /// <summary>
        /// Determines if the <see cref="VirtualKey"/> is an ExtendedKey
        /// </summary>
        /// <param name="key">The key code.</param>
        /// <returns>true if the key code is an extended key; otherwise, false.</returns>
        /// <remarks>
        /// The extended keys consist of the ALT and CTRL keys on the right-hand side of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; the NUM LOCK key; the BREAK (CTRL+PAUSE) key; the PRINT SCRN key; and the divide (/) and ENTER keys in the numeric keypad.
        ///
        /// See http://msdn.microsoft.com/en-us/library/ms646267(v=vs.85).aspx Section "Extended-Key Flag"
        /// </remarks>
        private static bool IsExtendedKey(VirtualKey key)
        {
            return key == VirtualKey.Menu
                || key == VirtualKey.LMenu
                || key == VirtualKey.RMenu
                || key == VirtualKey.Control
                || key == VirtualKey.RControl
                || key == VirtualKey.Insert
                || key == VirtualKey.Delete
                || key == VirtualKey.Home
                || key == VirtualKey.End
                || key == VirtualKey.Prior
                || key == VirtualKey.Next
                || key == VirtualKey.Right
                || key == VirtualKey.Up
                || key == VirtualKey.Left
                || key == VirtualKey.Down
                || key == VirtualKey.NumLock
                || key == VirtualKey.Cancel
                || key == VirtualKey.Snapshot
                || key == VirtualKey.Divide
                || key == VirtualKey.LWin
                || key == VirtualKey.RWin
                || key == VirtualKey.Apps;
        }
    }
}
