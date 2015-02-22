using System;
using System.Runtime.InteropServices;

namespace UnmanagedLibrary.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KeyboardData
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }
}
