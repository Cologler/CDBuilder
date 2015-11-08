using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CDBuilder
{
    class Program
    {
        private static readonly long Max = 24L * 1000 * 1000 * 1000;

        static void Main(string[] args)
        {
            var dirs = args
                .Where(Directory.Exists)
                .Select(z => new DirectoryInfo(z))
                .SelectMany(z => z.EnumerateDirectories())
                .Select(z => new Tuple<long, DirectoryInfo>(GetFolderSize(z), z))
                .Where(z =>
                {
                    if (z.Item1 >= Max) Console.WriteLine($"'{z.Item2.Name}' was > max value so it will be ignore.");
                    return z.Item1 < Max;
                })
                .OrderBy(z => z.Item1)
                .GroupBy(z => z.Item2.Name.ToUpper()[0])
                .ToArray();
            if (dirs.Length == 0) return;

            var ordered = new List<Tuple<long, List<DirectoryInfo>>>();
            foreach (var group in dirs)
            {
                ordered.AddRange(
                    JasilyIncrementBits.SelectItems(@group)
                        .Select(z => new Tuple<long, List<DirectoryInfo>>(
                            Max - z.Sum(x => x.Item1),
                            z.Select(x => x.Item2).ToList()))
                        .Where(z => z.Item1 > 0));
            }
            ordered = ordered.OrderBy(z => z.Item1).ToList();

            var first = ordered.FirstOrDefault();
            if (first != null)
            {
                Console.WriteLine($"total : {Max - first.Item1} => {Max}");
                foreach (var dir in first.Item2)
                {
                    Console.WriteLine(dir.Name);
                }
            }
            Console.Read();
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
