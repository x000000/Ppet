using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Ppet
{
    public static class Debug
    {
        public const bool IsEnabled =
#if DEBUG
            true
#else
            false
#endif
        ;

        public static ObservableCollection<string> History { get; } = new ObservableCollection<string>();

#if DEBUG
        private const int Limit = 300;
        private static readonly object Guard = new object();
#endif

        public static void WriteLine(string text, params object[] rest)
        {
#if DEBUG
            if (rest.Length > 0) {
                text = string.Format(text, rest);
            }
            Application.Current?.Dispatcher?.Invoke(() => {
                lock (Guard) {
                    for (var count = History.Count; count >= Limit; count--) {
                        History.RemoveAt(count - 1);
                    }
                    History.Insert(0, text);
                }
            });
            Console.WriteLine(text);
#endif
        }
    }
}
