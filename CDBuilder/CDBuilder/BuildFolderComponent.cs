using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CDBuilder
{
    [Command("build-folder-ref")]
    class BuildFolderComponent : IComponent
    {
        public void Handle(IEnumerable<string> args)
        {
            try
            {
                args.Where(Directory.Exists).Select(this.HandleFolder).ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private bool HandleFolder(string folderPath)
        {
            var destDir = folderPath + "_ref";
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            foreach (var folder in new DirectoryInfo(folderPath).EnumerateDirectories())
            {
                var size = GetSize(folder.FullName);
                var newPath = Path.Combine(destDir, folder.Name + "_" + size.ToString());
                if (!File.Exists(newPath))
                {
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                }
                else
                {
                    Console.WriteLine("file was exits : " + newPath);
                }
            }
            return false;
        }

        private static long GetSize(string folderPath)
        {
            var size = new DirectoryInfo(folderPath).EnumerateFiles().Sum(file => file.Length);
            size += new DirectoryInfo(folderPath).EnumerateDirectories().Sum(folder => GetSize(folder.FullName));
            return size;
        }
    }
}