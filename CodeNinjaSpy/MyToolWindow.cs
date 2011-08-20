using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace MufflonoSoft.CodeNinjaSpy
{
    [Guid("62fc0bfd-d05f-44c1-8170-8c5a1f1a5b3e")]
    public class MyToolWindow : ToolWindowPane
    {
        public MyToolWindow() : base(null)
        {
            this.Caption = Resources.ToolWindowTitle;
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            base.Content = new CodeNinjaSpyShell();
        }
    }
}