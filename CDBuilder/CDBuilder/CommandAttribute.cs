using System;

namespace CDBuilder
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    class CommandAttribute : Attribute
    {
        public CommandAttribute(string command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            this.Command = command;
        }

        public string Command { get; set; }
    }
}