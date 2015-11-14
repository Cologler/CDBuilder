using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CDBuilder
{
    [Command("folder")]
    class FolderComponent : FileSystemComponent<DirectoryInfo>
    {
        protected override IEnumerable<DirectoryInfo> CreateByArgs(IEnumerable<string> args)
        {
            return args.Where(Directory.Exists)
                .Select(z => new DirectoryInfo(z))
                .SelectMany(z => z.EnumerateDirectories())
                .ToArray();
        }

        protected override string GroupBy(DirectoryInfo item) => item.Name[0].ToString().ToUpper();

        protected override void MoveTo(DirectoryInfo item, string newPath)
        {
            item.MoveTo(newPath);
        }

        protected override long GetSize(DirectoryInfo item)
        {
            long size = 0;
            foreach (var file in item.EnumerateFiles())
            {
                try
                {
                    size += file.Length;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"get file size error on '{Path.Combine(item.FullName, file.Name)}': " + e.Message);
                }
            }
            size += item.EnumerateDirectories().Sum(subDir => this.GetSize(subDir));
            return size;
        }
    }
}