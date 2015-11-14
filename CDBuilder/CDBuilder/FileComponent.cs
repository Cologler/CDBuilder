using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CDBuilder
{
    [Command("sha1file")]
    class FileComponent : FileSystemComponent<FileInfo>
    {
        protected override IEnumerable<FileInfo> CreateByArgs(IEnumerable<string> args)
        {
            return args.Where(Directory.Exists)
                .Select(z => new DirectoryInfo(z))
                .SelectMany(z => z.EnumerateFiles())
                .Where(z => z.Name.ToLower().StartsWith("[sha1."))
                .ToArray();
        }

        protected override void MoveTo(FileInfo item, string newPath)
        {
            item.MoveTo(newPath);
        }

        protected override long GetSize(FileInfo item)
        {
            return item.Length;
        }
    }
}