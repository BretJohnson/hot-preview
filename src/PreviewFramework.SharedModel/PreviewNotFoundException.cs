using System;

namespace PreviewFramework.SharedModel;

public class PreviewNotFoundException : Exception
{
    public PreviewNotFoundException(string message) : base(message)
    {
    }
}
