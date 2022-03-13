using System.IO;

namespace Uninstaller;

internal sealed class SFile
    : SBase
{
    public SFile(string filePath)
    {
        FileContents = File.ReadAllText(filePath);
        Path = filePath;
    }

    public string FileContents { get; }
    public override string Path { get; }

    public override void Delete()
    {
        File.Delete(Path);
    }
}
