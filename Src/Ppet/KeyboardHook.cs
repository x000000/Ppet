using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using static Ppet.WinApi;
using static Ppet.WinApi.HookEventTypes;

namespace Ppet
{
    public class KeyEventArgs : EventArgs
    {
        public VirtualKey Key { get; }
        public uint Flags { get; }
        public uint ScanCode { get; }

        public KeyEventArgs(VirtualKey key, uint flags, uint scanCode)
        {
            Key = key;
            Flags = flags;
            ScanCode = scanCode;
        }

        public override string ToString() => Key.ToString();
    }

    public delegate bool KeyEventHandler(object sender, KeyEventArgs args);

    /// <summary>
    /// This class allows you to tap keyboard and/or to detect its activity even when an
    /// application runes in background or does not have any user interface at all.
    /// This class raises common .NET events with KeyEventArgs so
    /// you can easily retrieve any information you need.
    /// </summary>
    public class KeyboardHook
    {
        /// <summary>Occurs when the user presses a key</summary>
        public event KeyEventHandler KeyDown
        {
            add    => kdHandlers.Add(value);
            remove => kdHandlers.Remove(value);
        }

        /// <summary>Occurs when the user releases a key</summary>
        public event KeyEventHandler KeyUp
        {
            add    => kuHandlers.Add(value);
            remove => kuHandlers.Remove(value);
        }

        /// <summary>Stores the handle to the keyboard hook procedure.</summary>
        private int hKeyboardHook;

        // we have to have the reference to the callback as a field to prevent it from CG
        private HookProc? hookCallback;

        private readonly ISet<KeyEventHandler> kdHandlers = new HashSet<KeyEventHandler>();
        private readonly ISet<KeyEventHandler> kuHandlers = new HashSet<KeyEventHandler>();

        ~KeyboardHook() => Stop(false);

        /// <summary>Installs keyboard hook and starts rasing events</summary>
        /// <exception cref="Win32Exception">Any windows problem.</exception>
        public virtual void Start()
        {
            // install Keyboard hook only if it is not installed and must be installed
            if (hKeyboardHook == 0) {
                hKeyboardHook = SetWindowsHookEx(
                    (int) HookType.KeyboardLowLevel,
                    hookCallback = KeyboardHookProc,
                    Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),
                    0
                );
                //If SetWindowsHookEx fails.
                if (hKeyboardHook == 0) {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set.
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup
                    Stop(false);
                    //Initializes and throws a new instance of the Win32Exception class with the specified error.
                    throw new Win32Exception(errorCode);
                }
            }
        }

        /// <summary>
        /// Stops monitoring both mouse and keyboard events and rasing events.
        /// </summary>
        /// <exception cref="Win32Exception">Any windows problem.</exception>
        public void Stop() => Stop(true);

        /// <summary>
        /// Stops monitoring both or one of mouse and/or keyboard events and rasing events.
        /// </summary>
        /// <param name="throwExceptions"><b>true</b> if exceptions which occured during uninstalling must be thrown</param>
        /// <exception cref="Win32Exception">Any windows problem.</exception>
        private void Stop(bool throwExceptions)
        {
            if (hKeyboardHook != 0) {
                var result = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
                if (result == 0 && throwExceptions) {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set.
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error.
                    throw new Win32Exception(errorCode);
                }
            }
        }


        /// <summary>
        /// A callback function which will be called every time a keyboard activity detected.
        /// </summary>
        /// <param name="nCode">
        /// [in] Specifies whether the hook procedure must process the message.
        /// If nCode is HC_ACTION, the hook procedure must process the message.
        /// If nCode is less than zero, the hook procedure must pass the message to the
        /// CallNextHookEx function without further processing and must return the
        /// value returned by CallNextHookEx.
        /// </param>
        /// <param name="wParam">
        /// [in] Specifies whether the message was sent by the current thread.
        /// If the message was sent by the current thread, it is nonzero; otherwise, it is zero.
        /// </param>
        /// <param name="lParam">
        /// [in] Pointer to a CWPSTRUCT structure that contains details about the message.
        /// </param>
        /// <returns>
        /// If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx.
        /// If nCode is greater than or equal to zero, it is highly recommended that you call CallNextHookEx
        /// and return the value it returns; otherwise, other applications that have installed WH_CALLWNDPROC
        /// hooks will not receive hook notifications and may behave incorrectly as a result. If the hook
        /// procedure does not call CallNextHookEx, the return value should be zero.
        /// </returns>
        private int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (kdHandlers.Count > 0 || kuHandlers.Count > 0)) {
                switch (wParam) {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        if (kdHandlers.Count > 0 && !InvokeHandlers(lParam, kdHandlers)) {
                            return 1;
                        }
                        break;
                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                        if (kuHandlers.Count > 0 && !InvokeHandlers(lParam, kuHandlers)) {
                            return 1;
                        }
                        break;
                }
            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

        protected internal virtual bool InvokeHandlers(IntPtr lParam, ISet<KeyEventHandler> handlers)
        {
            var data = Marshal.PtrToStructure<KeyboardHookStruct>(lParam);
            var args = new KeyEventArgs((VirtualKey) data.VkCode, data.Flags, data.ScanCode);
            foreach (var handler in handlers) {
                if (!handler(this, args)) {
                    return false;
                }
            }
            return true;
        }
    }
}
