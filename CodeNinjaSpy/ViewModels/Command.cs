using System;
using System.Collections.Generic;
using EnvDTE;

namespace MufflonoSoft.CodeNinjaSpy.ViewModels
{
    [Serializable]
    class Command
    {
        public string Name { get; private set; }
        public List<string> Bindings { get; private set; }
        public string Guid { get; set; }
        public int Id { get; set; }

        private readonly CommandEvents _commandEvent;

        public Command(string name, List<string> bindings, string guid, int id, CommandEvents commandEvent)
        {
            _commandEvent = commandEvent;
            Name = name;
            Bindings = bindings;
            Guid = guid;
            Id = id;
        }
    }
}