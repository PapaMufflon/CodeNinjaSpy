using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace MufflonoSoft.CodeNinjaSpy.ViewModels
{
    internal class ShortcutToCommandConverter
    {
        public event EventHandler<CommandFetchingStatusUpdatedEventArgs> CommandFetchingStatusUpdated;

        private DTE2 _dte;
        private readonly List<Command> _commands = new List<Command>();

        public ShortcutToCommandConverter()
        {
            System.Threading.Tasks.Task.Factory.StartNew(FetchCommandsWithBindings);
        }

        private void FetchCommandsWithBindings()
        {
            OnCommandFetchingStatusUpdatedEventArgs(0, "", true);

            _dte = Package.GetGlobalService(typeof(DTE)) as DTE2;

            if (_dte != null)
            {
                var numberOfCommands = (double)_dte.Commands.Count;
                var commandCounter = 0;

                try
                {
                    foreach (EnvDTE.Command command in _dte.Commands)
                    {
                        commandCounter++;
                        var status = commandCounter / numberOfCommands * 100.0;
                        var statusText = string.Format("Command {0} of {1}", commandCounter, numberOfCommands);
                        OnCommandFetchingStatusUpdatedEventArgs(status, statusText, true);

                        var bindings = command.Bindings as object[];

                        if (bindings != null && bindings.Length > 0)
                            _commands.Add(new Command(command.Name, (from b in bindings select b.ToString()).ToList()));
                    }
                }
                catch (InvalidComObjectException)
                {
                    // user closed the window or closed Visual Studio.
                }

                OnCommandFetchingStatusUpdatedEventArgs(100, "", false);
            }
        }

        private void OnCommandFetchingStatusUpdatedEventArgs(double status, string statusText, bool isLoading)
        {
            var handler = CommandFetchingStatusUpdated;

            if (handler != null)
                handler(this, new CommandFetchingStatusUpdatedEventArgs(status, statusText, isLoading));
        }

        public bool TryGetCommand(ICollection<Keys> pressedKeys, out Command calledCommand)
        {
            calledCommand = null;

            if (pressedKeys.Count > 1)
            {
                var pressedKeyBinding = ConvertToKeyBinding(pressedKeys);

                foreach (var command in _commands)
                {
                    foreach (var binding in command.Bindings)
                    {
                        var modifiedClosureProtection = binding;

                        if (pressedKeyBinding.All(pressedKey => modifiedClosureProtection.ToLower().Contains(pressedKey)))
                        {
                            calledCommand = new Command(command.Name, pressedKeyBinding);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static List<string> ConvertToKeyBinding(IEnumerable<Keys> pressedKeys)
        {
            var keyBinding = new List<string>();

            foreach (var pressedKey in pressedKeys)
            {
                switch (pressedKey)
                {
                    case Keys.Control:
                    case Keys.ControlKey:
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        keyBinding.Add("ctrl+");
                        break;

                    case Keys.Shift:
                    case Keys.ShiftKey:
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        keyBinding.Add("shift+");
                        break;

                    case Keys.Alt:
                    case Keys.LMenu:
                        keyBinding.Add("alt+");
                        break;

                    default:
                        keyBinding.Add(pressedKey.ToString().ToLower());
                        break;
                }
            }

            return keyBinding;
        }
    }
}