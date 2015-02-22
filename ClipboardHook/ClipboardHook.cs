using System;
using System.ComponentModel;
using System.Windows.Forms;
using CommonHookLibrary;
using UnmanagedLibrary;

[assembly: CLSCompliant(true)]
namespace ClipboardHook
{
    public class ClipboardHook
    {
        private Clipboard clipboard;

        public HookState State
        {
            get
            {
                if (this.clipboard == null || this.clipboard.Handle == IntPtr.Zero)
                {
                    return HookState.Uninstalled;
                }

                return HookState.Installed;
            }
        }

        public event EventHandler<ClipboardEventArgs> ClipboardChanged;

        public void InstallHook(Form window)
        {
            if (this.State == HookState.Uninstalled)
            {
                if (window != null)
                {
                    NativeExports.SetLastError(0);
                    IntPtr nextWindow = NativeExports.SetClipboardViewer(window.Handle);
                    if (nextWindow == IntPtr.Zero)
                    {
                        var errorCode = NativeExports.GetLastError();
                        if (errorCode == 0)
                        {
                            OnSuccessfullHook(window.Handle, nextWindow);
                        }
                        else
                        {
                            var message = ErrorCodeHelper.GetMessage(errorCode);
                            throw new Exception(message);
                        }
                    }
                    else
                    {
                        OnSuccessfullHook(window.Handle, nextWindow);
                    }
                }
            }
        }

        private void OnSuccessfullHook(IntPtr window, IntPtr nextWindow)
        {
            this.clipboard = new Clipboard(nextWindow);
            this.clipboard.WindowClosing += new EventHandler(ClipboardProcWindowClosing);
            this.clipboard.ClipboardChanged += new EventHandler<ClipboardEventArgs>(ClipboardProcClipboardChanged);
            this.clipboard.AssignHandle(window);
        }

        private void OnSuccessfullUnhook()
        {
            this.clipboard.ReleaseHandle();
            this.clipboard.WindowClosing -= new EventHandler(ClipboardProcWindowClosing);
            this.clipboard.ClipboardChanged -= new EventHandler<ClipboardEventArgs>(ClipboardProcClipboardChanged);
            this.clipboard = null;
        }

        public void RemoveHook()
        {
            if (this.State == HookState.Installed)
            {
                NativeExports.SetLastError(0);
                NativeExports.ChangeClipboardChain(this.clipboard.Handle, this.clipboard.NextWindow);
                var errorCode = NativeExports.GetLastError();
                if (errorCode == 0)
                {
                    this.OnSuccessfullUnhook();
                }
                else
                {
                    var message = ErrorCodeHelper.GetMessage(errorCode);
                    throw new Exception(message);
                }
            }
        }

        private void SafeRemove()
        {
            if (this.State == HookState.Installed)
            {
                NativeExports.ChangeClipboardChain(this.clipboard.Handle, this.clipboard.NextWindow);
                this.OnSuccessfullUnhook();
            }
        }

        private void ClipboardProcClipboardChanged(object sender, ClipboardEventArgs e)
        {
            this.OnClipboardChanged(e);
        }

        private void ClipboardProcWindowClosing(object sender, EventArgs e)
        {
            this.SafeRemove();
        }

        protected virtual void OnClipboardChanged(ClipboardEventArgs e)
        {
            if (ClipboardChanged != null)
                ClipboardChanged(this, e);
        }

        private class Clipboard : NativeWindow
        {
            private IntPtr nextWindow;

            public event EventHandler HandleChanged;
            public event EventHandler WindowClosing;
            public event EventHandler<ClipboardEventArgs> ClipboardChanged;

            public IntPtr NextWindow
            {
                get
                {
                    return this.nextWindow;
                }
                set
                {
                    this.nextWindow = value;
                }
            }

            public Clipboard(IntPtr nextWindow)
            {
                this.nextWindow = nextWindow;
            }

            protected override void OnHandleChange()
            {
                base.OnHandleChange();

                if (this.Handle == IntPtr.Zero)
                    this.nextWindow = IntPtr.Zero;

                if (this.HandleChanged != null)
                    this.HandleChanged(this, EventArgs.Empty);
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case NativeConstants.WM_DRAWCLIPBOARD:
                        {
                            if (ClipboardChanged != null)
                                ClipboardChanged(this, new ClipboardEventArgs(m.WParam));

                            NativeExports.SendMessage(this.nextWindow, (uint)m.Msg, m.WParam, m.LParam);
                        }
                        break;
                    case NativeConstants.WM_CHANGECBCHAIN:
                        {
                            if (m.WParam == this.nextWindow)
                            {
                                // The window is being removed is the next window on the clipboard chain.
                                // Change the ClipboardHook._nextWind handle with LParam.
                                // There is no need to pass this massage any farther.
                                this.nextWindow = m.LParam;
                            }
                            else
                            {
                                NativeExports.SendMessage(this.nextWindow, (uint)m.Msg, m.WParam, m.LParam);
                            }
                        }
                        break;
                    default:
                        {
                            if (m.Msg == NativeConstants.WM_DESTROY && WindowClosing != null)
                                WindowClosing(this, EventArgs.Empty);

                            base.WndProc(ref m);
                        }
                        break;
                }
            }
        }
    }
}
