using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CDBuilder
{
    abstract class FileSystemComponent<T> : IComponent
        where T : FileSystemInfo
    {
        public long? Max { get; private set; }

        public long? Min { get; private set; }

        public void Handle(IEnumerable<string> args)
        {
            var dirs = this.CreateByArgs(args.Where(this.IsNotSetterArgs))
                .Select(z => new Tuple<long, T>(this.GetSize(z), z))
                .ToArray();

            if (this.Max.HasValue && this.Min.HasValue)
            {
                this.Calc(dirs, this.Max.Value, this.Min.Value);
            }
            else
            {
                Console.WriteLine("max and min args not found.");
            }

            Console.Read();
        }

        private bool IsNotSetterArgs(string arg)
        {
            var lower = arg.ToLower();
            if (lower.StartsWith("-max:"))
            {
                try
                {
                    var value = CalcValue(lower.Substring(5));
                    if (value.HasValue)
                    {
                        this.Max = value.Value;
                        return false;
                    }
                }
                catch (FormatException) { }
            }
            else if (lower.StartsWith("-min:"))
            {
                try
                {
                    var value = CalcValue(lower.Substring(5));
                    if (value.HasValue)
                    {
                        this.Min = value.Value;
                        return false;
                    }
                }
                catch (FormatException) { }
            }
            return true;
        }

        private static long? CalcValue(string text)
        {
            if (text.Length == 0) return null;
            return text
                .Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(z => z.Split(new[] { '*' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(long.Parse)
                    .Aggregate(1L, (current, value) => current * value))
                .Sum();
        }

        protected abstract IEnumerable<T> CreateByArgs(IEnumerable<string> args);

        protected void Calc(IEnumerable<Tuple<long, T>> args, long max, long min)
        {
            Console.WriteLine($"================== {min} ~ {max} =================");

            var dirs = args.Where(z =>
                {
                    if (z.Item1 < max) return true;
                    using (new ConsoleTempEnvironment())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"'{z.Item2.Name}' was > max value so it will be ignore.");
                    }
                    return false;
                })
                .GroupBy(z => this.GroupBy(z.Item2))
                .ToArray();
            if (dirs.Length == 0) return;

            var ordered = new List<Tuple<long, List<T>>>();
            foreach (var group in dirs)
            {
                ordered.AddRange(
                    JasilyIncrementBits.SelectItems(@group)
                        .Select(z => new Tuple<long, List<T>>(
                            z.Sum(x => x.Item1),
                            z.Select(x => x.Item2).ToList()))
                        .Where(z => max > z.Item1 && z.Item1 > min));
            }
            ordered = ordered.OrderByDescending(z => z.Item1).ToList();

            var dict = new Dictionary<int, List<T>>();
            var index = 0;
            foreach (var value in ordered)
            {
                dict[index] = value.Item2;
                Console.WriteLine();

                var better = max - value.Item1 < value.Item1 - min;
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
                if (index % 100 == 0)
                {
                    Console.WriteLine("... MORE");
                    Console.ReadKey();
                }
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
                        this.GroupBy(dict[inputValue].First()) + "x" + new Random().Next(100, 1000).ToString());
                    if (Directory.Exists(parent)) return;
                    Directory.CreateDirectory(parent);
                    foreach (var directoryInfo in dict[inputValue])
                    {
                        this.MoveTo(directoryInfo, Path.Combine(parent, directoryInfo.Name));
                    }
                }
            }
        }

        protected abstract string GroupBy(T item);

        protected abstract void MoveTo(T item, string newPath);

        protected abstract long GetSize(T item);
    }
}