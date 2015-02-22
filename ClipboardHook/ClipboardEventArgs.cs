using System;

namespace ClipboardHook
{
    public class ClipboardEventArgs : EventArgs
    {
        private readonly IntPtr sourceWindow;

        public IntPtr SourceWindow
        {
            get
            {
                return this.sourceWindow;
            }
        }

        public ClipboardEventArgs(IntPtr sourceWindow)
        {
            this.sourceWindow = sourceWindow;
        }
    }
}
