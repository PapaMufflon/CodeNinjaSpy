using System.Collections.Generic;

namespace MufflonoSoft.CodeNinjaSpy.ViewModels
{
    class Command
    {
        public string Name { get; private set; }
        public List<string> Bindings { get; private set; }

        public Command(string name, List<string> bindings)
        {
            Name = name;
            Bindings = bindings;
        }
    }
}