using System;

namespace Radon.Ide.Backend.Core;

public sealed class IdAttribute : Attribute
{
    public string Id { get; }

    public IdAttribute(string id)
    {
        Id = id;
    }
}