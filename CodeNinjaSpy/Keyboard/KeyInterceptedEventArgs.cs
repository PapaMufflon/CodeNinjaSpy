using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MufflonoSoft.CodeNinjaSpy.Keyboard
{
    internal class KeyInterceptedEventArgs : EventArgs
    {
        public List<Keys> PressedKeys { get; set; }

        public KeyInterceptedEventArgs(List<Keys> pressedKeys)
        {
            PressedKeys = pressedKeys;
        }
    }
}