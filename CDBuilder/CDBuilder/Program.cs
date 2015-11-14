using System;
using System.Linq;
using System.Reflection;

namespace CDBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var coms = Assembly.GetEntryAssembly().DefinedTypes
                .Where(z => !z.IsGenericType && typeof(IComponent).IsAssignableFrom(z))
                .Select(z =>
                {
                    var component = (IComponent)z.GetConstructor(new Type[0])?.Invoke(new object[0]);
                    return component == null
                        ? null
                        : new Component(component,
                            z.GetCustomAttribute<CommandAttribute>() ?? new CommandAttribute(z.Name));
                })
                .Where(z => z != null)
                .ToArray();

            var first = args.FirstOrDefault();

            var com = coms.FirstOrDefault(z => "-" + z.Command.Command.ToLower() == first?.ToLower());

            if (com == null)
            {
                Console.WriteLine("parameter error. first parameter should be one of below:");
                foreach (var component in coms)
                {
                    Console.Write($"\"-{component.Command.Command.ToLower()}\" ");
                }
                Console.WriteLine();
                Console.Read();
                return;
            }
            else
            {
                com.Instance.Handle(args.Skip(1));
            }
        }
    }
}
