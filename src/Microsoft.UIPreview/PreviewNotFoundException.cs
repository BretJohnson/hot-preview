using System;

namespace Microsoft.UIPreview.App;

public class PreviewNotFoundException : Exception
{
    public PreviewNotFoundException(string message) : base(message)
    {
    }
}
