namespace Radon.Runtime.RuntimeSystem;

public enum RadonVersion : ulong
{
    RADON_1_0_0 = 4607182418800017408,
    RADON_1_1_0 = 4607632778762754458,
    
    // The latest version of Radon.
    VER_AUTO,
    RADON_LATEST = VER_AUTO - 1,
}