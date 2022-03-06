namespace Uninstaller;

internal abstract class SBase
{
    public abstract void Delete();

    public abstract string Path { get; }
}
