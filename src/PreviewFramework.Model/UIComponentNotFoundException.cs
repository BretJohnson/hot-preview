using System;

namespace PreviewFramework.Model;

public class UIComponentNotFoundException : Exception
{
    public UIComponentNotFoundException(string message) : base(message)
    {
    }
}
