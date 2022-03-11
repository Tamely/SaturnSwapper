using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Uninstaller;

internal sealed class SDirectory
    : SBase
{
    private SDirectory(string directoryPath)
    {
        Path = directoryPath;
        if (!Directory.Exists(Path))
        {
            Children = Enumerable.Empty<SDirectory>();
            Files = Enumerable.Empty<SFile>();
            return;
        }

        var nestedDirectories = Directory.GetDirectories(directoryPath);
        var children = new List<SDirectory>();
        foreach (var nestedDirectory in nestedDirectories)
            children.Add(new SDirectory(nestedDirectory));

        Children = children;
        var files = Directory.GetFiles(directoryPath);
        var wndFiles = new List<SFile>();
        foreach (var file in files)
            wndFiles.Add(new SFile(file));

        Files = wndFiles;
    }

    public override string Path { get; }
    public IEnumerable<SDirectory> Children { get; set; }
    public IEnumerable<SFile> Files { get;  set; }

    private IEnumerable<SDirectory> Flatten(SDirectory directory)
    {
        var list = new List<SDirectory> { directory };
        foreach (var child in directory.Children)
        {
            list.AddRange(Flatten(child));
        }

        return list;
    }

    public override void Delete()
    {
        Directory.Delete(Path, true);
    }

    public static async Task<SDirectory> CreateDirectoryAsync(string directoryPath)
    {
        return await Task.Run(() =>
        {
            return new SDirectory(directoryPath);
        });
    }
}
