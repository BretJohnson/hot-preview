using System;

namespace HotPreview.SharedModel;

public class PreviewNotFoundException : Exception
{
    public PreviewNotFoundException(string message) : base(message)
    {
    }
}
