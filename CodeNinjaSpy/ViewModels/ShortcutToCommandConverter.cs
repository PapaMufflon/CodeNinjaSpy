using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using MufflonoSoft.CodeNinjaSpy.Logging;

namespace MufflonoSoft.CodeNinjaSpy.ViewModels
{
    internal class ShortcutToCommandConverter
    {
        public event EventHandler<CommandFetchingStatusUpdatedEventArgs> CommandFetchingStatusUpdated;
        public event EventHandler<CommandWithoutShortcutEventArgs> CommandWithoutShortcut;

        private static readonly string _commandsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CodeNinjaSpyCommands.dat");

        private readonly ILogger _logger;
        private List<Command> _commands = new List<Command>();
        private DTE2 _dte;

        public ShortcutToCommandConverter(ILogger logger)
        {
            _logger = logger;
            System.Threading.Tasks.Task.Factory.StartNew(FetchCommandsWithBindings);
        }

        private void FetchCommandsWithBindings()
        {
            OnCommandFetchingStatusUpdatedEventArgs(0, "", true);

            //if (!TryGetSerializedCommands())
            GetCommandsFromVisualStudio();

            OnCommandFetchingStatusUpdatedEventArgs(100, "", false);
        }

        private bool TryGetSerializedCommands()
        {
            try
            {
                var stream = File.Open(_commandsPath, FileMode.Open);
                var bFormatter = new CustomFormatter();
                _commands = bFormatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void GetCommandsFromVisualStudio()
        {
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
                        {
                            var commandEvent = _dte.Events.CommandEvents[command.Guid, command.ID];
                            commandEvent.AfterExecute += CommandExecuted;

                            _commands.Add(new Command(string.IsNullOrEmpty(command.Name) ? "no Name" : command.Name,
                                GetBindings(bindings),
                                command.Guid,
                                command.ID,
                                commandEvent));
                        }
                    }
                }
                catch (InvalidComObjectException)
                {
                    // user closed the window or closed Visual Studio.
                }
            }

            _logger.Log(string.Format("Loaded {0} commands.", _commands.Count));
            //SerializeCommands();

        }

        private void CommandExecuted(string guid, int id, object customin, object customout)
        {
            var command = _commands.Where(x => x.Guid == guid && x.Id == id).FirstOrDefault();

            if (command != null && command.Name != "Format.AlignBottoms")
                OnCommandWithoutShortcut(command);
        }

        private void OnCommandWithoutShortcut(Command command)
        {
            var handler = CommandWithoutShortcut;

            if (handler != null)
                handler(this, new CommandWithoutShortcutEventArgs(command));
        }

        private void SerializeCommands()
        {
            var stream = File.Open(_commandsPath, FileMode.Create);
            var bFormatter = new CustomFormatter();
            bFormatter.Serialize(stream, _commands);
            stream.Close();
        }

        private static List<string> GetBindings(IEnumerable<object> bindings)
        {
            var result = bindings.Select(binding => binding.ToString().IndexOf("::") >= 0
                ? binding.ToString().Substring(binding.ToString().IndexOf("::") + 2)
                : binding.ToString()).ToList();

            return result;
        }

        private void OnCommandFetchingStatusUpdatedEventArgs(double status, string statusText, bool isLoading)
        {
            var handler = CommandFetchingStatusUpdated;

            if (handler != null)
                handler(this, new CommandFetchingStatusUpdatedEventArgs(status, statusText, isLoading));
        }

        public bool TryGetCommand(List<List<Keys>> keyCombinations, List<Command> calledCommands)
        {
            if (keyCombinations.Select(a => a.Count).Sum() <= 1)
                return false;

            var pressedKeyBinding = ConvertToKeyBinding(keyCombinations);

            foreach (var command in _commands)
            {
                var commandMatchingKeyBinding = GetMatchingKeyBinding(command, pressedKeyBinding);
                if (commandMatchingKeyBinding != null)
                    calledCommands.Add(commandMatchingKeyBinding);
            }

            return calledCommands.Count > 0;
        }

        private static Command GetMatchingKeyBinding(Command command, string pressedKeyBinding)
        {
            foreach (var binding in command.Bindings)
            {
                if (binding.ToLower() == pressedKeyBinding)
                {
                    var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;

                    if (dte.Commands.Item(command.Name).IsAvailable)
                        return new Command(command.Name, new List<string> { pressedKeyBinding }, command.Guid, command.Id, null);
                }
            }

            return null;
        }

        internal static string ConvertToKeyBinding(List<List<Keys>> keyCombinations)
        {
            var keyBinding = "";

            foreach (var pressedKeysParameter in keyCombinations)
            {
                var pressedKeys = pressedKeysParameter.Select(k => k); // just copy
                var singleKeyBinding = "";

                // first ctrl
                if (pressedKeys.Any(IsControlKey))
                {
                    singleKeyBinding = "ctrl+";
                    pressedKeys = pressedKeys.Where(k => !IsControlKey(k));
                }

                // then shift
                if (pressedKeys.Any(IsShiftKey))
                {
                    singleKeyBinding += "shift+";
                    pressedKeys = pressedKeys.Where(k => !IsShiftKey(k));
                }

                // then alt
                if (pressedKeys.Any(IsAltKey))
                {
                    singleKeyBinding += "alt+";
                    pressedKeys = pressedKeys.Where(k => !IsAltKey(k));
                }

                singleKeyBinding = pressedKeys.Aggregate(singleKeyBinding, (current, pressedKey) => current + (KeyToString(pressedKey) + "+"));
                singleKeyBinding = singleKeyBinding.Substring(0, singleKeyBinding.Length - 1);

                keyBinding += singleKeyBinding + ", ";
            }

            return keyBinding.Substring(0, keyBinding.Length - 2);
        }

        private static string KeyToString(Keys pressedKey)
        {
            var result = "";

            switch (pressedKey)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    result = pressedKey + " arrow";
                    break;

                case Keys.Escape:
                    result = "esc";
                    break;

                case Keys.Insert:
                    result = "ins";
                    break;

                case Keys.PageUp:
                    result = "pgup";
                    break;

                case Keys.PageDown:
                    result = "pgdn";
                    break;

                case Keys.Delete:
                    result = "del";
                    break;

                case Keys.Return:
                    result = "enter";
                    break;

                default:
                    result = pressedKey.ToString();
                    break;
            }

            return result.ToLower();
        }

        public static bool IsControlKey(Keys key)
        {
            return (key == Keys.Control ||
                    key == Keys.ControlKey ||
                    key == Keys.LControlKey ||
                    key == Keys.RControlKey);
        }

        public static bool IsShiftKey(Keys key)
        {
            return (key == Keys.Shift ||
                    key == Keys.ShiftKey ||
                    key == Keys.LShiftKey ||
                    key == Keys.RShiftKey);
        }

        public static bool IsAltKey(Keys key)
        {
            return (key == Keys.Alt ||
                    key == Keys.LMenu);
        }
    }
}