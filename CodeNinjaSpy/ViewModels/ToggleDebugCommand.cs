using System;
using System.Windows.Input;
using MufflonoSoft.CodeNinjaSpy.Logging;

namespace MufflonoSoft.CodeNinjaSpy.ViewModels
{
    internal class ToggleDebugCommand : ICommand
    {
        private readonly CodeNinjaSpyViewModel _codeNinjaSpyViewModel;
        private readonly ILogger _logger;

        public ToggleDebugCommand(CodeNinjaSpyViewModel codeNinjaSpyViewModel, ILogger logger)
        {
            _codeNinjaSpyViewModel = codeNinjaSpyViewModel;
            _logger = logger;
        }

        public void Execute(object parameter)
        {
            _logger.Debug = !_logger.Debug;
            _codeNinjaSpyViewModel.DebugOpacity = _logger.Debug ? 1 : 0.01;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}