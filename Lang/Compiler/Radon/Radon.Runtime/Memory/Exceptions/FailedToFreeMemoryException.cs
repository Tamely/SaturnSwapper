using System;

namespace Radon.Runtime.Memory.Exceptions;

internal sealed class FailedToFreeMemoryException : Exception
{
    public FailedToFreeMemoryException()
        : base("Failed to free memory.")
    {
    }
}