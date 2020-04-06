using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Ppet;

namespace PpetTests
{
    [TestFixture]
    public class InputReaderTests
    {
        public static IEnumerable<object[]> TestData()
        {
            var arr = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            foreach (var size in new [] { 2, 3, 4, 5 }) {
                var nChunks = arr.Length / size;
                var chunks = new List<byte[]>(nChunks);
                for (int i = 0; i < nChunks; i++) {
                    chunks.Add( arr.Skip(size * i).Take(size).ToArray() );
                }

                foreach (var bufferSize in new [] { 3, 4, 5, 32 }) {
                    yield return new object[] { arr, size, chunks, bufferSize };
                }
            }
        }

        [Test]
        [TestCaseSource(nameof(TestData))]
        public void InputStreamReadTest(byte[] streamData, int messageSize, IList<byte[]> messages, int bufferSize)
        {
            var stream  = new MemoryStream(streamData);
            var handler = Substitute.ForPartsOf<InputHandler>(messageSize);
            var reader  = new InputReader(stream, handler.Handle, bufferSize);
            reader.ReadToEnd();

            handler.ReceivedWithAnyArgs(messages.Count).OnMessage(new byte[0]);
            Received.InOrder(() => {
                foreach (var msg in messages) {
                    var eq = Is.EquivalentTo(msg);
                    handler.OnMessage(Arg.Is<byte[]>(o => eq.ApplyTo(o).IsSuccess));
                }
            });
        }

        public class InputHandler
        {
            private readonly int msgSize;

            public InputHandler(int messageSize) => msgSize = messageSize;

            public virtual void Handle(byte[] bytes, int startIndex, out int nextStartIndex)
            {
                nextStartIndex = startIndex + msgSize;
                if (nextStartIndex > bytes.Length) {
                    throw new IndexOutOfRangeException();
                }
                OnMessage( bytes.Skip(startIndex).Take(msgSize).ToArray() );
            }

            public virtual void OnMessage(byte[] bytes)
                => Debug.WriteLine("[{0}]", string.Join(' ', bytes));
        }
    }
}
