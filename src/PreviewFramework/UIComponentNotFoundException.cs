using System;

namespace ExampleFramework.App;

public class UIComponentNotFoundException : Exception
{
    public UIComponentNotFoundException(string message) : base(message)
    {
    }
}
