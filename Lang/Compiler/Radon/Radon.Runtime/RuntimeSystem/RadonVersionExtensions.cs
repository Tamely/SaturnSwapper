namespace Radon.Runtime.RuntimeSystem;

public static class RadonVersionExtensions
{
    public static unsafe double GetVersion(this RadonVersion version)
    {
        return *(double*)&version;
    }
}