using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CommonHookLibrary;
using UnmanagedLibrary;
using UnmanagedLibrary.Structs;

namespace KeyboardHook
{
    public class KeyboardHook
    {
        delegate IntPtr KeyboardMessageEventHandler(Int32 nCode, IntPtr wParam, ref KeyboardData lParam);

        public event EventHandler<StateChangedEventArgs> StateChanged;
        public event EventHandler<KeyboardEventArgs> KeyDown;
        public event EventHandler<KeyboardEventArgs> KeyUp;


        [MarshalAs(UnmanagedType.FunctionPtr)]
        private KeyboardMessageEventHandler keyboardProcessor;
        private IntPtr handle;
        private Keys keyData;

        public bool AltKeyDown
        {
            get
            {
                return (this.keyData & Keys.Alt) == Keys.Alt;
            }
        }

        public bool CtrlKeyDown
        {
            get
            {
                return (this.keyData & Keys.Control) == Keys.Control;
            }
        }

        public bool ShiftKeyDown
        {
            get
            {
                return (this.keyData & Keys.Shift) == Keys.Shift;
            }
        }

        public HookState State
        {
            get
            {
                if (this.handle == IntPtr.Zero)
                    return HookState.Uninstalled;

                return HookState.Installed;
            }
        }

        public void InstallHook(Form window)
        {
            if (this.State == HookState.Uninstalled)
            {
                this.keyboardProcessor = new KeyboardMessageEventHandler(KeyboardProc);
                this.handle = NativeExports.SetWindowsHookEx(NativeConstants.WH_KEYBOARD_LL, this.keyboardProcessor, IntPtr.Zero, 0);
                if (this.handle == IntPtr.Zero)
                {
                    this.keyboardProcessor = null;
                    var errorCode = NativeExports.GetLastError();
                    throw new Exception(ErrorCodeHelper.GetMessage(errorCode));
                }
                else
                {
                    this.OnStateChanged(new StateChangedEventArgs(this.State));
                }
            }
        }

        public void RemoveHook()
        {
            if (this.State == HookState.Installed)
            {
                if (NativeExports.UnhookWindowsHookEx(this.handle))
                {
                    this.OnSuccessfullUnhook();
                }
                else
                {
                    var errorCode = NativeExports.GetLastError();
                    throw new Exception(ErrorCodeHelper.GetMessage(errorCode));
                }
            }
        }

        private void OnSuccessfullUnhook()
        {
            this.keyboardProcessor = null;
            this.handle = IntPtr.Zero;
            this.keyData = Keys.None;
            this.OnStateChanged(new StateChangedEventArgs(this.State));
        }


        private void SafeRemove()
        {
            if (this.handle != IntPtr.Zero)
            {
                NativeExports.UnhookWindowsHookEx(this.handle);
                this.OnSuccessfullUnhook();
            }
        }

        private IntPtr KeyboardProc(int nCode, IntPtr wParam, ref KeyboardData lParam)
        {
            if (nCode >= NativeConstants.HC_ACTION)
            {
                KeyboardEventArgs e;
                var vkCode = (Keys)lParam.vkCode;
                if ((int)wParam == NativeConstants.WM_KEYDOWN | (int)wParam == NativeConstants.WM_SYSKEYDOWN)
                {
                    if (vkCode == Keys.LMenu | vkCode == Keys.RMenu)
                    {
                        this.keyData = (this.keyData | Keys.Alt);
                        e = new KeyboardEventArgs(this.keyData | Keys.Menu, vkCode);
                    }
                    else if (vkCode == Keys.LControlKey | vkCode == Keys.RControlKey)
                    {
                        this.keyData = (this.keyData | Keys.Control);
                        e = new KeyboardEventArgs(this.keyData | Keys.ControlKey, vkCode);
                    }
                    else if (vkCode == Keys.LShiftKey | vkCode == Keys.RShiftKey)
                    {
                        this.keyData = (this.keyData | Keys.Shift);
                        e = new KeyboardEventArgs(this.keyData | Keys.ShiftKey, vkCode);
                    }
                    else
                    {
                        e = new KeyboardEventArgs(this.keyData | vkCode, vkCode);
                    }

                    this.OnKeyDown(e);
                    if (e.Handled)
                        return new IntPtr(1);
                }
                else if ((int)wParam == NativeConstants.WM_KEYUP | (int)wParam == NativeConstants.WM_SYSKEYUP)
                {
                    if (vkCode == Keys.LMenu | vkCode == Keys.RMenu)
                    {
                        this.keyData = (this.keyData & ~Keys.Alt);
                        e = new KeyboardEventArgs(this.keyData | Keys.Menu, vkCode);
                    }
                    else if (vkCode == Keys.LControlKey | vkCode == Keys.RControlKey)
                    {
                        this.keyData = (this.keyData & ~Keys.Control);
                        e = new KeyboardEventArgs(this.keyData | Keys.ControlKey, vkCode);
                    }
                    else if (vkCode == Keys.LShiftKey | vkCode == Keys.RShiftKey)
                    {
                        this.keyData = (this.keyData & ~Keys.Shift);
                        e = new KeyboardEventArgs(this.keyData | Keys.ShiftKey, vkCode);
                    }
                    else
                    {
                        e = new KeyboardEventArgs(this.keyData | vkCode, vkCode);
                    }

                    this.OnKeyUp(e);
                    if (e.Handled)
                        return new IntPtr(1);
                }
            }

            return NativeExports.CallNextHookEx(this.handle, nCode, wParam, ref lParam);
        }

        protected virtual void OnStateChanged(StateChangedEventArgs e)
        {
            if (StateChanged != null)
                StateChanged(this, e);
        }

        protected virtual void OnKeyUp(KeyboardEventArgs e)
        {
            if (KeyUp != null)
                KeyUp(this, e);
        }

        protected virtual void OnKeyDown(KeyboardEventArgs e)
        {
            if (KeyDown != null)
                KeyDown(this, e);
        }
    }
}
