using System;

namespace HotPreview.SharedModel;

public class UIComponentNotFoundException : Exception
{
    public UIComponentNotFoundException(string message) : base(message)
    {
    }
}
