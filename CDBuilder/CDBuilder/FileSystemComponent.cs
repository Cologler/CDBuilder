using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CDBuilder
{
    abstract class FileSystemComponent<T> : IComponent
        where T : FileSystemInfo
    {
        public void Handle(IEnumerable<string> args)
        {
            var dirs = this.CreateByArgs(args)
                .Select(z => new Tuple<long, T>(this.GetSize(z), z))
                .ToArray();

            var max = new long[]
            {
                24200000000L,
                50L * 1000 * 1000 * 1000,
                100L * 1000 * 1000 * 1000
            };
            var result = max.Select(z => this.Calc(dirs, z)).ToArray();
            Console.Read();
        }

        protected abstract IEnumerable<T> CreateByArgs(IEnumerable<string> args);

        protected bool Calc(IEnumerable<Tuple<long, T>> args, long max)
        {
            Console.WriteLine($"================== {max} =================");

            var dirs = args
                .Where(z =>
                {
                    if (z.Item1 >= max)
                    {
                        using (new ConsoleTempEnvironment())
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"'{z.Item2.Name}' was > max value so it will be ignore.");
                        }
                    }
                    return z.Item1 < max;
                })
                .GroupBy(z => z.Item2.Name.ToUpper()[0])
                .ToArray();
            if (dirs.Length == 0) return false;

            var ordered = new List<Tuple<long, List<T>>>();
            foreach (var group in dirs)
            {
                ordered.AddRange(
                    JasilyIncrementBits.SelectItems(@group)
                        .Select(z => new Tuple<long, List<T>>(
                            z.Sum(x => x.Item1),
                            z.Select(x => x.Item2).ToList()))
                        .Where(z => max > z.Item1));
            }
            ordered = ordered.OrderByDescending(z => z.Item1).ToList();

            var dict = new Dictionary<int, List<T>>();
            var index = 0;
            foreach (var value in ordered.Where(z => z.Item1 > max * 0.95))
            {
                dict[index] = value.Item2;
                Console.WriteLine();

                var better = value.Item1 < max * 0.975;
                using (new ConsoleTempEnvironment())
                {
                    Console.ForegroundColor = better ? ConsoleColor.Green : ConsoleColor.Yellow;
                    Console.WriteLine($"[{index}] total : {value.Item1} => {max} * {value.Item1 / (double)max}");
                    foreach (var dir in value.Item2)
                    {
                        Console.WriteLine(dir.Name);
                    }
                }

                index++;
            }
            Console.WriteLine();

            if (dict.Count > 0)
            {
                Console.WriteLine("do you want to move file into :" + Environment.CurrentDirectory);
                var input = Console.ReadLine();
                int inputValue;
                if (int.TryParse(input, out inputValue) && dict.ContainsKey(inputValue))
                {
                    var parent = Path.Combine(Environment.CurrentDirectory,
                        dict[inputValue].First().Name.ToUpper()[0].ToString() + "x");
                    if (Directory.Exists(parent)) return true;
                    Directory.CreateDirectory(parent);
                    foreach (var directoryInfo in dict[inputValue])
                    {
                        this.MoveTo(directoryInfo, Path.Combine(parent, directoryInfo.Name));
                    }
                }
            }

            return true;
        }

        protected abstract void MoveTo(T item, string newPath);

        protected abstract long GetSize(T item);
    }
}