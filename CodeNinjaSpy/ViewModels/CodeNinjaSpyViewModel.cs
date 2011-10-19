using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using MufflonoSoft.CodeNinjaSpy.Keyboard;
using MufflonoSoft.CodeNinjaSpy.Logging;

namespace MufflonoSoft.CodeNinjaSpy.ViewModels
{
    public class CodeNinjaSpyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _currentShortcut;
        private string _currentCommand;
        private string _lastShortcut;
        private string _lastCommand;
        private string _nextToLastShortcut;
        private string _nextToLastCommand;
        private double _status;
        private string _statusText;
        private bool _isLoading;
        private double _debugOpacity;
        private readonly InterceptKeys _keyInterceptor;
        private readonly ShortcutToCommandConverter _shortcutToCommandConverter;
        private readonly List<List<Keys>> _keyCombinations = new List<List<Keys>>();
        private readonly ILogger _logger = new SimpleLogger();

        public CodeNinjaSpyViewModel()
        {
            _shortcutToCommandConverter = new ShortcutToCommandConverter(_logger);
            _shortcutToCommandConverter.CommandFetchingStatusUpdated += (s, e) =>
            {
                Status = e.Status;
                StatusText = e.StatusText;
                IsLoading = e.IsLoading;
            };

            _shortcutToCommandConverter.CommandWithoutShortcut += (s, e) =>
            {
                UpdateShortcut(new List<Command> { e.Command });
            };

            _keyInterceptor = new InterceptKeys();
            _keyInterceptor.KeyIntercepted += (sender, eArgs) => TryGetCommand(eArgs.PressedKeys);

            _debugOpacity = 0.01;
        }

        private void TryGetCommand(ICollection<Keys> pressedKeys)
        {
            if (IsLoading)
                return;

            // a shortcut begins with at least two keys pressed simultaneously
            if ((_keyCombinations.Count == 0 && pressedKeys.Count <= 1) ||
                (pressedKeys.Where(x => !(ShortcutToCommandConverter.IsAltKey(x) ||
                    ShortcutToCommandConverter.IsControlKey(x) ||
                    ShortcutToCommandConverter.IsShiftKey(x))).Count() == 0))
                return;

            _keyCombinations.Add(pressedKeys.ToList());

            var commands = new List<Command>();
            if (_shortcutToCommandConverter.TryGetCommand(_keyCombinations, commands))
            {
                _keyCombinations.Clear();
                UpdateShortcut(commands);
            }
            else
            {
                _logger.Log("Cannot find a shortcut with these bindings: " + ShortcutToCommandConverter.ConvertToKeyBinding(_keyCombinations));
            }
        }

        private void UpdateShortcut(List<Command> commands)
        {
            var lastShortcut = commands[0].Bindings[0];
            if (lastShortcut.EndsWith("+"))
                lastShortcut = lastShortcut.Substring(0, lastShortcut.Length - 1);

            NextToLastShortcut = LastShortcut;
            NextToLastCommand = LastCommand;
            LastShortcut = CurrentShortcut;
            LastCommand = CurrentCommand;
            CurrentShortcut = lastShortcut;
            CurrentCommand = commands[0].Name;
        }

        private void NotifyOfPropertyChange(string property)
        {
            var handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }

        public ICommand ToggleDebugCommand
        {
            get { return new ToggleDebugCommand(this, _logger); }
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }

            set
            {
                NotifyOfPropertyChange("IsLoading");
                _isLoading = value;
            }
        }

        public string StatusText
        {
            get
            {
                return _statusText;
            }

            set
            {
                NotifyOfPropertyChange("StatusText");
                _statusText = value;
            }
        }

        public string CurrentShortcut
        {
            get { return _currentShortcut; }

            set
            {
                _currentShortcut = value;
                NotifyOfPropertyChange("CurrentShortcut");
            }
        }

        public double Status
        {
            get
            {
                return _status;
            }

            set
            {
                NotifyOfPropertyChange("Status");
                _status = value;
            }
        }

        public string CurrentCommand
        {
            get { return _currentCommand; }

            set
            {
                _currentCommand = value;
                NotifyOfPropertyChange("CurrentCommand");
            }
        }

        public string LastShortcut
        {
            get { return _lastShortcut; }

            set
            {
                _lastShortcut = value;
                NotifyOfPropertyChange("LastShortcut");
            }
        }

        public string LastCommand
        {
            get { return _lastCommand; }

            set
            {
                _lastCommand = value;
                NotifyOfPropertyChange("LastCommand");
            }
        }

        public string NextToLastShortcut
        {
            get { return _nextToLastShortcut; }

            set
            {
                _nextToLastShortcut = value;
                NotifyOfPropertyChange("NextToLastShortcut");
            }
        }

        public string NextToLastCommand
        {
            get { return _nextToLastCommand; }

            set
            {
                _nextToLastCommand = value;
                NotifyOfPropertyChange("NextToLastCommand");
            }
        }

        public double DebugOpacity
        {
            get { return _debugOpacity; }
            set
            {
                _debugOpacity = value;
                NotifyOfPropertyChange("DebugOpacity");
            }
        }
    }
}