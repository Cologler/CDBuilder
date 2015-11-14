using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CDBuilder
{
    [Command("folder")]
    class FolderComponent : IComponent
    {
        public void Handle(IEnumerable<string> args)
        {
            var dirs = args
                .Where(Directory.Exists)
                .Select(z => new DirectoryInfo(z))
                .SelectMany(z => z.EnumerateDirectories())
                .Select(z => new Tuple<long, DirectoryInfo>(GetFolderSize(z), z))
                .ToArray();

            var max = new long[]
            {
                24200000000L,
                50L * 1000 * 1000 * 1000,
                100L * 1000 * 1000 * 1000
            };
            var result = max.Select(z => Calc(dirs, z)).ToArray();
            Console.Read();
        }

        static bool Calc(IEnumerable<Tuple<long, DirectoryInfo>> args, long max)
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

            var ordered = new List<Tuple<long, List<DirectoryInfo>>>();
            foreach (var group in dirs)
            {
                ordered.AddRange(
                    JasilyIncrementBits.SelectItems(@group)
                        .Select(z => new Tuple<long, List<DirectoryInfo>>(
                            z.Sum(x => x.Item1),
                            z.Select(x => x.Item2).ToList()))
                        .Where(z => max > z.Item1));
            }
            ordered = ordered.OrderByDescending(z => z.Item1).ToList();

            var dict = new Dictionary<int, List<DirectoryInfo>>();
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
                        directoryInfo.MoveTo(Path.Combine(
                            parent,
                            directoryInfo.Name));
                    }
                }
            }

            return true;
        }

        private static long GetFolderSize(DirectoryInfo dir)
        {
            long size = 0;
            foreach (var file in dir.EnumerateFiles())
            {
                try
                {
                    size += file.Length;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"get file size error on '{Path.Combine(dir.FullName, file.Name)}': " + e.Message);
                }
            }
            size += dir.EnumerateDirectories().Sum(subDir => GetFolderSize(subDir));
            return size;
        }
    }
}