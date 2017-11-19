using System.Windows;
using System.Windows.Media;
using MufflonoSoft.CodeNinjaSpy.ViewModels;

namespace MufflonoSoft.CodeNinjaSpy.Views
{
    public partial class CodeNinjaSpyShell
    {
        public CodeNinjaSpyShell()
        {
            InitializeComponent();
            DataContext = new CodeNinjaSpyViewModel();
        }
    }
}