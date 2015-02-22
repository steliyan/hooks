using System.Windows.Forms;

namespace KeyboardHook
{
    public class KeyboardEventArgs : KeyEventArgs
    {
        private readonly Keys virtualKeyCode;

        new public bool SuppressKeyPress
        {
            get
            {
                return base.SuppressKeyPress;
            }
            set
            {
                base.SuppressKeyPress = value;
            }
        }

        public Keys VirtualKeyCode
        {
            get
            {
                return this.virtualKeyCode;
            }
        }

        public KeyboardEventArgs(Keys keyData, Keys virtualKeyCode)
            : base(keyData)
        {
            this.virtualKeyCode = virtualKeyCode;
        }
    }
}
