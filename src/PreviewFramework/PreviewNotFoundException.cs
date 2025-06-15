using System;

namespace PreviewFramework.App;

public class PreviewNotFoundException : Exception
{
    public PreviewNotFoundException(string message) : base(message)
    {
    }
}
