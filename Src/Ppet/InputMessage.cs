using System.Runtime.InteropServices;

namespace Ppet
{
    public static class InputMessage
    {
        public const byte KeyDown    = 0;
        public const byte KeyUp      = 1;
        public const byte MouseLDown = 2;
        public const byte MouseLUp   = 3;
        public const byte MouseRDown = 4;
        public const byte MouseRUp   = 5;
        public const byte MouseMDown = 6;
        public const byte MouseMUp   = 7;
        public const byte MouseWheel = 8;

        public static byte[] GetBytes<T>(T obj) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            var arr  = new byte[size];
            var ptr  = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public static T FromBytes<T>(byte[] arr, int startIndex, out int size) where T : struct
        {
            size = Marshal.SizeOf<T>();
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(arr, startIndex, ptr, size);
            var obj = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return obj;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct KeyboardMessage
    {
        public byte Type;
        public WinApi.VirtualKey Key;
        public uint Flags;
        public uint ScanCode;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct MouseMessage
    {
        public byte Type;
        public MouseButton Button;
        public int X;
        public int Y;
        public short WheelDelta;
    }
}
