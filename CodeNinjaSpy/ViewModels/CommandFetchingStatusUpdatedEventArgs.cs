using System;

namespace MufflonoSoft.CodeNinjaSpy.ViewModels
{
    internal class CommandFetchingStatusUpdatedEventArgs : EventArgs
    {
        public double Status { get; set; }
        public string StatusText { get; set; }
        public bool IsLoading { get; set; }

        public CommandFetchingStatusUpdatedEventArgs(double status, string statusText, bool isLoading)
        {
            Status = status;
            StatusText = statusText;
            IsLoading = isLoading;
        }
    }
}