using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace MufflonoSoft.CodeNinjaSpy.ViewModels
{
    /// <summary>
    /// BinaryFormatter within a Visual Studio Extension is somehow not possible.
    /// Wrote my own before investing a lot of time to find a solution...
    /// </summary>
    internal class CustomFormatter
    {
        public void Serialize(FileStream stream, List<Command> commands)
        {
            var serializedCommands = new StringBuilder();

            foreach (var command in commands)
            {
                serializedCommands.Append(command.Name);
                serializedCommands.Append(":");

                foreach (var binding in command.Bindings)
                {
                    serializedCommands.Append(binding);
                    serializedCommands.Append(",");
                }

                serializedCommands.Append(":");
                serializedCommands.Append(command.Guid);
                serializedCommands.Append(":");
                serializedCommands.Append(command.Id);
                serializedCommands.Append(";");
            }

            var bytes = Encoding.ASCII.GetBytes(serializedCommands.ToString());
            stream.Write(bytes, 0, bytes.Length);
        }

        public List<Command> Deserialize(FileStream stream)
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            var commands = new List<Command>();

            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            var content = Encoding.ASCII.GetString(buffer);
            var commandsAsString = content.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var commandAsString in commandsAsString)
            {
                var slicedCommand = commandAsString.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                var name = slicedCommand[0];
                var bindings = slicedCommand[1].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var guid = slicedCommand[2];
                var id = int.Parse(slicedCommand[3]);
                commands.Add(new Command(name, bindings, guid, id, dte.Events.CommandEvents[guid, id]));
            }

            return commands;
        }
    }
}