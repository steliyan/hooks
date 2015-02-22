using System;

namespace UnmanagedLibrary
{
    public static class NativeConstants
    {
        public const int HC_ACTION = 0;
        public const int WM_DESTROY = 0x0002;

        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_SYSKEYUP = 0x105;

        public const int WM_DRAWCLIPBOARD = 0x308;
        public const int WM_CHANGECBCHAIN = 0x30D;
    }
}
