namespace MufflonoSoft.CodeNinjaSpy.Logging
{
    internal interface ILogger
    {
        bool Debug { get; set; }
        void Log(string message);
    }
}