using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace MufflonoSoft.CodeNinjaSpy
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MyToolWindow))]
    [Guid(GuidList.guidCodeNinjaSpyPkgString)]
    public sealed class CodeNinjaSpyPackage : Package
    {
        private void ShowToolWindow(object sender, EventArgs e)
        {
            var window = this.FindToolWindow(typeof(MyToolWindow), 0, true);

            if ((null == window) || (null == window.Frame))
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            
            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        protected override void Initialize()
        {
            base.Initialize();

            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (null != mcs)
            {
                var toolwndCommandID = new CommandID(GuidList.guidCodeNinjaSpyCmdSet, (int)PkgCmdIDList.cmdidCodeNinjaSpy);
                var menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
            }
        }
    }
}