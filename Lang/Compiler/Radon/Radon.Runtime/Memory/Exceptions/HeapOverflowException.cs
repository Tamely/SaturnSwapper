using System;

namespace Radon.Runtime.Memory.Exceptions;

internal sealed class HeapOverflowException : Exception
{
    public HeapOverflowException()
        : base("Operation caused a heap overflow.")
    {
    }
}