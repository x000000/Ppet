using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Ppet;

namespace PpetTests
{
    [TestFixture]
    public class InputClientTests
    {
        private InputClient? client;
        private KeyboardHook kbHook = null!;
        private MouseHook    msHook = null!;

        [SetUp]
        public void Setup()
        {
            kbHook = Substitute.ForPartsOf<KeyboardHook>();
            kbHook.Configure().WhenForAnyArgs(x => x.Start()).DoNotCallBase();

            msHook = Substitute.ForPartsOf<MouseHook>();
            msHook.Configure().WhenForAnyArgs(x => x.Start()).DoNotCallBase();
        }

        [TearDown]
        public void TearDown()
        {
            client?.Disconnect();
            client = null;
        }

        [Test]
        public void MouseEventsTest(
            [Values(-1, 0, 4, 8)] int insertIndex,
            [Values] bool consecutiveInsert
        )
        {
            var stream = new MemoryStream(2048);
            client = new InputClient(msHook, kbHook, stream);
            client.Connect("localhost");

            var events = new [] {
                Enumerable.Range(1, 5).Select(n => (btn: MouseButton.None, x:  n, y: -n)).ToList(),
                Enumerable.Range(3, 8).Select(n => (btn: MouseButton.None, x: -n, y:  n)).ToList(),
                Enumerable.Range(6, 3).Select(n => (btn: MouseButton.None, x:  n, y: -n)).ToList(),

                Enumerable.Range(3, 8).Select(n => (btn: MouseButton.None, x:  n, y: -n)).Concat(
                    Enumerable.Range(3, 8).Select(n => (btn: MouseButton.None, x: -n, y: n))
                ).ToList(),
            };
            if (insertIndex != -1) {
                events[1].Insert(insertIndex, (MouseButton.LeftDown, 16, 16));
                if (consecutiveInsert) {
                    events[1].Insert(insertIndex, (MouseButton.LeftDown, 16, 16));
                }
            }

            var expectedBytes = new MemoryStream(2048);
            foreach (var hwEvents in events) {
                Assert.AreEqual(expectedBytes.Length, stream.Length);

                var skip = 0;
                while (skip < hwEvents.Count) {
                    if (hwEvents[skip].btn == MouseButton.None) {
                        var moveEvents = hwEvents.Skip(skip)
                            .TakeWhile(t => t.btn == MouseButton.None)
                            .Select(t => (t.x, t.y))
                            .ToList();
                        skip += moveEvents.Count;

                        var (x, y) = MoveMouse(moveEvents);
                        if (x != 0 || y != 0) {
                            var msg = new MouseMessage {
                                Type = InputMessage.MouseWheel,
                                Button = MouseButton.None,
                                X = x,
                                Y = y,
                            };
                            var bytes = InputMessage.GetBytes(msg);
                            expectedBytes.Write(bytes, 0, bytes.Length);
                        }
                    } else {
                        var (btn, x, y) = hwEvents[skip++];
                        msHook.InvokeHandlers(new MouseEventArgs(btn, x, y, 0));

                        var type = Ppet.InputClient.ButtonToType.TryGetValue(btn, out var b) ? b : InputMessage.MouseWheel;
                        var bytes = InputMessage.GetBytes(new MouseMessage {
                            Type = type,
                            Button = btn,
                            X = x,
                            Y = y,
                        });
                        expectedBytes.Write(bytes, 0, bytes.Length);
                    }
                }

                Thread.Sleep(Ppet.InputClient.FlushInterval * 2);
            }

            Assert.AreEqual(expectedBytes.ToArray(), stream.ToArray());
        }

        private (int x, int y) MoveMouse(IEnumerable<(int x, int y)> hwEvents)
        {
            var x = 0;
            var y = 0;
            foreach (var (nx, ny) in hwEvents) {
                x += nx;
                y += ny;
                msHook.InvokeHandlers(new MouseEventArgs(MouseButton.None, nx, ny, 0));
            }
            return (x, y);
        }

        public class InputClient : Ppet.InputClient
        {
            private readonly Stream stream;

            public InputClient(MouseHook msHook, KeyboardHook kbHook, Stream stream) : base(msHook, kbHook)
                => this.stream = stream;

            protected internal override (TcpClient?, Stream) Connect(string address, int port) => (null, stream);
        }
    }
}
