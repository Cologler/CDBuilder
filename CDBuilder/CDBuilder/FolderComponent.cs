using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CDBuilder
{
    [Command("folder")]
    class FolderComponent : FileSystemComponent<DirectoryInfo>
    {
        static readonly Regex RefName = new Regex(@"_(\d+)$");

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
            var match = RefName.Match(item.Name);
            return match.Success ? long.Parse(match.Groups[1].Value) : GetFolerSize(item);
        }

        internal static long GetFolerSize(DirectoryInfo folder)
        {
            var size = folder.EnumerateFiles().Sum(z => z.Length);
            size += folder.EnumerateDirectories().Sum(z => GetFolerSize(z));
            return size;
        }
    }
}