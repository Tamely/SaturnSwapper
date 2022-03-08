using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Uninstaller;

internal sealed class DirectoryTree
{
    private DirectoryTree(SDirectory baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public static async Task<DirectoryTree> CreateDirectoryTreeAsync(string parentDirectory)
    {
        return await Task.Run(async () =>
        {
            return new DirectoryTree(await SDirectory.CreateDirectoryAsync(parentDirectory));
        });
    }

    /*
       Directory Tree:

            DRoot
             |
            / \
         ND1   ND2
          |
         / \
     ND1:1  ND1:2
              |
             / \
      ND1:2:1   ND1:2:2
                   |
               ND1:2:2:1 // Last node
        
    */
    public SDirectory BaseDirectory { get; }

    private static List<SDirectory> FlattenDirectory(in SDirectory directory)
    {
        /*
            Flattened Directory:

            DRoot
            ND1
            ND2
            ND1:1
            ND1:2
            ND1:2:1
            ND1:2:2
            ND1:2:2:1
        */
        var list = new List<SDirectory> { directory };
        foreach (var child in directory.Children)
        {
            list.AddRange(FlattenDirectory(child));
        }

        return list;
    }

    public IEnumerable<SDirectory> GetNestedDirectories()
    {
        var directories = FlattenDirectory(BaseDirectory);
        directories.RemoveAt(0);
        return directories;
    }

    // I don't know if this will be needed, so for now, it will just be here and not compile
#if false
    public IEnumerable<SDirectory> GetNestedDirectories(string[] partialDirectories, int option = 0)
    {
        var dirs = GetNestedDirectories().ToList();
        var targetDirs = new List<SDirectory>();
        if (partialDirectories is not null &&
            partialDirectories != Array.Empty<string>())

        {
            foreach (var path in partialDirectories)
            {
                for (var i = 0; i < dirs.Count(); i++)
                {
                    var dir = dirs[i];
                    if (option == 0 &&
                        dir.Path.Contains(path))
                    {
                        dirs.RemoveAt(i);
                    }
                    else if (option == 1 &&
                             dir.Path.Contains(path))
                    {
                        targetDirs.Add(dir);
                    }
                }
            }
        }
        else
        {
            option = 0;
        }

        return option == 0 ? dirs : targetDirs;
    }
#endif

    public SDirectory GetNestedDirectory(string partialPath)
    {
        var root = BaseDirectory;
        var directories = FlattenDirectory(root);
        foreach (var dir in directories)
        {
            if (dir.Path.Contains(partialPath))
            {
                return dir;
            }
        }

        return null;
    }

    public IEnumerable<SFile> GetFiles(Func<SFile, bool> predicate)
    {
        var files = GetFiles();
        var validFiles = files.Where(predicate);
        return validFiles;
    }

    public SFile GetFile(string partialPath)
    {
        var stack = new Stack<SDirectory>();
        stack.Push(BaseDirectory);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node.Children.Any())
            {
                foreach (var child in node.Children)
                {
                    stack.Push(child);
                }
            }

            foreach (var file in node.Files)
            {
                if (file.Path.Contains(partialPath))
                {
                    return file;
                }
            }
        }

        return null;
    }

    public IEnumerable<SFile> GetFiles() => GetFiles(Array.Empty<string>());

    public IEnumerable<SFile> GetFiles(string[] partialPaths, int option = 0 /* 0 means remove, 1 means only find*/)
    {
        var files = new List<SFile>();
        var stack = new Stack<SDirectory>();
        stack.Push(BaseDirectory);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node.Children.Any())
            {
                foreach (var child in node.Children)
                {
                    stack.Push(child);
                }
            }

            foreach (var file in node.Files)
            {
                files.Add(file);
            }
        }

        var targetFiles = new List<SFile>();

        if (partialPaths is not null &&
            partialPaths != Array.Empty<string>())

        {
            foreach (var path in partialPaths)
            {
                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    if (option == 0 &&
                        file.Path.Contains(path))
                    {
                        files.RemoveAt(i);
                        break;
                    }
                    else if (option == 1 &&
                             file.Path.Contains(path))
                    {
                        targetFiles.Add(file);
                        break;
                    }
                }
            }
        }
        else
        {
            option = 0;
        }

        return option == 0 ? files : targetFiles;
    }
}
