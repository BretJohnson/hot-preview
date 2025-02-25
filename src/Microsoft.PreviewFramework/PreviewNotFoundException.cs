using System;

namespace Microsoft.PreviewFramework.App;

public class PreviewNotFoundException : Exception
{
    public PreviewNotFoundException(string message) : base(message)
    {
    }
}
