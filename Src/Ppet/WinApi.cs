using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Ppet
{
    public static class WinApi
    {
        #region Windows structure definitions

        /// <summary>The MSLLHOOKSTRUCT structure contains information about a low-level keyboard input event.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MouseHookStruct
        {
            /// <summary>Specifies the x-coordinate of the point.</summary>
            public int X;
            /// <summary>Specifies the y-coordinate of the point.</summary>
            public int Y;
            /// <summary>
            /// If the message is WM_MOUSEWHEEL, the high-order word of this member is the wheel delta.
            /// The low-order word is reserved. A positive value indicates that the wheel was rotated forward,
            /// away from the user; a negative value indicates that the wheel was rotated backward, toward the user.
            /// One wheel click is defined as WHEEL_DELTA, which is 120.
            ///If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP,
            /// or WM_NCXBUTTONDBLCLK, the high-order word specifies which X button was pressed or released,
            /// and the low-order word is reserved. This value can be one or more of the following values. Otherwise, MouseData is not used.
            ///XBUTTON1
            ///The first X button was pressed or released.
            ///XBUTTON2
            ///The second X button was pressed or released.
            /// </summary>
            public int MouseData;
            /// <summary>
            /// Specifies the event-injected flag. An application can use the following value to test the mouse flags.
            /// Value Purpose LLMHF_INJECTED Test the event-injected flag.
            /// 0 Specifies whether the event was injected.
            /// The value is 1 if the event was injected; otherwise, it is 0.
            /// 1-15 Reserved.
            /// </summary>
            public uint Flags;
            /// <summary>Specifies the time stamp for this message.</summary>
            public int Time;
            /// <summary>Specifies extra information associated with the message.</summary>
            public uint ExtraInfo;
        }

        /// <summary>The KBDLLHOOKSTRUCT structure contains information about a low-level keyboard input event.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardHookStruct
        {
            /// <summary>Specifies a virtual-key code. The code must be a value in the range 1 to 254.</summary>
            public uint VkCode;
            /// <summary>Specifies a hardware scan code for the key.</summary>
            public uint ScanCode;
            /// <summary>Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.</summary>
            public uint Flags;
            /// <summary>Specifies the time stamp for this message.</summary>
            public uint Time;
            /// <summary>Specifies extra information associated with the message.</summary>
            public uint ExtraInfo;
        }

        /// <summary>The MOUSEINPUT structure contains information about a simulated mouse event.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInputStruct
        {
            /// <summary>Specifies the x-coordinate of the point.</summary>
            public int X;
            /// <summary>Specifies the y-coordinate of the point.</summary>
            public int Y;
            /// <summary>
            /// If the message is WM_MOUSEWHEEL, the high-order word of this member is the wheel delta.
            /// The low-order word is reserved. A positive value indicates that the wheel was rotated forward,
            /// away from the user; a negative value indicates that the wheel was rotated backward, toward the user.
            /// One wheel click is defined as WHEEL_DELTA, which is 120.
            ///If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP,
            /// or WM_NCXBUTTONDBLCLK, the high-order word specifies which X button was pressed or released,
            /// and the low-order word is reserved. This value can be one or more of the following values. Otherwise, mouseData is not used.
            ///XBUTTON1
            ///The first X button was pressed or released.
            ///XBUTTON2
            ///The second X button was pressed or released.
            /// </summary>
            public int MouseData;
            /// <summary>
            /// Specifies the event-injected flag. An application can use the following value to test the mouse flags. Value Purpose
            ///LLMHF_INJECTED Test the event-injected flag.
            ///0
            ///Specifies whether the event was injected. The value is 1 if the event was injected; otherwise, it is 0.
            ///1-15
            ///Reserved.
            /// </summary>
            public uint Flags;
            /// <summary>
            /// Specifies the time stamp for this message.
            /// </summary>
            public int Time;
            /// <summary>
            /// Specifies extra information associated with the message.
            /// </summary>
            public IntPtr ExtraInfo;
        }

        /// <summary>The KEYBDINPUT structure contains information about a simulated keyboard event.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInputStruct
        {
            /// <summary>Specifies a virtual-key code. The code must be a value in the range 1 to 254.</summary>
            public ushort VkCode;
            /// <summary>Specifies a hardware scan code for the key.</summary>
            public ushort ScanCode;
            /// <summary>Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.</summary>
            public uint Flags;
            /// <summary>Specifies the time stamp for this message.</summary>
            public uint Time;
            /// <summary>Specifies extra information associated with the message.</summary>
            public IntPtr ExtraInfo;
        }

        /// <summary>
        /// The HARDWAREINPUT structure contains information about a simulated message generated by an input device other than a keyboard or mouse.
        /// (see: http://msdn.microsoft.com/en-us/library/ms646269(VS.85).aspx)
        /// Declared in Winuser.h, include Windows.h
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HwInputStruct
        {
            /// <summary>Value specifying the message generated by the input hardware.</summary>
            public uint Msg;
            /// <summary>Specifies the low-order word of the lParam parameter for uMsg.</summary>
            public ushort LParam;
            /// <summary>Specifies the high-order word of the lParam parameter for uMsg.</summary>
            public ushort HParam;
        }

        /// <summary>
        /// The InputStruct structure is used by SendInput to store information for synthesizing input events such as keystrokes, mouse movement, and mouse clicks. (see: http://msdn.microsoft.com/en-us/library/ms646270(VS.85).aspx)
        /// Declared in Winuser.h, include Windows.h
        /// </summary>
        /// <remarks>
        /// This structure contains information identical to that used in the parameter list of the keybd_event or mouse_event function.
        /// Windows 2000/XP: INPUT_KEYBOARD supports nonkeyboard input methods, such as handwriting recognition or voice recognition, as if it were text input by using the KEYEVENTF_UNICODE flag. For more information, see the remarks section of KEYBDINPUT.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct InputStruct
        {
            /// <summary>
            /// Specifies the type of the input event. This member can be one of the following values.
            /// <see cref="InputType.Mouse"/> - The event is a mouse event. Use the mi structure of the union.
            /// <see cref="InputType.Keyboard"/> - The event is a keyboard event. Use the ki structure of the union.
            /// <see cref="InputType.Hardware"/> - Windows 95/98/Me: The event is from input hardware other than a keyboard or mouse. Use the hi structure of the union.
            /// </summary>
            public uint Type;

            /// <summary>
            /// The data structure that contains information about the simulated Mouse, Keyboard or Hardware event.
            /// </summary>
            public InputData Data;
        }

        /// <summary>
        /// The combined/overlayed structure that includes Mouse, Keyboard and Hardware Input message data (see: http://msdn.microsoft.com/en-us/library/ms646270(VS.85).aspx)
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct InputData
        {
            [FieldOffset(0)]
            public MouseInputStruct Mouse;

            [FieldOffset(0)]
            public KeyboardInputStruct Keyboard;

            [FieldOffset(0)]
            public HwInputStruct Hardware;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PointStruct
        {
            public int X;
            public int Y;
        }

        #endregion

        #region Windows constants

        /// <summary>
        /// Specifies various aspects of a mouse event. This member can be reasonable combination of the following values.
        /// </summary>
        [Flags]
        public enum MouseFlag : uint
        {
            Absolute       = 0x8000,  // The dx and dy members contain normalized absolute coordinates. If the flag is not set, dxand dy contain relative data (the change in position since the last reported position). This flag can be set, or not set, regardless of what kind of mouse or other pointing device, if any, is connected to the system. For further information about relative mouse motion, see the following Remarks section.
            HWheel         = 0x1000,  // The wheel was moved horizontally, if the mouse has a wheel. The amount of movement is specified in MouseData. (Windows XP/2000:  This value is not supported)
            Move           = 0x0001,  // Movement occurred.
            MoveNoCoalesce = 0x2000,  // The WM_MOUSEMOVE messages will not be coalesced. The default behavior is to coalesce WM_MOUSEMOVE messages. (Windows XP/2000:  This value is not supported)
            LeftDown       = 0x0002,  // The left button was pressed.
            LeftUp         = 0x0004,  // The left button was released.
            RightDown      = 0x0008,  // The right button was pressed.
            RightUp        = 0x0010,  // The right button was released.
            MiddleDown     = 0x0020,  // The middle button was pressed.
            MiddleUp       = 0x0040,  // The middle button was released.
            VirtualDesk    = 0x4000,  // Maps coordinates to the entire desktop. Must be used with MOUSEEVENTF_ABSOLUTE.
            Wheel          = 0x0800,  // The wheel was moved, if the mouse has a wheel. The amount of movement is specified in MouseData.
            XDown          = 0x0080,  // An X button was pressed.
            XUp            = 0x0100,  // An X button was released.
        }

        /// <summary>
        /// Specifies various aspects of a keystroke. This member can be certain combinations of the following values.
        /// </summary>
        [Flags]
        public enum KeyboardFlag : uint
        {
            /// <summary>
            /// KEYEVENTF_EXTENDEDKEY = 0x0001 (If specified, the scan code was preceded by a prefix byte that has the value 0xE0 (224).)
            /// </summary>
            ExtendedKey = 0x0001,

            /// <summary>
            /// KEYEVENTF_KEYUP = 0x0002 (If specified, the key is being released. If not specified, the key is being pressed.)
            /// </summary>
            KeyUp = 0x0002,

            /// <summary>
            /// KEYEVENTF_UNICODE = 0x0004 (If specified, wScan identifies the key and wVk is ignored.)
            /// </summary>
            Unicode = 0x0004,

            /// <summary>
            /// KEYEVENTF_SCANCODE = 0x0008 (Windows 2000/XP: If specified, the system synthesizes a VK_PACKET keystroke.
            /// The wVk parameter must be zero. This flag can only be combined with the KEYEVENTF_KEYUP flag.
            /// For more information, see the Remarks section.)
            /// </summary>
            ScanCode = 0x0008,
        }

        public enum InputType : uint
        {
            Mouse    = 0,
            Keyboard = 1,
            Hardware = 2,
        }

        public enum HookType
        {
            /// <summary>
            /// Windows NT/2000/XP: Installs a hook procedure that monitors low-level mouse input events.
            /// </summary>
            MouseLowLevel    = 14,
            /// <summary>
            /// Windows NT/2000/XP: Installs a hook procedure that monitors low-level keyboard  input events.
            /// </summary>
            KeyboardLowLevel = 13,
        }

        /// <summary>
        /// The list of VirtualKeyCodes (see: http://msdn.microsoft.com/en-us/library/ms645540(VS.85).aspx)
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum VirtualKey //: uint
        {
            LButton           = 0x01,
            RButton           = 0x02,
            Cancel            = 0x03,
            MButton           = 0x04,
            XButton1          = 0x05,
            XButton2          = 0x06,
            //                  0x07 : Undefined
            Back              = 0x08,
            Tab               = 0x09,
            //                  0x0A - 0x0B : Reserved
            Clear             = 0x0C,
            Return            = 0x0D,
            //                  0x0E - 0x0F : Undefined
            Shift             = 0x10,
            Control           = 0x11,
            Menu              = 0x12,
            Pause             = 0x13,
            Capital           = 0x14,
            Kana              = 0x15,
            Hangeul           = 0x15,
            Hangul            = 0x15,
            //                  0x16 : Undefined
            Junja             = 0x17,
            Final             = 0x18,
            Hanja             = 0x19,
            Kanji             = 0x19,
            //                  0x1A : Undefined
            Escape            = 0x1B,
            Convert           = 0x1C,
            NonConvert        = 0x1D,
            Accept            = 0x1E,
            ModeChange        = 0x1F,
            Space             = 0x20,
            Prior             = 0x21,
            Next              = 0x22,
            End               = 0x23,
            Home              = 0x24,
            Left              = 0x25,
            Up                = 0x26,
            Right             = 0x27,
            Down              = 0x28,
            Select            = 0x29,
            Print             = 0x2A,
            Execute           = 0x2B,
            Snapshot          = 0x2C,
            Insert            = 0x2D,
            Delete            = 0x2E,
            Help              = 0x2F,
            VK_0              = 0x30,
            VK_1              = 0x31,
            VK_2              = 0x32,
            VK_3              = 0x33,
            VK_4              = 0x34,
            VK_5              = 0x35,
            VK_6              = 0x36,
            VK_7              = 0x37,
            VK_8              = 0x38,
            VK_9              = 0x39,
            //                  0x3A - 0x40 : Udefined
            VK_A              = 0x41,
            VK_B              = 0x42,
            VK_C              = 0x43,
            VK_D              = 0x44,
            VK_E              = 0x45,
            VK_F              = 0x46,
            VK_G              = 0x47,
            VK_H              = 0x48,
            VK_I              = 0x49,
            VK_J              = 0x4A,
            VK_K              = 0x4B,
            VK_L              = 0x4C,
            VK_M              = 0x4D,
            VK_N              = 0x4E,
            VK_O              = 0x4F,
            VK_P              = 0x50,
            VK_Q              = 0x51,
            VK_R              = 0x52,
            VK_S              = 0x53,
            VK_T              = 0x54,
            VK_U              = 0x55,
            VK_V              = 0x56,
            VK_W              = 0x57,
            VK_X              = 0x58,
            VK_Y              = 0x59,
            VK_Z              = 0x5A,
            LWin              = 0x5B,
            RWin              = 0x5C,
            Apps              = 0x5D,
            //                  0x5E : reserved
            Sleep             = 0x5F,
            Numpad0           = 0x60,
            Numpad1           = 0x61,
            Numpad2           = 0x62,
            Numpad3           = 0x63,
            Numpad4           = 0x64,
            Numpad5           = 0x65,
            Numpad6           = 0x66,
            Numpad7           = 0x67,
            Numpad8           = 0x68,
            Numpad9           = 0x69,
            Multiply          = 0x6A,
            Add               = 0x6B,
            Separator         = 0x6C,
            Subtract          = 0x6D,
            Decimal           = 0x6E,
            Divide            = 0x6F,
            F1                = 0x70,
            F2                = 0x71,
            F3                = 0x72,
            F4                = 0x73,
            F5                = 0x74,
            F6                = 0x75,
            F7                = 0x76,
            F8                = 0x77,
            F9                = 0x78,
            F10               = 0x79,
            F11               = 0x7A,
            F12               = 0x7B,
            F13               = 0x7C,
            F14               = 0x7D,
            F15               = 0x7E,
            F16               = 0x7F,
            F17               = 0x80,
            F18               = 0x81,
            F19               = 0x82,
            F20               = 0x83,
            F21               = 0x84,
            F22               = 0x85,
            F23               = 0x86,
            F24               = 0x87,
            //                  0x88 - 0x8F : Unassigned
            NumLock           = 0x90,
            Scroll            = 0x91,
            //                  0x92 - 0x96 : OEM Specific
            //                  0x97 - 0x9F : Unassigned
            //
            // L* & R* - left and right Alt, Ctrl and Shift virtual keys.
            // Used only as parameters to GetAsyncKeyState() and GetKeyState().
            // No other API or message will distinguish left and right keys in this way.
            //
            LShift            = 0xA0,
            RShift            = 0xA1,
            LControl          = 0xA2,
            RControl          = 0xA3,
            LMenu             = 0xA4,
            RMenu             = 0xA5,
            BrowserBack       = 0xA6,
            BrowserForward    = 0xA7,
            BrowserRefresh    = 0xA8,
            BrowserStop       = 0xA9,
            BrowserSearch     = 0xAA,
            BrowserFavorites  = 0xAB,
            BrowserHome       = 0xAC,
            VolumeMute        = 0xAD,
            VolumeDown        = 0xAE,
            VolumeUp          = 0xAF,
            MediaNextTrack    = 0xB0,
            MediaPrevTrack    = 0xB1,
            MediaStop         = 0xB2,
            MediaPlayPause    = 0xB3,
            LaunchMail        = 0xB4,
            LaunchMediaSelect = 0xB5,
            LaunchApp1        = 0xB6,
            LaunchApp2        = 0xB7,
            //                  0xB8 - 0xB9 : Reserved
            Oem1              = 0xBA,
            OemPlus           = 0xBB,
            OemComma          = 0xBC,
            OemMinus          = 0xBD,
            OemPeriod         = 0xBE,
            Oem2              = 0xBF,
            Oem3              = 0xC0,
            //                  0xC1 - 0xD7 : Reserved
            //                  0xD8 - 0xDA : Unassigned
            Oem4              = 0xDB,
            Oem5              = 0xDC,
            Oem6              = 0xDD,
            Oem7              = 0xDE,
            Oem8              = 0xDF,
            //                  0xE0 : Reserved
            //                  0xE1 : OEM Specific
            Oem102            = 0xE2,
            //                  0xE3 - 0xE4) : OEM specific
            ProcessKey        = 0xE5,
            //                  0xE6 : OEM specific
            Packet            = 0xE7,
            //                  0xE8 : Unassigned
            //                  0xE9 - 0xF5 : OEM specific
            Attn              = 0xF6,
            CrSel             = 0xF7,
            ExSel             = 0xF8,
            ErEof             = 0xF9,
            Play              = 0xFA,
            Zoom              = 0xFB,
            Noname            = 0xFC,
            Pa1               = 0xFD,
            OemClear          = 0xFE,
        }

        //values from Winuser.h in Microsoft SDK.
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static class HookEventTypes
        {
            /// <summary>
            /// The WM_MOUSEMOVE message is posted to a window when the cursor moves.
            /// </summary>
            public const int WM_MOUSEMOVE      = 0x200;
            /// <summary>
            /// The WM_LBUTTONDOWN message is posted when the user presses the left mouse button
            /// </summary>
            public const int WM_LBUTTONDOWN    = 0x201;
            /// <summary>
            /// The WM_RBUTTONDOWN message is posted when the user presses the right mouse button
            /// </summary>
            public const int WM_RBUTTONDOWN    = 0x204;
            /// <summary>
            /// The WM_MBUTTONDOWN message is posted when the user presses the middle mouse button
            /// </summary>
            public const int WM_MBUTTONDOWN    = 0x207;
            /// <summary>
            /// The WM_LBUTTONUP message is posted when the user releases the left mouse button
            /// </summary>
            public const int WM_LBUTTONUP      = 0x202;
            /// <summary>
            /// The WM_RBUTTONUP message is posted when the user releases the right mouse button
            /// </summary>
            public const int WM_RBUTTONUP      = 0x205;
            /// <summary>
            /// The WM_MBUTTONUP message is posted when the user releases the middle mouse button
            /// </summary>
            public const int WM_MBUTTONUP      = 0x208;
            /// <summary>
            /// The WM_LBUTTONDBLCLK message is posted when the user double-clicks the left mouse button
            /// </summary>
            public const int WM_LBUTTONDBLCLK  = 0x203;
            /// <summary>
            /// The WM_RBUTTONDBLCLK message is posted when the user double-clicks the right mouse button
            /// </summary>
            public const int WM_RBUTTONDBLCLK  = 0x206;
            /// <summary>
            /// The WM_RBUTTONDOWN message is posted when the user presses the right mouse button
            /// </summary>
            public const int WM_MBUTTONDBLCLK  = 0x209;
            /// <summary>
            /// The WM_MOUSEWHEEL message is posted when the user presses the mouse wheel.
            /// </summary>
            public const int WM_MOUSEWHEEL     = 0x020A;

            /// <summary>
            /// The WM_KEYDOWN message is posted to the window with the keyboard focus when a nonsystem
            /// key is pressed. A nonsystem key is a key that is pressed when the ALT key is not pressed.
            /// </summary>
            public const int WM_KEYDOWN = 0x100;
            /// <summary>
            /// The WM_KEYUP message is posted to the window with the keyboard focus when a nonsystem
            /// key is released. A nonsystem key is a key that is pressed when the ALT key is not pressed,
            /// or a keyboard key that is pressed when a window has the keyboard focus.
            /// </summary>
            public const int WM_KEYUP = 0x101;
            /// <summary>
            /// The WM_SYSKEYDOWN message is posted to the window with the keyboard focus when the user
            /// presses the F10 key (which activates the menu bar) or holds down the ALT key and then
            /// presses another key. It also occurs when no window currently has the keyboard focus;
            /// in this case, the WM_SYSKEYDOWN message is sent to the active window. The window that
            /// receives the message can distinguish between these two contexts by checking the context
            /// code in the lParam parameter.
            /// </summary>
            public const int WM_SYSKEYDOWN = 0x104;
            /// <summary>
            /// The WM_SYSKEYUP message is posted to the window with the keyboard focus when the user
            /// releases a key that was pressed while the ALT key was held down. It also occurs when no
            /// window currently has the keyboard focus; in this case, the WM_SYSKEYUP message is sent
            /// to the active window. The window that receives the message can distinguish between
            /// these two contexts by checking the context code in the lParam parameter.
            /// </summary>
            public const int WM_SYSKEYUP = 0x105;
        }

        #endregion

        #region External function imports

        /// <summary>
        /// The SetWindowsHookEx function installs an application-defined hook procedure into a hook chain.
        /// You would install a hook procedure to monitor the system for certain types of events. These events
        /// are associated either with a specific thread or with all threads in the same desktop as the calling thread.
        /// </summary>
        /// <param name="idHook">
        /// [in] Specifies the type of hook procedure to be installed. This parameter can be one of the following values.
        /// </param>
        /// <param name="lpfn">
        /// [in] Pointer to the hook procedure. If the dwThreadId parameter is zero or specifies the identifier of a
        /// thread created by a different process, the lpfn parameter must point to a hook procedure in a dynamic-link
        /// library (DLL). Otherwise, lpfn can point to a hook procedure in the code associated with the current process.
        /// </param>
        /// <param name="hMod">
        /// [in] Handle to the DLL containing the hook procedure pointed to by the lpfn parameter.
        /// The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread created by
        /// the current process and if the hook procedure is within the code associated with the current process.
        /// </param>
        /// <param name="dwThreadId">
        /// [in] Specifies the identifier of the thread with which the hook procedure is to be associated.
        /// If this parameter is zero, the hook procedure is associated with all existing threads running in the
        /// same desktop as the calling thread.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is the handle to the hook procedure.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
        /// </returns>
        /// <remarks>
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
        /// </remarks>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        /// <summary>
        /// The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// </summary>
        /// <param name="idHook">
        /// [in] Handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to SetWindowsHookEx.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        /// <remarks>
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
        /// </remarks>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int UnhookWindowsHookEx(int idHook);

        /// <summary>
        /// The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain.
        /// A hook procedure can call this function either before or after processing the hook information.
        /// </summary>
        /// <param name="idHook">Ignored.</param>
        /// <param name="nCode">
        /// [in] Specifies the hook code passed to the current hook procedure.
        /// The next hook procedure uses this code to determine how to process the hook information.
        /// </param>
        /// <param name="wParam">
        /// [in] Specifies the wParam value passed to the current hook procedure.
        /// The meaning of this parameter depends on the type of hook associated with the current hook chain.
        /// </param>
        /// <param name="lParam">
        /// [in] Specifies the lParam value passed to the current hook procedure.
        /// The meaning of this parameter depends on the type of hook associated with the current hook chain.
        /// </param>
        /// <returns>
        /// This value is returned by the next hook procedure in the chain.
        /// The current hook procedure must also return this value. The meaning of the return value depends on the hook type.
        /// For more information, see the descriptions of the individual hook procedures.
        /// </returns>
        /// <remarks>
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
        /// </remarks>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        /// <summary>
        /// The CallWndProc hook procedure is an application-defined or library-defined callback
        /// function used with the SetWindowsHookEx function. The HOOKPROC type defines a pointer
        /// to this callback function. CallWndProc is a placeholder for the application-defined
        /// or library-defined function name.
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
        /// <remarks>
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/callwndproc.asp
        /// </remarks>
        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern bool GetCursorPos(out PointStruct lpPoint);

        /// <summary>
        /// The SendInput function synthesizes keystrokes, mouse motions, and button clicks.
        /// </summary>
        /// <param name="numberOfInputs">Number of structures in the Inputs array.</param>
        /// <param name="inputs">Pointer to an array of InputStruct structures. Each structure represents an event to be inserted into the keyboard or mouse input stream.</param>
        /// <param name="sizeOfInputStructure">Specifies the size, in bytes, of an InputStruct structure. If cbSize is not the size of an InputStruct structure, the function fails.</param>
        /// <returns>The function returns the number of events that it successfully inserted into the keyboard or mouse input stream. If the function returns zero, the input was already blocked by another thread. To get extended error information, call GetLastError.Microsoft Windows Vista. This function fails when it is blocked by User Interface Privilege Isolation (UIPI). Note that neither GetLastError nor the return value will indicate the failure was caused by UIPI blocking.</returns>
        /// <remarks>
        /// Microsoft Windows Vista. This function is subject to UIPI. Applications are permitted to inject input only into applications that are at an equal or lesser integrity level.
        /// The SendInput function inserts the events in the InputStruct structures serially into the keyboard or mouse input stream. These events are not interspersed with other keyboard or mouse input events inserted either by the user (with the keyboard or mouse) or by calls to keybd_event, mouse_event, or other calls to SendInput.
        /// This function does not reset the keyboard's current state. Any keys that are already pressed when the function is called might interfere with the events that this function generates. To avoid this problem, check the keyboard's state with the GetAsyncKeyState function and correct as necessary.
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint numberOfInputs, InputStruct[] inputs, int sizeOfInputStructure);

        #endregion
    }
}
