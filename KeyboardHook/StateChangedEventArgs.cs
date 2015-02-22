using System;
using CommonHookLibrary;

namespace KeyboardHook
{
    public class StateChangedEventArgs : EventArgs
    {
        private readonly HookState state;

        public HookState State
        {
            get
            {
                return this.state;
            }
        }

        public StateChangedEventArgs(HookState state)
        {
            this.state = state;
        }
    }
}