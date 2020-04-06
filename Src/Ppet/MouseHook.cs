using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using static Ppet.WinApi;
using static Ppet.WinApi.HookEventTypes;

namespace Ppet
{
    public enum MouseButton
    {
        None        = 0,
        LeftDown    = 1,
        LeftUp      = 2,
        LeftDClick  = 3,
        RightDown   = 4,
        RightUp     = 5,
        RightDClick = 6,
        MiddleDown  = 7,
        MiddleUp    = 8,
    }

    public class MouseEventArgs
    {
        public MouseButton Button { get; }
        public int X { get; }
        public int Y { get; }
        public short WheelDelta { get; }

        public MouseEventArgs(MouseButton button, int x, int y, short wheelDelta)
        {
            Button = button;
            X = x;
            Y = y;
            WheelDelta = wheelDelta;
        }

        public override string ToString() => $"{Button} ({X}, {Y}) {WheelDelta}";
    }

    public delegate bool MouseEventHandler(object sender, MouseEventArgs args);

    /// <summary>
    /// This class allows you to tap mouse and/or to detect its activity even when an
    /// application runes in background or does not have any user interface at all.
    /// This class raises common .NET events with MouseEventArgs so
    /// you can easily retrieve any information you need.
    /// </summary>
    public class MouseHook
    {
        /// <summary>Occurs when the user moves the mouse, presses any mouse button or scrolls the wheel</summary>
        public event MouseEventHandler MouseActivity
        {
            add => handlers.Add(value);
            remove => handlers.Remove(value);
        }

        /// <summary>Stores the handle to the mouse hook procedure.</summary>
        private int hMouseHook;

        // we have to have the reference to the callback as a field to prevent it from CG
        private HookProc? hookCallback;

        private readonly ISet<MouseEventHandler> handlers = new HashSet<MouseEventHandler>();

        ~MouseHook() => Stop(false);

        /// <summary>Installs mouse hook and starts rasing events</summary>
        /// <exception cref="Win32Exception">Any windows problem.</exception>
        public virtual void Start()
        {
            // install Mouse hook only if it is not installed and must be installed
            if (hMouseHook == 0) {
                hMouseHook = SetWindowsHookEx(
                    (int) HookType.MouseLowLevel,
                    hookCallback = MouseHookProc,
                    Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),
                    0
                );
                //If SetWindowsHookEx fails.
                if (hMouseHook == 0) {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set.
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup
                    Stop(false);
                    //Initializes and throws a new instance of the Win32Exception class with the specified error.
                    throw new Win32Exception(errorCode);
                }
            }
        }

        /// <summary>Stops monitoring both mouse and keyboard events and rasing events.</summary>
        /// <exception cref="Win32Exception">Any windows problem.</exception>
        public void Stop() => Stop(true);

        /// <summary>Stops monitoring both or one of mouse and/or keyboard events and rasing events.</summary>
        /// <param name="throwExceptions"><b>true</b> if exceptions which occured during uninstalling must be thrown</param>
        /// <exception cref="Win32Exception">Any windows problem.</exception>
        private void Stop(bool throwExceptions)
        {
            if (hMouseHook != 0) {
                var result = UnhookWindowsHookEx(hMouseHook);
                hMouseHook = 0;
                if (result == 0 && throwExceptions) {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set.
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error.
                    throw new Win32Exception(errorCode);
                }
            }
        }


        /// <summary>A callback function which will be called every time a mouse activity detected.</summary>
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
        private int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            // if ok and someone listens to our events
            if (nCode >= 0 && handlers.Count != 0) {
                //Marshall the data from callback.
                var data = Marshal.PtrToStructure<MouseHookStruct>(lParam);

                // var pos = new PointStruct();

                //detect button clicked
                var button = MouseButton.None;
                short wheelDelta = 0;
                if (wParam == WM_MOUSEWHEEL) {
                    //If the message is WM_MOUSEWHEEL, the high-order word of MouseData member is the wheel delta.
                    //One wheel click is defined as WHEEL_DELTA, which is 120.
                    //(value >> 16) & 0xffff; retrieves the high-order word from the given 32-bit value
                    wheelDelta = (short) ((data.MouseData >> 16) & 0xffff);
                } else {
                    button = wParam switch {
                        WM_LBUTTONDOWN   => MouseButton.LeftDown,
                        WM_LBUTTONUP     => MouseButton.LeftUp,
                        WM_RBUTTONDOWN   => MouseButton.RightDown,
                        WM_RBUTTONUP     => MouseButton.RightUp,
                        // WM_LBUTTONDBLCLK => MouseButton.LeftDClick,
                        // WM_RBUTTONDBLCLK => MouseButton.RightDClick,
                        WM_MBUTTONDOWN   => MouseButton.MiddleDown,
                        WM_MBUTTONUP     => MouseButton.MiddleUp,
                        _ => MouseButton.None
                    };
                    //TODO: X BUTTONS (I havent them so was unable to test)
                    //If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP,
                    //or WM_NCXBUTTONDBLCLK, the high-order word specifies which X button was pressed or released,
                    //and the low-order word is reserved. This value can be one or more of the following values.
                    //Otherwise, MouseData is not used.
                }

                if (button == MouseButton.None && GetCursorPos(out var pos)) {
                    data.X -= pos.X;
                    data.Y -= pos.Y;
                }

                if (!InvokeHandlers(new MouseEventArgs(button, data.X, data.Y, wheelDelta))) {
                    return 1;
                }
            }
            return CallNextHookEx(hMouseHook, nCode, wParam, lParam);
        }

        protected internal virtual bool InvokeHandlers(MouseEventArgs eventData)
        {
            foreach (var handler in handlers) {
                if (!handler(this, eventData)) {
                    return false;
                }
            }
            return true;
        }
    }
}
