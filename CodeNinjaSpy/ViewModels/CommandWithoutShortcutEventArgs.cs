using System;

namespace MufflonoSoft.CodeNinjaSpy.ViewModels
{
    internal class CommandWithoutShortcutEventArgs : EventArgs
    {
        public Command Command { get; set; }

        public CommandWithoutShortcutEventArgs(Command command)
        {
            Command = command;
        }
    }
}