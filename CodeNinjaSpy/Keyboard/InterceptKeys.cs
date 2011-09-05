using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MufflonoSoft.CodeNinjaSpy.Keyboard
{
    class InterceptKeys : IDisposable
    {
        public event EventHandler<KeyInterceptedEventArgs> KeyIntercepted;

        private const int WH_KEYBOARD_LL = 13;
        private static readonly IntPtr WM_KEYDOWN = (IntPtr)0x0100;
        private static readonly IntPtr WM_KEYUP = (IntPtr)0x0101;
        private static readonly IntPtr WM_SYSKEYDOWN = (IntPtr) 0x104;
        private static readonly IntPtr WM_SYSKEYUP = (IntPtr) 0x105;
        private LowLevelKeyboardProc _proc;
        private readonly IntPtr _hookID = IntPtr.Zero;
        private List<Keys> _pressedKeys = new List<Keys>();

        public InterceptKeys()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0)
            {
                var pressedKey = (Keys)Marshal.ReadInt32(lParam);

                if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
                {
                    if (!_pressedKeys.Contains(pressedKey))
                        _pressedKeys.Add(pressedKey);
                }
                else if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
                {
                    if (_pressedKeys.Contains(pressedKey))
                        _pressedKeys.Remove(pressedKey);
                }

                OnKeysIntercepted(_pressedKeys);
            }

            return CallNextHookEx(_hookID, code, wParam, lParam);
        }

        private void OnKeysIntercepted(List<Keys> pressedKeys)
        {
            var handler = KeyIntercepted;

            if (handler != null)
                handler(this, new KeyInterceptedEventArgs(pressedKeys));
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        #region DllImport

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }
}